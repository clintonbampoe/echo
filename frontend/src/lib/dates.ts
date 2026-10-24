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

// Sunday to Saturday, in full, in the order Date.getUTCDay counts them.
export const weekdayNames = [
  "Sunday",
  "Monday",
  "Tuesday",
  "Wednesday",
  "Thursday",
  "Friday",
  "Saturday",
];

export const todayText = () => new Date().toISOString().slice(0, 10);

// The time now as the plain "14:05:00" the API expects for a time of day, on the clock of the person's own device.
export function nowTime(): string {
  const d = new Date();
  return (
    [d.getHours(), d.getMinutes()]
      .map((n) => String(n).padStart(2, "0"))
      .join(":") + ":00"
  );
}

// Jan 1 to today, as the plain days the API expects.
export const thisYear = (today: string) => ({
  year: Number(today.slice(0, 4)),
  from: `${today.slice(0, 4)}-01-01`,
  to: today,
});

// The first day of the week (Sunday, as on the server) that a plain day falls in, and of its month.
export function weekStart(day: string): string {
  const [y, m, d] = dayParts(day);
  const weekday = new Date(Date.UTC(y, m - 1, d)).getUTCDay();
  return daysAfter(day, -weekday);
}

export const monthStart = (day: string) => `${day.slice(0, 7)}-01`;

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

// A whole calendar year (this one, or "last-year"), not just up to today, or all time. For the lists whose records
// run across dates (projects, assets) rather than happen on one.
export function calendarYearDates(
  key: string,
  today: string,
): { from: string; to: string } {
  if (key === "all") return ALL_TIME;
  const year = Number(today.slice(0, 4));
  const wanted = key === "last-year" ? year - 1 : year;
  return { from: `${wanted}-01-01`, to: `${wanted}-12-31` };
}

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

// Whether text is a real plain day ("2026-02-30" is not).
export function isDay(text: string): boolean {
  if (!/^\d{4}-\d{2}-\d{2}$/.test(text)) return false;
  const [y, m, d] = dayParts(text);
  return new Date(Date.UTC(y, m - 1, d)).toISOString().slice(0, 10) === text;
}

// How many days a month has. The month is 1 to 12.
export const monthLength = (year: number, month: number) =>
  new Date(Date.UTC(year, month, 0)).getUTCDate();

// The month some months after another ("2026-10" plus 3 is "2027-01").
export function monthsAfter(month: string, months: number): string {
  const [year = 0, m = 1] = month.split("-").map(Number);
  const index = year * 12 + (m - 1) + months;
  return `${Math.floor(index / 12)}-${pad((index % 12) + 1)}`;
}

// A month as the 6 weeks that show it, Sunday first: 42 plain days, with the end of the month before and the
// start of the month after filling the corners. The same size every month, so the calendar never changes height.
export function monthGrid(month: string): string[] {
  const first = `${month}-01`;
  const start = weekStart(first);
  return Array.from({ length: 42 }, (_, i) => daysAfter(start, i));
}

// A day written out for a screen reader: "Sunday 11 October 2026".
export function spokenDay(day: string): string {
  const [y, m, d] = dayParts(day);
  return `${weekdayNames[new Date(Date.UTC(y, m - 1, d)).getUTCDay()]} ${d} ${monthNames[m - 1]} ${y}`;
}
