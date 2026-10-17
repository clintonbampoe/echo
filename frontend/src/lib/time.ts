// A time of day as plain "08:05" text (24 hours), the way the API and the forms carry it.
const pad = (n: number) => String(n).padStart(2, "0");

export type Clock = { hour: number; minute: number; pm: boolean };

// "14:05" -> 2:05 pm. The hour is 1 to 12.
export function toClock(time: string): Clock {
  const [h = 0, m = 0] = time.split(":").map(Number);
  return { hour: h % 12 || 12, minute: m, pm: h >= 12 };
}

// 2:05 pm -> "14:05".
export function fromClock({ hour, minute, pm }: Clock): string {
  return `${pad((hour % 12) + (pm ? 12 : 0))}:${pad(minute)}`;
}

// "14:05" -> "2:05 pm".
export function spoken(time: string): string {
  const { hour, minute, pm } = toClock(time);
  return `${hour}:${pad(minute)} ${pm ? "pm" : "am"}`;
}

// The time on a clock now, as "HH:mm", in the viewer's own time zone.
export const timeNow = (now = new Date()) =>
  `${pad(now.getHours())}:${pad(now.getMinutes())}`;
