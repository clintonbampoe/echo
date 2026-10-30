# Echo — Performance Log

> The ritual. One entry per move, five lines each. This file is simultaneously the
> engineering log, the content pipeline, and the guard against repeating a mistake.
>
> Format follows the plan in `docs/performance/plan.md`.

---

## Template

```markdown
## Move N — <title>  ·  <YYYY-MM-DD>

**Hypothesis**
<what we thought would happen>

**Before**
<the number>

**After**
<the number>

**Learned**
<the principle, in one sentence>

**Surprise**
<what it changed our mind about>
```

---

## Entries

## Move 2 — Stop asking for things you already know  ·  2026-10-30

**Hypothesis**
Most of the requests a page makes are answers the app already holds: the tithes page asks for each payment method
separately although the summary carries the split, the dashboard asks twice for "last week", and the event page reads
every registration and check-in just to count them. Fewer, narrower requests are faster and stay under the rate limit.

**Before**
Cold loads, distinct requests (own + 2 shell): tithes 11, dashboard 21, one event page 15 (10 chained pages of rows).
`attendance/sessions/summary` read 158,124 buffers (150,102 from disk) and took 837 ms cold; its old form averaged 4 s.
Global rate limit: 100 requests a minute per IP, so five page changes in a minute could lock a person out.

**After**
Tithes 6, dashboard 18, event page 4 on open (the numbers come from one count) and 5 once a tab is opened, reading one
page of 50 rows instead of all of them. The sessions summary reads 6,265 buffers (4,419 from disk), 424 ms cold.
Rate limit is 600 a minute per signed-in person (100 per IP for anyone not signed in), counted on their own.
An unused 342 MB index on `attendance` is gone; two small indexes serve one event's lists in their display order.

**Learned**
Ask the question the screen needs (a count, one page, one filter) and let the server answer it, instead of fetching
everything and working it out in the browser.

**Surprise**
Three of the worst problems were not slow queries at all. A non-nullable value in a query-string record is a *required*
parameter in a minimal API, whatever default it has, so `months`, `order` and `limit` were documented as optional but
rejected when left out (and the Finance and Tithes charts failed outright). And one query was slow only because an
extra `congregation_id` condition sent the planner through the whole table instead of walking the sessions.

### What was left alone, and why

- Counting 560,000 attendance rows (a year of one very large congregation) is inherent to a count, and the plan is
  already an index-only scan. Pre-aggregation would add counters that can drift; not worth it at real sizes.
- The events, tithes and attendance monthly summaries are index-only scans of 100,000+ rows each, cached for 2 minutes.
- The two shell requests (`/users/me`, `/congregation`) could be one, saving about 5 ms on a full reload.

