# Frontend Architecture

**Written by:** @clintonbampoe
**Last updated:** 2026-10-14 by @clintonbampoe

This page explains how the Echo web app is organised and the rules that keep it easy to read. Every
file should be small, do one thing, and be easy to find. The rules below are checked automatically
where possible, so a build fails when one is broken.

## Overview

- React 19, TypeScript, Vite.
- Server data is handled by TanStack Query, forms by React Hook Form and Zod, routing by React Router.
- Styling is CSS Modules plus one file of design tokens.
- Tests use Vitest and Testing Library.

## The four layers

The code is split into four layers. A layer may import from the layers below it, never from the
ones above. ESLint fails the build if this is broken.

```
app        wiring: providers, router, page shell
 ↓
features   one folder per area of the product (auth, members, finance, ...)
 ↓
ui         shared building blocks: props in, markup out
 ↓
lib        plain TypeScript: API client, config, helpers
```

| Layer | May import from | Notes |
|---|---|---|
| `lib/` | nothing in the app | No React components. |
| `ui/` | `lib/` | No data, no routes, no API calls. If a component needs data, it belongs in a feature. |
| `features/` | `ui/`, `lib/` | A feature may use another feature only through that feature's `index.ts`. No cycles. |
| `app/` | everything | Nothing imports from `app/`. |

## Folder layout

```
frontend/src/
  app/         App.tsx, routes.tsx, queryClient.ts, the page shell (sidebar, top bar)
  features/    landing/  auth/  dashboard/  members/  visitors/  organizations/
               attendance/  events/  tithes/  finance/  projects/  assets/  invitations/
  ui/          Button, Field, Icon, Logo, Quote, Reveal, Alert ... and index.ts
  lib/         config.ts  tokens.ts  api/ (client, errors, page)  verses.ts ...
  styles/      tokens.css  reset.css  motion.css  fonts.css  index.css
  test/        setup.ts
```

## Inside a feature

```
features/members/
  api.ts         HTTP calls. Plain async functions, no React.
  hooks.ts       TanStack Query hooks and the query keys. The only file that touches the cache.
  schema.ts      Zod schemas for forms.
  columns.ts     How a row is shown in the list.
  format.ts      Small formatting helpers.
  options.ts     Fixed dropdown lists.
  pages/         One file per route. Pages only put pieces together (about 30-60 lines).
  components/    Pieces used by this feature's pages.
  index.ts       What other features may import (often nothing).
```

Read a feature in this order: `api` → `hooks` → `pages`. A page calls hooks and renders components.
It never calls `fetch` directly, and a component never decides what to fetch.

## Where state lives

State has exactly three homes.

| Kind of state | Home | Example |
|---|---|---|
| Server data | TanStack Query cache | the members list |
| What the user is looking at | the URL (route and search params) | filters, date range, selected tab |
| Short-lived UI state | `useState` | is this menu open, is the password visible |

There is no global store. If two distant components need the same thing, it is either server data
(read it from the cache) or URL state.

## Lists and pagination

Lists are cursor-paged. A response looks like `{ data, hasMore, next }` and has **no total**.

- Use `useInfiniteQuery` with `nextCursor` from `lib/api`.
- The footer counts what has loaded: "Showing 50 members. Scroll to load more." When `hasMore` is
  false: "Showing all 50 members."
- A real total (such as a headline "1,284 members") comes from the module's `/summary` endpoint.
  Never count rows or add up money in the browser.

## Errors and screen states

- Every failed request becomes an `ApiError` (`lib/api/errors.ts`) with `status`, `errorCode`,
  `message` and `fieldErrors`. Screens check `errorCode`, never the message text.
- Forms show `fieldErrors` under their fields. Every other error appears in an `Alert`.
- A screen that shows data has four states: loading (skeleton), empty, no results, and error.
  Leaving one out is a bug.

## Styling

- Colours, radii and durations are tokens in `styles/tokens.css`. Components use `var(--text)`,
  never a hex value.
- Each component has its own `Name.module.css` next to it, about 100 lines at most. There are no
  global component classes. Shared animations are in `styles/motion.css` (`anim-rise`, `anim-fade`,
  and the `Reveal` component).
