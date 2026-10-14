# Echo Component Inventory

**Written by:** @clintonbampoe
**Last updated:** 2026-10-14 by @clintonbampoe

From the 167 desktop design boards. The first table is the design's inventory; "Built in ui/" says what exists in code.

Counts are the number of desktop boards that use each pattern. Class names are the design's own vocabulary (`.btn`, `.chip`...). The mobile and tablet boards reuse the same vocabulary, so this list covers all 485 boards.

## Design inventory

| Component | Design class(es) | Boards | Notes |
| --- | --- | --- | --- |
| Button | btn, pri, sm, dng, ghost, lnk, icon-btn | 164 | primary / default / small / danger / link / icon-only |
| Card | card | 149 | radius 14 |
| Chip | chip pos/neg/warn/neu | 109 | status and type chips, height 22 |
| Input / Field | inp, fi, lb, er, fg, fs, cur-in | 75 | label row, input, error line, 2-col grid |
| Select (native) | inp | - | OS picker on mobile (rule S2) |
| Segmented | seg, sw | 30 | 2-3 equal-width options (rule S2) |
| Tabs | tabs, tab | 41 | |
| Checkbox | box | 22 | inside menus and lists |
| Pill (filter) | pill, win, wp, fb, rst | 90 | the date-window pill, its popover (wp) and the filter bar (fb) |
| Menu / RowMenu | menu, opt | 22 | |
| Drawer | scrim, drawer, dh, db, df | 53 | desktop form overlay; header / body / footer |
| Modal / ConfirmDialog | modal, scrim | 28 | |
| SearchPicker | pk, pko | 28 | searchable list inside a form |
| DataList | table.t, row-h, av, sk | 111 | table on desktop, cards below 768 (S1, S4) |
| Avatar | av | 47 | initials |
| Skeleton | sk | 6 | the design's loading state (shimmer) |
| Toast / Banner | tst, bn, msg, nt | ~20 | |
| ProgressBar | pg, bar | ~20 | projects, check-in |
| KeyValue / DefinitionList | kv, dl | 29 | detail pages |
| StatTile | tile, kv | 11 | |
| DayStrip | ds, dt | 4 | single horizontally scrolling row |
| Quote | blockquote | 2 | the rotating verse |
| Icon | (43 svgs, 29 distinct icons) | all | one component + icons.json |

## Built in ui/

Every feature imports these through `ui/index.ts` only. Each one holds no data, only props.

| Component | What it is | Used by |
| --- | --- | --- |
| ActionMenu | the ⋯ menu on a row (Edit, Delete) | PersonList, RecordTable |
| Alert, EmptyState | an error with a fix, and an empty list with what to do | every page |
| Avatar, Chip, Icon, Logo, Quote, Reveal | small display pieces | everywhere |
| Button, LinkButton | the button, and a link styled as one | everywhere |
| ConfirmDialog, DeleteDialog | the dialog, and the "Delete X?" version with its error state | members, visitors |
| Drawer, Sheet | the overlay that holds a form | FormSheet |
| Field, PasswordField, SelectField, ControlledSelect, TextAreaField | form inputs | auth, members, visitors |
| FormSheet, FormSection | a sheet with the shared footer, and a titled group of fields | add and edit forms |
| PageHeader, FilterBar, FilterSelect, SearchBox, SearchField | the top of a list page | members, visitors |
| ListBody | loading, empty, error and "load more" around a list | members, visitors |
| PersonList | a list of people: avatar, name link, columns, ⋯ menu, cards on a phone | members, visitors |
| RecordTable | a table of records (a row can link), cards on a phone | attendance, tithes, events |
| Section, StatStrip, CellStrip, Tabs | a titled block, the numbers, the strip of squares, the tabs | profile pages |
| ProfileLayout, BackLink, ProfileIdentity | a record's page: back link, name card, numbers and history | member and visitor pages |
| DetailCard, DetailGroup, DetailBlock | the details card with titled blocks | member and visitor pages |
| useMedia, useNarrowProfile, useSurface | screen-size and surface hooks | app, profile pages |

## app/ shell

side (sidebar), nav-i (+ on, soon), grp, soon-b, top (mobile top bar), bottom tab bar (mobile), panel. Present on 148 boards.

## Not yet decided

- Charts (dashboard, tithes, finance, projects): small purpose-built SVG, no chart library.
- Pagination: infinite scroll with a footer line, "Showing 50 members. Sorted by name. Scroll to load more." The list response has no total (only hasMore, next, data), so the footer counts loaded rows. A true total comes from a /summary endpoint.

## Verify while building

A few short class names (ex, n, pair, sr, acc, dn, dd) are not defined in any board stylesheet and appear in markup only. I'll confirm each when its screen is built.
