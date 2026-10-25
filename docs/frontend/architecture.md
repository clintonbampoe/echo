# Frontend Architecture

**Written by:** @clintonbampoe
**Last updated:** 2026-10-25 by @clintonbampoe

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
- The shell (`app/shell`) is a 244px sidebar on wide screens and a top bar with a full-screen menu page on a phone or tablet. The nav is flat (Main, then an Admin group for admins) and follows the design boards. `/me` and `/settings` open from the sidebar footer.
- Settings that stay on the device (density, reduce motion, currency symbol) live in `lib/preferences` and are read by CSS through attributes on `<html>`.

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

`features/members` is the model, and every feature with a screen now has the same shape (`account`,
`invitations` and `settings` are small and carry only the folders they need; `website` is the public site and
keeps its own).

```text
features/members/
  index.ts       what other features may use. Nothing else is importable.
  routes.tsx     its routes, lazy-loaded
  api/           one file per resource: HTTP calls and API types. No React.
  hooks/         keys.ts, queries.ts, mutations.ts, figures.ts. The only place that knows the cache.
  model/         pure logic: filters, labels, options, figures, display lines. No React; has its own tests.
  forms/         schema.ts (zod), mapping.ts (API <-> form values)
  components/    grouped by screen area (list/, profile/, form/, dialogs/, picker/); a component, its
                 stylesheet and its test sit together
  pages/         compose components and hooks, and nothing else. A page has no logic of its own.
  testing/       fixtures for this feature's tests (`aMember()`)
```

Read top to bottom: `api` → `hooks` → `model` → `components` → `pages`. Not every feature has every folder.

## Shared pieces

Use these before writing your own. A new list page is about 40 lines made from them.

| Where | Pieces | For |
| --- | --- | --- |
| `lib` | `api`, `usePagedList` | requests, cursor paging |
| `lib` | `urlFilters`, `useUrlFilters`, `windows` | filters and date ranges kept in the URL. `resetKey` empties a search box after Clear |
| `lib` | `dates`, `format`, `validation`, `regions`, `paths` | days, money, required fields, links |
| `ui` | `PageHeader`, `FilterBar`, `SearchBox`, `ListBody` | the shell of a list page. `FilterBar` folds filters behind a "Filters" button on a phone or tablet, even for one filter |
| `ui` | `ListCard`, `ListHeader`, `PersonList`, `RecordTable`, `RecordLines` | every list card opens with a `ListHeader` (title left, count right), drawn from the list's `title`; pages never hand-build one |
| `ui` | `Section`, `StatStrip`, `HistoryTrack`, `RankedBars`, `DistributionBar`, `EmptyState` | numbers, the history track (a line of dots joined by a rail), ranked bars (Finance categories), a distribution bar with its legend (Tithes methods, Projects, the dashboard) |
| `ui` | `WithSide` | a page whose list sits beside a column of figures (Finance, Tithes: 8 + 4) and stacks on a tablet |
| `ui` | `Lookup`, `PickerAnchor`, `PanelSelect` | one family of pickers: a searchable lookup (member picker), and the anchor that places a picker panel under its field |
| `ui` | `FormSheet`, `FormSection`, `Field`, `ControlledSelect`, `DeleteDialog`, `Notice` | forms, delete and notes inside a form |
| `ui` | `MonthYearPill`, `MonthBars` | picking a month, and a month-by-month chart |
| `ui` | `Chip` + `ChipTone`, `plainOptions`, `withAny` | status labels, dropdown lists |
| `ui` | `RowCard` + `Row`, `Switch` | a card of icon rows with an action menu, an on/off switch |
| `ui` | `Tabs`, `SegmentedField`, `Disclosure`, `ProfileHeader` | pill tabs and switches, a fold in a form, a profile's top |
| `ui` | `ToastProvider` + `useToast`, `Presence` + `useHeld` | saying what happened, letting a sheet or dialog play its exit |
| `test` | `kit` (`renderApp`, `signIn`, `json`, `pathOf`), `pickers` | what component tests share. Don't copy these into a test |

Features import `ui` only through `ui/index.ts`, which stays alphabetical.

## Liquid glass

Menus, panels, sheets, pickers and tooltips share one material, defined once in `styles/glass.css`: `.glass-pop`
(small floating things), `.glass-panel` (sheets, dialogs, pickers), `.glass-scrim`, `.glass-tip` (the dark pill
tooltip), `.glass-opt` (an option that staggers in with `--i`). Lists get `.liquid-row` (a tile that lifts behind
a row on hover), people get `.liquid-av`, dates `.liquid-date`. Nothing writes its own blur or shadow.

The platform rule is "less, not smaller": a phone shows fewer things, never the same things shrunk. A control
that would shrink below a comfortable touch size is folded away instead (filters behind one button, one pill
of dates, two chart ranges).