- There are two surfaces. The signed-in app is light. Public pages (landing and auth) are dark,
  requested with `useSurface('dark')`. Dark mode for the app itself is not planned yet.
- Motion is subtle: things settle into place and nothing bounces. `prefers-reduced-motion` turns
  all motion off with one rule in `reset.css`, so never add animation that bypasses it.

## Code rules

- Plain functions and named exports. No default exports.
- `type` instead of `interface`, string unions instead of enums, no classes (`ApiError` is the one
  exception), no `any`.
- Return early instead of nesting. Work values out while rendering instead of syncing them with
  `useEffect`. Do not use `useMemo` or `useCallback` unless a measurement shows they help.
- Dates stay as `YYYY-MM-DD` strings. Do not convert them through `new Date()`.
- Aim for components of 150 lines or fewer. Past that, split them.

| Rule | Checked by |
|---|---|
| Layer direction, features import each other only through `index.ts` | ESLint `no-restricted-imports` |
| File under 250 lines (error), function under 100 lines (warning), complexity at most 10 (error) | ESLint |
| No default exports, enums, classes, `console.log`, `dangerouslySetInnerHTML`, `useMemo`, `useCallback` | ESLint `no-restricted-syntax` |
| Strict types, no unchecked array access | `tsconfig.app.json` |
| Config is present and valid at startup | `lib/config.ts` (Zod) |
| `lib/` and `ui/` behaviour | Vitest tests next to the code |

"Complexity" counts the branches in a function (`if`, `&&`, `?:`, loops, `case`). More than 10 usually
means the function should be split.

## Routing

- All URLs are listed in `app/routes.tsx`. Each feature owns its own routes and exports them.
- Pages load on demand (`lazy`). The whole list sits under one root route that supplies an empty
  `HydrateFallback`, because React Router warns without one when the first page is lazy-loaded.
- Create and edit forms are routes (`/members/new`, `/members/:id/edit`). They show as a drawer on
  desktop and a full screen on small screens. Confirm dialogs and row menus are local state.
- `/` is the landing page for everyone. Signed-in people see "Open Echo" instead of "Sign in".
  The app itself lives under `/dashboard`.

### Auth routes

Auth owns `/signin`, `/signup`, `/signup/church`, `/verify`, `/verify-email`, `/forgot`,
`/reset-password`, `/join` and `/join/:code`.

- The two links sent by email, `/verify-email` and `/reset-password`, are built by the backend. Their
  paths cannot change without changing the backend too.
- Signed-in pages sit under `RequireAuth` in `app/routes.tsx`. A signed-out visitor is sent to
  `/signin` and returned to the page they asked for.
- Gotcha: `AuthLayout` re-keys its card on every path so each page replays its entrance animation.
  That remounts everything inside the card. Anything that must survive from one auth page to the
  next (such as sign-up step 1) is held in `AuthShell`, which sits outside the keyed element.

## Security and config

- `VITE_API_URL` is read once in `lib/config.ts`. In production it is `/api` (same origin, behind
  nginx), which is why the Content Security Policy can say `connect-src 'self'`.
- Tokens are stored in `localStorage` (`lib/tokens.ts`) so a session survives a reload. The cost is
  that an injected script could read them. The Content Security Policy in
  `infra/nginx/spa-headers.conf` and the ban on `dangerouslySetInnerHTML` are the defences.
- Never log tokens or personal data.

## Testing

- Use Vitest and Testing Library.
- Test what a user or caller can see: a disabled button, an error message, a retry after a 401.
  Do not test implementation details. No snapshot tests.
- Tests sit next to the file they test.

## Adding a feature

1. Create `features/<name>/` with `api.ts`, `hooks.ts` and `pages/`.
2. Export its routes from `index.ts` and add them to `app/routes.tsx`.
3. Build pages from `ui/` pieces. If a piece is missing, add it to `ui/`, not to the feature.
4. Handle all four screen states.
5. Run `npm run check` (lint, types, tests, build) before pushing.

## Dependencies

Each new runtime dependency needs a one-line reason in its pull request. The current set:

- React and React Router
- TanStack Query (server data)
- React Hook Form and Zod (forms)
- `@fontsource` (self-hosted fonts)
- Radix Dialog and DropdownMenu (accessible overlays), to be added when the first feature needs them

There is no chart library. Charts are small custom SVG.
