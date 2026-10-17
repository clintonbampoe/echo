import type { Option } from "./selectTypes";

// A list where each value is its own label: plainOptions(['Male', 'Female']).
export const plainOptions = (values: readonly string[]): Option[] =>
  values.map((value) => ({ value, label: value }));

// A filter's list: the choices with "Any ..." in front, whose value is empty (meaning no filter).
export const withAny = (
  label: string,
  options: readonly Option[],
): Option[] => [{ value: "", label }, ...options];

// A list this long gets a search box above it (design rule S10).
export const SEARCH_MIN = 8;

// The options whose label contains what was typed, ignoring case and spaces at the ends.
export function filterOptions(
  options: readonly Option[],
  query: string,
): readonly Option[] {
  const text = query.trim().toLowerCase();
  return text
    ? options.filter((o) => o.label.toLowerCase().includes(text))
    : options;
}