Charts are doors. A bar, a segment or a rank row is a button: hover or focus lifts it from its centre and dims
its siblings, a dark pill says what it is, and a press either narrows the page (`selected` / `onPick`) or opens
the filtered list (`to`). Segments and legend rows light each other up (`:has()`). A distribution shows its top
four and one "Other", which opens nothing.

## Interaction

Everything reacts to the person, and it is all built once in `ui/`, `styles/tokens.css` and `styles/motion.css`
so no screen writes its own. The rulebook is `claude/feel-and-interaction-spec.md`; components use its tokens
(`--ease-out`, `--ease-spring`, `--ease-soft`, `--t-fast`/`--t-base`/`--t-spring`/`--t-enter`, `--lift-1`, `--lift-3`,
`--press*`, `--hover-*`, `--shadow-*`, `--focus-ring`), never their own numbers.

- **Hover only where there is a hover.** Every hover reaction sits in `@media (hover: hover)`; a touch screen gets
  the press state instead. A card that does nothing when clicked reacts to nothing. A clickable card gets a
  stronger border, a shadow and a 3px lift, and presses to `--press-card`.
- **One element never crowds another.** A reaction moves or fills only its own element (a row's hover fill is
  inset, the dividers on both sides hide, a nav item slides its content not its box).
- **Press**: every `button` and `[role=button]` shrinks to `--press` and springs back. Opt out with `data-press="none"`.
- **Liquid pill**: `Tabs`, joined `SegmentedField` and the sidebar's `NavList` glide one thumb to the pick
  (`useSlidingThumb` measures and sets `--x/--w/--y/--h`; CSS does the rest). Tab panels fade and rise.
- **Pop and exit**: menus, dropdowns, sheets, dialogs and toasts animate in and out. `usePresence` keeps a
  closing thing on the page until its exit has played. A sheet or dialog that is mounted only while wanted is
  wrapped in `<Presence open>`, with `useHeld` keeping what it showed.
- **Touch gestures**: there are no bottom sheets. On a phone `Sheet` and `CenterPanel` materialise in the middle of
  the screen as glass panels; a drag moves them and a flick or a quarter-distance drag closes them
  (`useDragDismiss`: follows the finger, springs back, and comes back if a dirty form refuses). The shell (`app/shell`) owns two app-wide gestures: pulling the page
  down from the top refreshes every query (`PullToRefresh`, never on forms or dialogs) and holding a row opens its
  ⋯ menu (`useLongPressMenu`, which presses the row's own menu button). Lists build neither.
- **Loading**: shimmer skeletons shaped like what will arrive (`.skeleton`, `PersonListSkeleton`, `StatStrip busy`).
  Bars and progress grow from the baseline (`.grow-y`, `.grow-x`, 40ms apart). Rows arrive one after another (`.stagger`).
- **Saying what happened**: `Toast` lasts 5s and holds while hovered; a list's header summary (`ListHeader`) is a
  polite live region, so a filter that changes the count is read out.
- **Numbers**: `formatNumber` writes whole numbers below 1,000, then `k` and `m` with up to two decimals and no
  trailing zeros (2k, 2.05k, 15.64k). Exact values live in tooltips and detail panes.
- **Reduced motion**: `reset.css` shortens every animation and transition, and the app's own Reduce motion
  setting (`html[data-reduce-motion="true"]`) does the same: no lift, no scale, no spring. Exit waits are skipped
  too (`exitDuration`). Tests turn exit animations off (`test/setup.ts`).

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
  The app is light. The public pages (the website and auth) are light or dark by the
  person's choice (moon/sun button, remembered, otherwise the device setting); they use
  `[data-surface=light|dark]` tokens with the same names, and call `usePublicSurface()`.

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

## List and page building blocks

- **Page rhythm**: the shell makes a page's root a column with a 22px gap (16px on a phone). Pages do not add their
  own `margin-top` between sections.
- **Headers**: `PageHeader` has one main button (`add`: a pill on a wide screen, a round + on a phone) and an optional
  `action` for a second button. A record's page uses `ProfileHeader` or its own header: the main action plus an
  `ActionMenu` (⋯) for Edit and Delete.
- **Lists**: `ListBody` does loading, failed, empty and the count. With `footerInside` it hands its footer to the list,
  which puts it at the bottom of its own card. `PersonList` (people rows), `RecordTable` (`line` mode gives
  `RecordLines`, a row with a title, a line under it and figures on the right) and card grids (projects, organizations)
  are the three shapes. A whole row or card opens the record.
- **Figures**: `StatStrip` is one card on a wide screen. A `Stat` may carry a `unit` and a `delta`. `IconTile` and
  `DateTile` mark the start of a row. `DayStrip` is the day wheel, drawn as one card.

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

1. Create `features/<name>/` in the shape above: `api/`, `hooks/`, `components/`, `pages/`, `index.ts`, `routes.tsx`.
2. Check the shared pieces first. If something generic is missing, add it to `ui/` or `lib/`.
3. Export its routes and add them to `app/routes.tsx`.
4. Handle all four states.
5. Run `npm run check` before pushing.
