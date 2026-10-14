// "This year / Last year / All time" for lists of things with a date. The server's default is this year, so
// "This year" sends no dates at all. Pure, so the tests can pass "now".
export type WindowKey = "year" | "last-year" | "all";

export const windowKeys: readonly WindowKey[] = ["year", "last-year", "all"];

export const windowOptions: readonly { value: WindowKey; label: string }[] = [
  { value: "year", label: "This year" },
  { value: "last-year", label: "Last year" },
  { value: "all", label: "All time" },
];

export function windowDates(
  key: string,
  now = new Date(),
): { from?: string; to?: string } {
  const year = now.getFullYear();
  if (key === "last-year")
    return { from: `${year - 1}-01-01`, to: `${year - 1}-12-31` };
  if (key === "all") return { from: "1900-01-01", to: "2999-12-31" };
  return {};
}
