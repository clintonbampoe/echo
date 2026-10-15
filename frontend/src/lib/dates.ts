// Plain "2026-10-14" days. Pure functions, so the tests can pass "today".
const pad = (n: number) => String(n).padStart(2, "0");

// January to December, in full. Format.ts shortens them; a tithe's month comes back by name.
export const monthNames = [
  "January",
  "February",
  "March",
  "April",
  "May",
  "June",
  "July",
  "August",
  "September",
  "October",
  "November",
  "December",
];

export const todayText = () => new Date().toISOString().slice(0, 10);

// Jan 1 to today, as the plain days the API expects.
export const thisYear = (today: string) => ({
  year: Number(today.slice(0, 4)),
  from: `${today.slice(0, 4)}-01-01`,
  to: today,
});

// The same day some months earlier ("2026-10-31" minus 3 -> "2026-07-31"), or the month's last day when
// that month is shorter ("2026-05-31" minus 3 -> "2026-02-28").
export function monthsBefore(day: string, months: number): string {
  const [year = 0, month = 1, date = 1] = day.split("-").map(Number);
  const index = year * 12 + (month - 1) - months;
  const y = Math.floor(index / 12);
  const m = (index % 12) + 1;
  const last = new Date(Date.UTC(y, m, 0)).getUTCDate();
  return `${y}-${pad(m)}-${pad(Math.min(date, last))}`;
}

// Some lists have no "everything" mode: asked for by owner with no dates, the server answers for this
// year only. A wide range asks for the lot.
export const ALL_TIME = { from: "2000-01-01", to: "2100-12-31" };

// Year, month and day of a plain "2019-03-12" day (a time part is ignored).
export const dayParts = (day: string) =>
  day.slice(0, 10).split("-").map(Number) as [number, number, number];

// Whole years since a birth date: the age goes up on the birthday, not on 1 January.
export function ageOn(birth: string, today: string): number {
  const [by, bm, bd] = dayParts(birth);
  const [ty, tm, td] = dayParts(today);
  return ty - by - (tm < bm || (tm === bm && td < bd) ? 1 : 0);
}

// Whole days from one plain day to a later one ("2026-10-01" to "2026-10-14" is 13). Never below 0.
export function daysBetween(from: string, to: string): number {
  const [fy, fm, fd] = dayParts(from);
  const [ty, tm, td] = dayParts(to);
  return Math.max(
    0,
    Math.round(
      (Date.UTC(ty, tm - 1, td) - Date.UTC(fy, fm - 1, fd)) / 86_400_000,
    ),
  );
}

// The plain day some days after another ("2026-10-14" plus 60 is "2026-12-13").
export function daysAfter(day: string, days: number): string {
  const [y, m, d] = dayParts(day);
  return new Date(Date.UTC(y, m - 1, d + days)).toISOString().slice(0, 10);
}
