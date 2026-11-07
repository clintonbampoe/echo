import { useState } from "react";
import { useMedia } from "./useMedia";

// The ranges a chart card's pill offers: the last six or twelve months.
export const monthRanges = [
  { value: "6", label: "6 months" },
  { value: "12", label: "12 months" },
] as const;
export type MonthRange = (typeof monthRanges)[number]["value"];

// The range a chart card draws. A phone, held either way up, has room for six months and never more; a tablet and
// a desktop offer twelve too. A phone on its side is wide but short, which no tablet is.
export function useMonthRange(initial: MonthRange = "6") {
  const phone = useMedia("(max-width: 700px), (max-height: 500px)");
  const [chosen, setRange] = useState<MonthRange>(initial);
  const range: MonthRange = phone ? "6" : chosen;
  const options = phone ? monthRanges.slice(0, 1) : monthRanges;
  return { range, setRange, options };
}
