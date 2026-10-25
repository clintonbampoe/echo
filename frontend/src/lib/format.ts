import { ageOn, dayParts, daysBetween, monthNames } from "./dates";
import { getPreferences } from "./preferences";

const months = monthNames.map((name) => name.slice(0, 3));

// "2019-03-12" -> "12 Mar 2019". Dates from the API are plain days with no time zone, so this
// reads the text directly instead of going through a Date (which could shift the day).
export function formatDate(value: string): string {
  const [year, month, day] = value.slice(0, 10).split("-");
  return `${day} ${months[Number(month) - 1] ?? ""} ${year}`;
}

// "2019-03-12" -> "12 Mar". For a day inside a context that already says which year it is.
export function formatDayMonth(value: string): string {
  const [, month, day] = value.slice(0, 10).split("-");
  return `${Number(day)} ${months[Number(month) - 1] ?? ""}`;
}

// Whole numbers, with k from a thousand and m from a million: 640 -> "640", 1284 -> "1k", 2400000 -> "2m". The design
// shows every figure this way.
export function formatNumber(n: number): string {
  const sign = n < 0 ? "−" : "";
  const abs = Math.round(Math.abs(n));
  const thousands = Math.round(abs / 1000);
  if (abs < 1000) return `${sign}${abs}`;
  if (thousands < 1000) return `${sign}${thousands}k`;
  return `${sign}${Math.round(abs / 1_000_000)}m`;
}

// How many of something, with the right word: plural(1, 'asset') -> "1 asset", plural(3, 'asset') -> "3 assets".
export const plural = (n: number, one: string, many = `${one}s`): string =>
  `${formatNumber(n)} ${n === 1 ? one : many}`;

// "2026-10-11T09:04:00" -> "9:04 am". Read in the viewer's own time zone.
export function formatTime(value: string): string {
  return new Date(value)
    .toLocaleTimeString("en-GB", {
      hour: "numeric",
      minute: "2-digit",
      hour12: true,
    })
    .replace(/\s?(am|pm)/i, (_, half: string) => ` ${half.toLowerCase()}`);
}

// 6400 -> "GH₵ 6k" (or "6k" when the person turned the symbol off in Settings). Whole cedis, written like any figure.
export function formatMoney(amount: number): string {
  const figure = formatNumber(amount);
  return getPreferences().currencySymbol ? `GH₵ ${figure}` : figure;
}

// "08:42:00" -> "8:42 am". For a time of day the API sends on its own, with no date.
export function formatClock(value: string): string {
  const [h = "0", m = "00"] = value.split(":");
  const hour = Number(h);
  return `${hour % 12 || 12}:${m.slice(0, 2)} ${hour < 12 ? "am" : "pm"}`;
}

// "Ama Mensah" -> "Ama". Used to greet people.
export const firstName = (name: string) => name.trim().split(/\s+/)[0] ?? "";

// "2016-01-05" -> "Jan 2016". For when only the month matters, like the day someone joined a group.
export function formatMonth(value: string): string {
  const [year, month] = dayParts(value);
  return `${months[month - 1] ?? ""} ${year}`;
}

// "2026-11-14", "17:00:00" -> "Sat 14 Nov, 5:00 pm". With no time of day, "Sat 14 Nov".
export function formatDayTime(day: string, time?: string | null): string {
  const [year, month, date] = dayParts(day);
  const weekday = new Date(Date.UTC(year, month - 1, date)).toLocaleDateString(
    "en-GB",
    { weekday: "short", timeZone: "UTC" },
  );
  const when = `${weekday} ${date} ${months[month - 1] ?? ""}`;
  return time ? `${when}, ${formatClock(time)}` : when;
}

// How long ago a plain day was, in the biggest whole unit: "10 years ago", "3 months ago", "5 days ago".
export function formatAgo(day: string, today: string): string {
  const ago = (n: number, unit: string) =>
    `${n} ${unit}${n === 1 ? "" : "s"} ago`;
  const years = ageOn(day, today);
  if (years >= 1) return ago(years, "year");
  const [fy, fm, fd] = dayParts(day);
  const [ty, tm, td] = dayParts(today);
  const whole = (ty - fy) * 12 + (tm - fm) - (td < fd ? 1 : 0);
  if (whole >= 1) return ago(whole, "month");
  const days = daysBetween(day, today);
  if (days === 0) return "Today";
  return days === 1 ? "Yesterday" : ago(days, "day");
}
