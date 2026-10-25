import type { Option } from "@/lib/options";

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
