// Filters that live in the address bar (?status=Active&gender=Female), so a filtered list can be
// bookmarked, shared, and survives a reload. Every list page describes its filters once and gets reading,
// writing and "is anything narrowing the list" from here. No React in this file.
export type FilterValues = Record<string, string>;

export type UrlFilters<T extends FilterValues> = {
  // Every filter at its default: nothing narrowing the list.
  empty: T;
  read: (params: URLSearchParams) => T;
  write: (values: T) => URLSearchParams;
  isFiltered: (values: T) => boolean;
};

// `empty` names every filter and its default. `allowed` lists the only values a filter may hold (a
// time range, say); anything else in the address falls back to the default.
export function urlFilters<T extends FilterValues>(
  empty: T,
  allowed: { [K in keyof T]?: readonly string[] } = {},
): UrlFilters<T> {
  const keys = Object.keys(empty) as (keyof T & string)[];
  const valid = (key: keyof T & string, value: string | null) => {
    if (value === null) return false;
    const only = allowed[key];
    return only ? only.includes(value) : true;
  };
  return {
    empty,
    read: (params) =>
      Object.fromEntries(
        keys.map((k) => [
          k,
          valid(k, params.get(k)) ? params.get(k) : empty[k],
        ]),
      ) as T,
    // Only what differs from the defaults goes in the address.
    write: (values) => {
      const params = new URLSearchParams();
      for (const k of keys) {
        const value = values[k];
        if (value !== undefined && value !== empty[k]) params.set(k, value);
      }
      return params;
    },
    isFiltered: (values) => keys.some((k) => values[k] !== empty[k]),
  };
}
