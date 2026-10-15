# Frontend Architecture

**Written by:** @clintonbampoe
**Last updated:** 2026-10-15 by @clintonbampoe

How the Echo web app is organised. The goal is low cognitive load: open a file and know what it is,
what it may import, and that it fits on a screen or two.

## Layers

Dependencies point down, never up. ESLint fails the build if they don't.

```text
app        wiring: providers, router, shell
 ↓
features   one folder per area of the product
 ↓
ui         dumb components: props in, markup out
 ↓
lib        plain TypeScript: API client, dates, formatting
```

- `lib/` has no components. `ui/` imports only `lib/`. A component that needs data isn't a `ui/` component.
- `features/` import `ui/` and `lib/`. Another feature is imported only through its `index.ts`.
- Nothing imports `app/`.

## Who owns what

1. **A feature owns its tables, numbers, labels and types.** Others only consume them through its
   `index.ts`. The member profile shows attendance, tithes, events, organizations and projects, but each
   is built and owned by its own feature.
2. **Code goes where it belongs, even if that feature isn't built yet.** Some boilerplate now beats a
   misplaced file later.
3. **A concept several features share gets its own home.** Payment methods serve tithes, contributions
   and offerings, so they live in `features/transactions`, mirroring the backend.
4. **One owner per record.** It is created, edited and deleted only on its own page. Other pages read
   it and link to it (`lib/paths.ts`).
5. **Generic code moves to `ui/` or `lib/` the second time it's needed.**
6. **A person shown inside another record opens a panel about that record**, with a deliberate "View
   profile" link to the member page. Only the Members and Visitors lists link a person straight to
   their page.

## Inside a feature

```text
features/tithes/
  api.ts       HTTP calls and API types. No React.
  hooks.ts     TanStack Query hooks and keys. The only file that knows the cache.
  labels.ts    words and chip colours for a status or kind
  options.ts   dropdown lists
  view.ts      pure logic for what is shown     stat.ts + useStat.ts   profile card numbers
  schema.ts    zod form schema
  filters.ts   the list's URL filters, and the API query built from them
  titheForm.ts form values, and mapping to and from the API (named for the form)
  components/  pieces it draws, including ones other features embed
  index.ts     what other features may use
```

Not every feature has every file. Read top to bottom: `api` → `hooks` → `components`.

## Shared pieces

Use these before writing your own. A new list page is about 40 lines made from them.

| Where | Pieces | For |
| --- | --- | --- |
| `lib` | `api`, `usePagedList` | requests, cursor paging |
| `lib` | `urlFilters`, `useUrlFilters`, `windows` | filters and date ranges kept in the URL. `resetKey` empties a search box after Clear |
| `lib` | `dates`, `format`, `validation`, `regions`, `paths` | days, money, required fields, links |
| `ui` | `PageHeader`, `FilterBar`, `SearchBox`, `ListBody` | the shell of a list page. `FilterBar` folds filters behind a "Filters" button on a phone or tablet, even for one filter |
| `ui` | `RecordTable`, `Section`, `StatStrip`, `CellStrip`, `EmptyState` | tables and numbers |
| `ui` | `FormSheet`, `FormSection`, `Field`, `ControlledSelect`, `DeleteDialog`, `Notice` | forms, delete and notes inside a form |
| `ui` | `MonthYearPill`, `MonthBars` | picking a month, and a month-by-month chart |
| `ui` | `Chip` + `ChipTone`, `plainOptions`, `withAny` | status labels, dropdown lists |

Features import `ui` only through `ui/index.ts`, which stays alphabetical.

## State, errors and styling

| Kind of state | Home |
| --- | --- |
| Server data | TanStack Query cache |
| What the user is looking at (filters, date range, `?tab=`) | the URL |
| Throwaway (is a menu open) | `useState` |

- No global store. Lists have no total: count what is loaded, and take any real total from `/summary`.
  Never count or sum in the browser.
- Failed requests become an `ApiError`. Screens branch on `errorCode`, never on message text. Forms put
  `fieldErrors` on their fields. Never show a raw server error to a user.
- Every data screen has four states: loading, empty, no results, error.
- Clearing filters clears the search box too. A box is emptied by giving it a new `key` (`resetKey`), never
  by writing into it.
- Colours and radii are tokens (`var(--text)`, never a hex). Each component has its own `X.module.css`.
  The app is light; landing and auth are dark.

## Code conventions

- Plain functions, named exports, `type` not `interface`, string unions not enums, no classes, no `any`.
- No semicolons, single quotes. Don't reformat files you aren't changing.
- Early returns. Derive values instead of syncing them with `useEffect`.
- No `useMemo` or `useCallback` unless a measurement says so.
- Dates stay `YYYY-MM-DD` text. Don't round-trip them through `new Date()`.
- Comments say why, in plain words. Components aim for 150 lines; past that, split.

## Enforced by

| Rule | Enforced by |
| --- | --- |
| Layer direction; other features only via `index.ts` | ESLint `no-restricted-imports` |
| File ≤ 250 lines, function ≤ 60 (warn), complexity ≤ 10 | ESLint |
| No classes, `console.log`, `useMemo`/`useCallback` | ESLint `no-restricted-syntax` |
| Strict types, no unchecked index access | `tsconfig.app.json` |
| Config present and valid at startup | `lib/config.ts` |

## Routing, security, tests

- Every URL is in `app/routes.tsx`; features export their own routes. A page not built yet is a
  placeholder, so links never 404. Signed-in pages sit under `RequireAuth`.
- The emailed links `/verify-email` and `/reset-password` are built by the backend; their paths can't
  change without changing it too.
- Gotcha: `AuthLayout` re-keys its card on every path, which remounts everything inside. Anything that
  must survive between auth pages lives in `AuthShell`, outside the keyed element.
- `VITE_API_URL` is read once in `lib/config.ts`. Tokens live in `localStorage`; the CSP and the ban on
  `dangerouslySetInnerHTML` defend them. Never log tokens or personal data.
- Vitest + Testing Library. Test what a user can see, not implementation. Tests sit beside their file.

## Adding a feature

1. Create `features/<name>/` with `api.ts`, `hooks.ts`, `components/`, `index.ts`.
2. Check the shared pieces first. If something generic is missing, add it to `ui/` or `lib/`.
3. Export its routes and add them to `app/routes.tsx`.
4. Handle all four states.
5. Run `npm run check` before pushing.
