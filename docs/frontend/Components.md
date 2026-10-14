# Echo Component Inventory

**Written by:** @clintonbampoe
**Last updated:** 2026-10-14 by @clintonbampoe

The list of reusable components the design needs, taken from the 167 desktop design boards.
The mobile and tablet boards reuse the same pieces, so this list covers all 485 boards.

**Built so far:** Button, Field (with Password and Select), Icon, Logo, Quote, Alert, Reveal.

"Boards" is how many desktop boards use the pattern. "Design class" is the name the design files
use for it. "Rule S1, S2, S4" refers to the design standards for layout, controls and table cells.

## Shared pieces (`ui/`: props in, markup out, no data)

| Component | Design class(es) | Boards | Notes |
|---|---|---|---|
| Button | btn, pri, sm, dng, ghost, lnk, icon-btn | 164 | primary, default, small, danger, link, icon-only |
| Card | card | 149 | corner radius 14 |
| Chip | chip pos/neg/warn/neu | 109 | status and type chips, height 22 |
| Input / Field | inp, fi, lb, er, fg, fs, cur-in | 75 | label row, input, error line, two-column grid |
| Select (native) | inp | - | the OS picker on mobile (rule S2) |
| Segmented | seg, sw | 30 | 2-3 equal-width options (rule S2) |
| Tabs | tabs, tab | 41 | |
| Checkbox | box | 22 | inside menus and lists |
| Pill (filter) | pill, win, wp, fb, rst | 90 | the date-range pill, its popover and the filter bar |
| Menu / RowMenu | menu, opt | 22 | |
| Drawer | scrim, drawer, dh, db, df | 53 | desktop form overlay: header, body, footer |
| Modal / ConfirmDialog | modal, scrim | 28 | |
| SearchPicker | pk, pko | 28 | searchable list inside a form |
| DataList | table.t, row-h, av, sk | 111 | a table on desktop, cards below 768px (rules S1, S4) |
| Avatar | av | 47 | initials |
| Skeleton | sk | 6 | the loading state (shimmer) |
| Toast / Banner | tst, bn, msg, nt | about 20 | |
| ProgressBar | pg, bar | about 20 | projects, check-in |
| KeyValue / DefinitionList | kv, dl | 29 | detail pages |
| StatTile | tile, kv | 11 | |
| DayStrip | ds, dt | 4 | one horizontally scrolling row |
| Quote | blockquote | 2 | the rotating verse |
| Icon | 43 SVGs, 29 distinct icons | all | one component, generated from `icons.json` |

## App shell (`app/`)

Sidebar, navigation items (with "on" and "soon" states), navigation groups, the mobile top bar, the
mobile bottom tab bar, and the content panel. Present on 148 boards.

## Open decisions

- **Charts** (dashboard, tithes, finance, projects): small purpose-built SVG, no chart library.
- **Pagination:** infinite scroll with a footer line, for example "Showing 50 members. Sorted by
  name. Scroll to load more." The list response has no total (only `hasMore`, `next` and `data`),
  so the footer counts the rows loaded so far. A true total comes from a `/summary` endpoint.

## To check while building

A few short class names (ex, n, pair, sr, acc, dn, dd) are used in the design markup but are not
defined in any design stylesheet. Confirm what each one means when its screen is built.
