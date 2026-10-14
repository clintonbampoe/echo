import { monthNames } from "./dates";

const months = monthNames.map((name) => name.slice(0, 3));

// "2019-03-12" -> "12 Mar 2019". Dates from the API are plain days with no time zone, so this
// reads the text directly instead of going through a Date (which could shift the day).
export function formatDate(value: string): string {
  const [year, month, day] = value.slice(0, 10).split("-");
  return `${day} ${months[Number(month) - 1] ?? ""} ${year}`;
}

// 1284 -> "1,284"
export const formatNumber = (n: number): string => n.toLocaleString("en-GB");

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

// 6400 -> "GH₵ 6,400". Whole cedis when there are no pesewas, two places otherwise.
export function formatMoney(amount: number): string {
  const whole = Number.isInteger(amount);
  return `GH₵ ${amount.toLocaleString("en-GB", { minimumFractionDigits: whole ? 0 : 2, maximumFractionDigits: 2 })}`;
}

// "08:42:00" -> "8:42 am". For a time of day the API sends on its own, with no date.
export function formatClock(value: string): string {
  const [h = "0", m = "00"] = value.split(":");
  const hour = Number(h);
  return `${hour % 12 || 12}:${m.slice(0, 2)} ${hour < 12 ? "am" : "pm"}`;
}

// "Ama Mensah" -> "Ama". Used to greet people.
export const firstName = (name: string) => name.trim().split(/\s+/)[0] ?? "";
