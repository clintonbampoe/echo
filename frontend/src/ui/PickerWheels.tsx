import { monthLength, monthNames } from "@/lib/dates";
import { fromClock, toClock, type Clock } from "@/lib/time";
import type { Option } from "./selectTypes";
import { Wheel, WheelSet } from "./Wheel";

const range = (from: number, to: number): number[] =>
  Array.from({ length: to - from + 1 }, (_, i) => from + i);
const pad = (n: number) => String(n).padStart(2, "0");
const asOptions = (
  values: number[],
  label: (n: number) => string = (n) => String(n),
): Option[] => values.map((v) => ({ value: String(v), label: label(v) }));

const hours = asOptions(range(1, 12));
const minutes = asOptions(range(0, 59), pad);
const halves: Option[] = [
  { value: "am", label: "am" },
  { value: "pm", label: "pm" },
];
const shortMonths = asOptions(range(1, 12), (m) =>
  (monthNames[m - 1] ?? "").slice(0, 3),
);

// Hour, minute and am/pm. The time is "14:05" text, as everywhere else.
export function TimeWheels({
  value,
  onChange,
  autoFocus,
}: {
  value: string;
  onChange: (time: string) => void;
  autoFocus?: boolean;
}) {
  const clock = toClock(value);
  const set = (change: Partial<Clock>) =>
    onChange(fromClock({ ...clock, ...change }));
  return (
    <WheelSet heads={["Hour", "Minute", ""]}>
      <Wheel
        label="Hour"
        options={hours}
        value={String(clock.hour)}
        onChange={(v) => set({ hour: Number(v) })}
        autoFocus={autoFocus}
      />
      <Wheel
        label="Minute"
        options={minutes}
        value={String(clock.minute)}
        onChange={(v) => set({ minute: Number(v) })}
      />
      <Wheel
        label="am or pm"
        options={halves}
        value={clock.pm ? "pm" : "am"}
        onChange={(v) => set({ pm: v === "pm" })}
      />
    </WheelSet>
  );
}

type Years = readonly [from: number, to: number];

// A month and a year, the same two wheels wherever a month is picked.
export function MonthYearWheels({
  year,
  month,
  years,
  onChange,
  autoFocus,
}: {
  year: number;
  month: number;
  years: Years;
  onChange: (year: number, month: number) => void;
  autoFocus?: boolean;
}) {
  const yearOptions = asOptions(range(years[0], years[1]));
  return (
    <WheelSet heads={["Month", "Year"]}>
      <Wheel
        label="Month"
        options={shortMonths}
        value={String(month)}
        onChange={(v) => onChange(year, Number(v))}
        autoFocus={autoFocus}
      />
      <Wheel
        label="Year"
        options={yearOptions}
        value={String(year)}
        onChange={(v) => onChange(Number(v), month)}
      />
    </WheelSet>
  );
}

// Day, month and year, for a date far back such as a birthday, where a calendar would be many months of tapping.
export function DayMonthYearWheels({
  day,
  years,
  onChange,
  autoFocus,
}: {
  day: string;
  years: Years;
  onChange: (day: string) => void;
  autoFocus?: boolean;
}) {
  const [year = 2000, month = 1, date = 1] = day.split("-").map(Number);
  const yearOptions = asOptions(range(years[0], years[1]));
  const dayOptions = asOptions(range(1, monthLength(year, month)));
  // A shorter month takes the day back to its last (31 March becomes 28 February).
  const set = (y: number, m: number, d: number) =>
    onChange(`${y}-${pad(m)}-${pad(Math.min(d, monthLength(y, m)))}`);
  return (
    <WheelSet heads={["Day", "Month", "Year"]}>
      <Wheel
        label="Day"
        options={dayOptions}
        value={String(date)}
        onChange={(v) => set(year, month, Number(v))}
        autoFocus={autoFocus}
      />
      <Wheel
        label="Month"
        options={shortMonths}
        value={String(month)}
        onChange={(v) => set(year, Number(v), date)}
      />
      <Wheel
        label="Year"
        options={yearOptions}
        value={String(year)}
        onChange={(v) => set(Number(v), month, date)}
      />
    </WheelSet>
  );
}
