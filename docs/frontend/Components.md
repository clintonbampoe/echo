# Echo Component Inventory

**Written by:** @clintonbampoe
**Last updated:** 2026-10-16 by @clintonbampoe

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
| Segmented | seg, sw | 30 | 2-3 equal-width options (rule S2). Built: SegmentedField |
| Tabs | tabs, tab | 41 | |
| Checkbox | box | 22 | inside menus and lists |
| Pill (filter) | pill, win, wp, fb, rst | 90 | the date-window pill, its popover (wp) and the filter bar (fb). Built: `FilterBar` (see Patterns) |
| Menu / RowMenu | menu, opt | 22 | |
| Drawer | scrim, drawer, dh, db, df | 53 | desktop form overlay; header / body / footer |
| Modal / ConfirmDialog | modal, scrim | 28 | |
| SearchPicker | pk, pko | 28 | searchable list inside a form. Built for members: `MemberPicker` in `features/members` |
| DataList | table.t, row-h, av, sk | 111 | table on desktop, cards below 768 (S1, S4) |
| Avatar | av | 47 | initials |
| Skeleton | sk | 6 | the design's loading state (shimmer) |
| Toast / Banner | tst, bn, msg, nt | ~20 | |
| ProgressBar | pg, bar | ~20 | projects, check-in |
| KeyValue / DefinitionList | kv, dl | 29 | detail pages |
| StatTile | tile, kv | 11 | |
| DayStrip | ds, dt | 4 | one dial of days, the picked day in a middle slot (rule S8) |
| Quote | blockquote | 2 | the rotating verse |
| Icon | (43 svgs, 29 distinct icons) | all | one component + icons.json |

## Built in ui/

Every feature imports these through `ui/index.ts` only. Each one holds no data, only props.

| Component | What it is | Used by |
| --- | --- | --- |
| ActionMenu | the ⋯ menu on a row: actions, or a choice with a heading and a ticked current one (`checked`, `group`, `separated`) | PersonList, RecordTable |
| Alert, EmptyState | an error with a fix, and an empty list with what to do | every page |
| Notice | a note inside a form or panel. `tone` is `info` (default) or `warning` (alert icon, warm colours) and it can carry an `action` button | tithes (one tithe a month) |
| Avatar, Chip, Icon, Logo, Quote, Reveal | small display pieces | everywhere |
| Button, LinkButton | the button, and a link styled as one | everywhere |
| ConfirmDialog, DeleteDialog | the dialog, and the "Delete X?" version with its error state; `confirmLabel` and `cancelLabel` change the buttons ("Remove from group"); `tone="neutral"` keeps the confirm button black for an action that is not a loss ("Mark completed") | members, visitors, organizations, attendance |
| Drawer, Sheet, BottomSheet | the overlay that holds a form, and a sheet that rises from the bottom on a phone | FormSheet, MonthYearPill |
| Field, PasswordField, SelectField, ControlledSelect, TextAreaField | form inputs. `Field` takes a `unit` (GH₵) shown inside the box | auth, members, visitors, organizations, tithes |
| SegmentedField | a few choices side by side, one picked (real radio buttons). `wrap` makes chips of their own width that wrap, for a longer list | organizations (role), tithes (payment method) |
| FormSheet, FormSection | a sheet with the shared footer, and a group of fields (`FormSection` takes an optional title). `another` adds a "Save and add another" button; `blocked` disables both save buttons while a notice explains why | add and edit forms, tithes |
| PageHeader, FilterBar, FilterSelect, SearchBox, SearchField | the top of a list page; its `action` can be two buttons (the main one last). `FilterBar` is the search, the filter pills and the scope pill, and folds the filters behind a "Filters" button on a phone or tablet | members, visitors, organizations (list and members tab) |
| DayStrip | a dial of days, like the wheels of an iPhone timer. Scroll, drag, swipe, use the trackpad, a mouse wheel (sideways) or the arrow keys and the row passes one day at a time at a set pace; the day that lands in the middle slot is the picked one, filled black. A tap or an arrow glides a day into the slot. A dot marks a day that has something on it, today is tagged | attendance |
| MonthYearPill, MonthBars | a filter pill that picks a month and year, and a row of clickable month bars (a chart without a library) | tithes |
| ListBody | loading, error, empty and nothing-matches around a list, then the count and "load more". `icon` is the one the page's menu item wears; `order` adds how it is sorted ("Showing all 5 tithes. Newest first.") | members, visitors, organizations, tithes |
| PersonList | a list of people: avatar, name, columns, ⋯ menu, cards on a phone. `nameHeader` renames the first column ("Member"). The name opens a page (`href`) or, in another record's list, a panel on the same page (`onOpen`) | members, visitors, organizations, tithes |
| RecordTable | a table of records (a row can link), cards
