# Echo Component Inventory

**Written by:** @clintonbampoe
**Last updated:** 2026-10-25 by @clintonbampoe

From the 167 desktop design boards. The first table is the design's inventory; "Built in ui/" says what exists in code.

Counts are the number of desktop boards that use each pattern. Class names are the design's own vocabulary (`.btn`, `.chip`...). The mobile and tablet boards reuse the same vocabulary, so this list covers all 485 boards.

## Design inventory

| Component | Design class(es) | Boards | Notes |
| --- | --- | --- | --- |
| Button | btn, pri, sm, dng, ghost, lnk, icon-btn | 164 | primary / default / small / danger / link / icon-only |
| Card | card | 149 | radius 14 |
| Chip | chip pos/neg/warn/neu | 109 | status and type chips, height 22 |
| Input / Field | inp, fi, lb, er, fg, fs, cur-in | 75 | label row, input, error line, 2-col grid |
| Select | inp | - | Echo's own dropdown (rule S10): a glass panel under the field on a mouse, a centred glass panel on touch. Built: `SelectField`, `FilterSelect` |
| Date picker | cal, pk | - | Echo's own calendar (rule S10). Built: `DateField`, `DatePill`, `DatePanel` |
| Time and month-year wheels | - | - | Echo's own wheels (rule S10). Built: `TimeField`, `MonthYearPill`, `Wheel` |
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
| DayStrip | ds, dt | 4 | one dial of days, the picked day in a middle slot |
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
| Drawer, Sheet, BottomSheet | the overlay that holds a form, and a sheet that rises from the bottom on a phone | FormSheet, `PickerAnchor`, the dropdowns |
| Field, PasswordField, TextAreaField | text inputs. `Field` takes a `unit` (GH₵) shown inside the box | auth, members, visitors, organizations, tithes |
| SelectField, ControlledSelect, FilterSelect | Echo's own dropdown. `SelectField` is a form field and `ControlledSelect` wires it to react-hook-form; `FilterSelect` is the same as a filter pill. On a mouse it is a popover list; on touch the same list in a centred panel with 52px rows. Arrow keys, Enter, Escape and type-to-jump work, and a list of 8 or more options (`SEARCH_MIN`) gets a search box. Built from `Listbox`, `PanelSelect`, `OptionList` and `OptionPanel`, with the logic in `useListbox` and `listboxKeys.ts` | every form and every filter bar |
| DateField, TimeField, ControlledDate, ControlledTime | form fields for a day and for a time of day, with the placeholder "Choose a date" or "Choose a time". Values are plain text ("2026-10-11", "14:05"). `min`, `max` limit the days; `optional` adds Clear; `kind="birth"` makes touch screens use day, month and year wheels (`BirthPanel`). `ControlledDate` takes `onPick` for a form that follows the day (the tithe month) | members, visitors, organizations, tithes, attendance |
| DatePanel, Calendar, TimePanel, BirthPanel | what a date or time picker holds. `Calendar` is 6 Sunday-first weeks with a roving focus, keys, limits, day dots (`marks`) and a range band; `DatePanel` adds the month arrows, the month and year wheels (`Roll`) and Today and Clear; `TimePanel` is the three wheels with Now, Clear, Cancel (touch) and Done. `DayBar` uses `DatePanel` on its own | the date and time fields, DatePill, DayBar |
| PickerAnchor | holds a trigger and the panel it opens: a popover on a mouse (focus in, focus back to the `data-trigger` element, Escape, click outside and tabbing away close it), a `BottomSheet` on touch | every date and time picker, DayBar |
| Wheel, WheelSet, TimeWheels, MonthYearWheels, DayMonthYearWheels | a column of values you roll through, the day dial turned on its end (`wheelDial.ts`, `useWheel.ts`). Each wheel is a real list: every row is an option, and arrows, Page Up and Down, Home, End and typed labels work. A tap, key or typed value picks at once; a drag, flick or mouse wheel picks when the column settles. All rows are drawn and one CSS variable (`--pos`) drives them | the time and month-year pickers, date of birth on touch |
| SegmentedField | a few choices side by side, one picked (real radio buttons). `hideLabel` keeps the label for screen readers only; `compact` shrinks the words for a switch that shares a line with a card's title | organizations (role), tithes (payment method), dashboard (ranges) |
| FormSheet, FormSection | a sheet with the shared footer, and a group of fields (`FormSection` takes an optional title). `another` adds a "Save and add another" button; `blocked` disables both save buttons while a notice explains why | add and edit forms, tithes |
| PageHeader, FilterBar, FilterSelect, SearchBox, SearchField | the top of a list page; its `action` can be two buttons (the main one last). `FilterBar` is the search, the filter pills and the scope pill, and folds the filters behind a "Filters" button on a phone or tablet | members, visitors, organizations (list and members tab) |
| DayStrip | a dial of days, like the wheels of an iPhone timer. Scroll, drag, swipe, use the trackpad, a mouse wheel (sideways) or the arrow keys and the row passes one day at a time at a set pace; the day that lands in the middle slot is the picked one, filled black. A tap or an arrow glides a day into the slot. A dot marks a day that has something on it, today is tagged | attendance |
| DatePill | a filter pill that shows one day and opens Echo's calendar. A custom range uses two (From and To); give each the other's day as `range` and the calendar draws the stretch between them. Clear is offered once a day is chosen | attendance records |
| MonthYearPill, MonthBars | a filter pill that picks a month and year with two wheels (Clear, Whole year, Done), and a row of clickable month bars (a chart without a library) | tithes |
| ListBody | loading, error, empty and nothing-matches around a list, then the count and "load more". `icon` is the one the page's menu item wears; `order` adds how it is sorted ("Showing all 5 tithes. Newest first.") | members, visitors, organizations, tithes |
| PersonList | a list of people: avatar, name, columns, ⋯ menu, cards on a phone. `nameHeader` renames the first column ("Member"). The name opens a page (`href`) or, in another record's list, a panel on the same page (`onOpen`) | members, visitors, organizations, tithes |
| RecordTable | a table of records (a row can link), cards on a phone. `total` puts a figure at the right of the footer ("Total GH₵ 6,400") | attendance, tithes, events, organizations |
| Section, StatStrip, HistoryTrack, Tabs | a titled block, the numbers (a value or note too long for its tile is cut with an ellipsis, full text on hover), the history track (dots on a rail, newest on the right; a node's `tip` is the hover pill and a node can open its record), the tabs | profile pages, organizations, attendance, tithes |
| ProfileLayout, BackLink, ProfileIdentity | a record's page: back link, name card, numbers and history | member and visitor pages |
| DetailCard, DetailGroup, DetailBlock | the details card with titled blocks | member and visitor pages |
| ListCard, ListHeader, RankedBars | a list's card and its header (`title` names the content, `summary` is the count on the right); a ranked-bar card (name, amount and share over a bar that fills from zero; hover lifts the row from its centre and dims the others; `selected` and `onPick` make it a filter; a `closed` row opens nothing) | every list; Finance categories |
| DistributionBar | one stacked bar and a legend under it, drawn from `parts` (`label`, `value`, `text`, optional `share`, `tag`, `to`, `closed`). Top four and one closed "Other" by convention. A segment and its legend row light each other up; a part with `to` is a link, with `onPick` a filter | Tithes methods, Projects (How it was given), dashboard Money / Assets / Members / Projects |
| WithSide | a main column and a side column (8 + 4); stacks under it on a tablet or phone | Finance, Tithes |
| Lookup, PickerAnchor, PanelSelect, OptionPanel | the picker family: one anchored glass panel for options, dates, times and members, with a search box where the list is long and recent picks first | every form; the member picker |
| useMedia, useNarrowProfile, useSurface | screen-size and surface hooks | app, profile pages |
| Pager | related dashboard cards: side by side on a wide screen, swipeable pages with a dot for each on a phone. `columns` sizes the halves ("3fr 2fr") | dashboard |

## Patterns

Rules every screen follows. They are about how pieces are put together, not about one piece.

- **People inside another record open a panel, not a profile.** A list of members on an organization, an event, an attendance session, a project or a tithe is about that record. Clicking a person opens a panel about their part in it (role, joined date, the check-in). The panel has a deliberate "View profile" link to the member page, on the tab about that record. Only the Members and Visitors lists link a person straight to their page. Built: `MembershipSheet` in `features/organizations`; the list uses `PersonList` with `onOpen`.
- **Search and filters share one bar.** Search comes first, then the filters, then the scope pill (the time range) at the far right on a wide screen. On a phone or tablet the search sits beside a "Filters" button, and every filter and the scope pill live behind it, even when there is only one, so adding a filter later changes nothing people are used to.
- **A rule the server enforces is also checked in the form.** A member has one tithe a month. Picking a member who already has one shows a warning `Notice` with an "Update that tithe" button and disables saving (`blocked`). The server still answers 409 `TITHE_DUPLICATE` if two people race, and the form shows that message in words.
- **A dashboard card measures itself, not the screen.** `DashCard` is a container (`container-type: inline-size`), so a card lays out by the room the row gives it: a wide card keeps its hint, its foot row ("All time, not the period · See projects →") and plain title; a narrow one — under 460px of *content*, which is a card about 40px wider — drops the foot and shows a small chevron after the title instead, because on a phone the title leads. The card is the container, so the numbers count its content box; the half-width cards of a desktop row sit just above the line and the cards of a phone or a tablet row below it. A card that leads with a figure on a phone ("3 of 5", "Average age 38") hides it on a wide card, where the same figure lives in the foot. `SeeLink` is the link both the head (as `control`, e.g. "All events →") and the foot carry; it goes away on a narrow card. Built: `features/dashboard` (`DashCard`, `SeeLink`).
- **A chart card's ranges are one switch, and the picked bar is the stretch it speaks about.** Attendance, Tithes, Member growth and Income and expenses all change range with the same joined switch: one bar, the picked range filled, three choices in as few words as the heading allows ("8 weeks / 6 months / Year", or "3 months / 6 months / Year" on the money cards), and the short ones ("8 wks") on a tight row. A phone keeps the two nearest ranges and shrinks the words, so the card's title stays on one line, as the phone board draws it. The bars are buttons: picking one makes it black and the card speaks about that stretch — the figures, the foot line and the narrow figure above the chart follow it; picking it again returns to the latest, and changing the range drops the pick. Built: `RangeTabs`; `pickedStretch` and `stretchName` in `features/dashboard/trend.ts`.
- **A distribution is a bar, and the bar is a door.** There are no rings. Hovering a segment lifts it from its centre, dims the rest, shows a dark pill with its name, amount and share, and lights its legend row; the legend row does the same to the segment. Pressing a part either narrows the page (Finance categories, Tithes methods, Projects' contributions) or opens the filtered list (the dashboard's projects, assets and genders). The top four are named and the rest are one "Other", which opens nothing. Built: `DistributionBar`.
- **A dashboard row is as tall as the boards draw it.** The row wrappers in `DashboardPage` carry the height (`--dash-h`) of the board's row — the money row 460, Attendance 316, the tithes row 534, member growth 418, the events row 461, the visitors row 265 — and each card fills it; the trend cards' charts take whatever the head, strip and foot leave, so the bars, the three figures and the foot land where the board puts them. A phone gives every card the same 402px and the cards find their own shape inside it.
- **The Money and Events rows give the first card three fifths.** Income and expenses sits beside By category, and Upcoming events beside the calendar, with `columns="3fr 2fr"` on the `Pager`. The money row is taller than the board's 368 (460) because the By category card is narrower and the figures need the room; the calendar keeps its 40px days at that width and drops to 32 on a phone or tablet, where every card is 402 tall. A narrow distribution card shows its total above the bar rather than in the legend, so its categories keep their names instead of squeezing.
- **A phone re-deals the dashboard rows into themed groups.** Below 900px (`useMedia`) the dashboard lays out six groups — Money, This week, Projects and assets, Events, Members, Visitors — each headed by a label, with related cards as one `Pager` to swipe (Money, Events, Members, Visitors; Projects and assets has the Projects pager and then an Assets pager). On a wide screen the same cards sit in the paired rows, with no headings. The queries are keyed by range, so the two trees share every number and nothing is asked twice. Built: `DashboardPage` (`groups`, `rows`).
- **On a phone the Assets card is two cards.** The value, the bar against its cost and the purchase and lost figures are one card; the distribution of categories is the next page of a `Pager`. `AssetsCard`'s `split` draws them that way, as the phone board does, and a failure takes the row as one card rather than asking for the same retry twice.
- **The period pill sits at the right at every size.** It keeps its natural width — calendar, the period's name, chevron — and never stretches; a phone puts it on its own line, still at the right, and the dates live in its menu (each option shows its stretch). Built: `PeriodPill`, the dashboard header in `DashboardPage`.
- **A record's person is fixed after it is created.** An existing tithe shows its member with a "View profile" button (the same one the membership panel uses) and no way to change them. A tithe on the wrong member is deleted and recorded again.
- **A record opens to be read, then changed.** Clicking a tithe opens it read only, with its details and "Edit tithe" and "Delete tithe" buttons. The ⋯ menu still goes straight to editing or deleting. A link to one tithe (`/tithes?tithe=id`, from a member's tab or year strip) opens the same view. Built: `TitheDialogs` steps from view to edit or delete.
- **A link to a member lands on the tab about where you came from.** A tithe links to the member's Tithes tab, an organization's member to Organizations, and attendance, events and responsibilities the same way (`paths.member(id, tab)`). Only the Members and Visitors lists link to the default tab.
- **A member's Tithes tab is the year strip and the table.** The strip shows this year month by month and a given month opens that tithe; the table has the count and the total in its footer, and a row opens that tithe too.
- **History tracks show the same number of nodes.** Attendance (members and visitors) and tithes all show 12. Hovering a node shows which session or month ("Oct 2026"); whether they came is the node itself, and screen readers still get the full sentence.
- **A day is picked on a strip and lives in the address.** Attendance keeps the day in `?day=`, so a day can be bookmarked, and the way back from a session returns to that day (`paths.attendanceDay`). Today is the default and is left out of the address.
- **Marking people present is search, then tap.** The panel on a session saves each person as they are tapped (or on Enter), clears the search for the next one, and offers an Undo for the last. "Came last time" lists who came to the previous session of the same type. A visitor nobody has met is added from the same panel.
- **Clear filters empties the search box too**, from the "Clear all" link and from an empty list's button alike.
- **Searching a record's own people is scoped to it.** The members tab on an organization searches only that group's members (`name` on the organization-members endpoint).
- **No browser pickers.** Echo draws every dropdown, date, time and month picker itself (design rule S10), so they look and behave the same on every device. `useFinePointer` decides: a mouse gets a popover under the field, touch gets the same panel in the middle of the screen. A picker holds a draft and keeps it on Done, except where picking is the answer (a day on the calendar, an option in a list). Add an icon to `scripts/icons.json` and run `npm run gen:icons` to use it; `Icon.tsx` is generated.
- **Testing a picker.** `src/test/pickers.ts` has `pickDate(user, label, day)` and `pickTime(user, label, time)`, which open the field and roll or tap the way a person does. jsdom has no `matchMedia`, so tests run the mouse path; a touch test stubs `matchMedia` with `vi.stubGlobal`.

## app/ shell

side (sidebar), nav-i (+ on, soon), grp, soon-b, top (mobile top bar), bottom tab bar (mobile), panel. Present on 148 boards.

## Not yet decided

- Charts (dashboard, finance, projects): small purpose-built SVG, no chart library. `MonthBars` is the first, built for tithes; the dashboard's `TrendBars` draws light bars with the latest — or the one picked — in black, names under every bar or only at the two ends (`ends`), a quieter shade where a strip below already tells the figures (`soft`), and `TrendBody` puts three figures under it in one strip divided by hairlines. Attendance keeps them in a rail beside the chart instead, and a phone hides the strip and leads with one figure over the chart.
- Pagination: infinite scroll with a footer line, "Showing 50 members. Sorted by name. Scroll to load more." The list response has no total (only hasMore, next, data), so the footer counts loaded rows. A true total comes from a /summary endpoint.

## Verify while building

A few short class names (ex, n, pair, sr, acc, dn, dd) are not defined in any board stylesheet and appear in markup only. I'll confirm each when its screen is built.
- **A record's page opens on Details, and its tabs carry no counts.** Member, visitor, organization, project, asset and event pages all start on Details; the other tabs follow, and the picked tab lives in the URL (`?tab=`). The count belongs in the tab's own list header, not on the tab.
