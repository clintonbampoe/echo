# Echo Component Inventory

**Written by:** @clintonbampoe
**Last updated:** 2026-10-15 by @clintonbampoe

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
| DayStrip | ds, dt | 4 | single horizontally scrolling row |
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
| ConfirmDialog, DeleteDialog | the dialog, and the "Delete X?" version with its error state; `confirmLabel` and `cancelLabel` change the buttons ("Remove from group") | members, visitors, organizations |
| Drawer, Sheet, BottomSheet | the overlay that holds a form, and a sheet that rises from the bottom on a phone | FormSheet, MonthYearPill |
| Field, PasswordField, SelectField, ControlledSelect, TextAreaField | form inputs. `Field` takes a `unit` (GH₵) shown inside the box | auth, members, visitors, organizations, tithes |
| SegmentedField | a few choices side by side, one picked (real radio buttons) | organizations (role) |
| FormSheet, FormSection | a sheet with the shared footer, and a titled group of fields. `another` adds a "Save and add another" button; `blocked` disables both save buttons while a notice explains why | add and edit forms, tithes |
| PageHeader, FilterBar, FilterSelect, SearchBox, SearchField | the top of a list page. `FilterBar` is the search, the filter pills and the scope pill, and folds the filters behind a "Filters" button on a phone or tablet | members, visitors, organizations (list and members tab) |
| MonthYearPill, MonthBars | a filter pill that picks a month and year, and a row of clickable month bars (a chart without a library) | tithes |
| ListBody | loading, error, empty and nothing-matches around a list, then the count and "load more". `icon` is the one the page's menu item wears | members, visitors, organizations, tithes |
| PersonList | a list of people: avatar, name, columns, ⋯ menu, cards on a phone. The name opens a page (`href`) or, in another record's list, a panel on the same page (`onOpen`) | members, visitors, organizations, tithes |
| RecordTable | a table of records (a row can link), cards on a phone | attendance, tithes, events, organizations |
| Section, StatStrip, CellStrip, Tabs | a titled block, the numbers (a value or note too long for its tile is cut with an ellipsis, full text on hover), the strip of squares, the tabs | profile pages, organizations |
| ProfileLayout, BackLink, ProfileIdentity | a record's page: back link, name card, numbers and history | member and visitor pages |
| DetailCard, DetailGroup, DetailBlock | the details card with titled blocks | member and visitor pages |
| useMedia, useNarrowProfile, useSurface | screen-size and surface hooks | app, profile pages |

## Patterns

Rules every screen follows. They are about how pieces are put together, not about one piece.

- **People inside another record open a panel, not a profile.** A list of members on an organization, an event, an attendance session, a project or a tithe is about that record. Clicking a person opens a panel about their part in it (role, joined date, the check-in). The panel has a deliberate "View profile" link to the member page. Only the Members and Visitors lists link a person straight to their page. Built: `MembershipSheet` in `features/organizations`; the list uses `PersonList` with `onOpen`.
- **Search and filters share one bar.** Search comes first, then the filters, then the scope pill (the time range) at the far right on a wide screen. On a phone or tablet the search sits beside a "Filters" button, and every filter and the scope pill live behind it, even when there is only one, so adding a filter later changes nothing people are used to.
- **A rule the server enforces is also checked in the form.** A member has one tithe a month. Picking a member who already has one shows a warning `Notice` with an "Update that tithe" button and disables saving (`blocked`). The server still answers 409 `TITHE_DUPLICATE` if two people race, and the form shows that message in words.
- **A record's person is fixed after it is created.** An existing tithe shows its member with a "View profile" link and no way to change them. A tithe on the wrong member is deleted and recorded again.
- **Clear filters empties the search box too**, from the "Clear all" link and from an empty list's button alike.
- **Searching a record's own people is scoped to it.** The members tab on an organization searches only that group's members (`name` on the organization-members endpoint).

## app/ shell

side (sidebar), nav-i (+ on, soon), grp, soon-b, top (mobile top bar), bottom tab bar (mobile), panel. Present on 148 boards.

## Not yet decided

- Charts (dashboard, finance, projects): small purpose-built SVG, no chart library. `MonthBars` is the first, built for tithes.
- Pagination: infinite scroll with a footer line, "Showing 50 members. Sorted by name. Scroll to load more." The list response has no total (only hasMore, next, data), so the footer counts loaded rows. A true total comes from a /summary endpoint.

## Verify while building

A few short class names (ex, n, pair, sr, acc, dn, dd) are not defined in any board stylesheet and appear in markup only. I'll confirm each when its screen is built.
