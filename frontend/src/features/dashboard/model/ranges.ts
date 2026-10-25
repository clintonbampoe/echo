// The ranges a chart card's pill offers: the last six or twelve months.
export const monthRanges = [
  { value: "6", label: "6 months" },
  { value: "12", label: "12 months" },
] as const;
export type MonthRange = (typeof monthRanges)[number]["value"];
