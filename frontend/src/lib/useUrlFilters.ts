import { useState } from "react";
import { useSearchParams } from "react-router-dom";
import type { FilterValues, UrlFilters } from "./urlFilters";

// The filters of a list page, kept in the address. `update` changes some of them, `clear` resets all of
// them. `resetKey` changes after a clear: give it as the `key` of a search box so the box empties too.
export function useUrlFilters<T extends FilterValues>(def: UrlFilters<T>) {
  const [params, setParams] = useSearchParams();
  const [resetKey, setResetKey] = useState(0);
  const [clearing, setClearing] = useState(false);
  const filters = def.read(params);
  // Writes the filters and leaves the rest of the query alone (the open tab, a linked record). `also` sets other
  // parts of the query in the same step: two separate writes in one click would each start from the old query.
  const write = (values: T, also: Record<string, string> = {}) =>
    setParams(
      (current) => {
        const next = new URLSearchParams(current);
        for (const key of Object.keys(def.empty)) next.delete(key);
        for (const [key, value] of def.write(values)) next.append(key, value);
        for (const [key, value] of Object.entries(also)) next.set(key, value);
        return next;
      },
      { replace: true, preventScrollReset: true },
    );
  // The address changes a moment after `clear` runs. The search box is emptied by being rebuilt, and a box
  // rebuilt too early would start from the old search again. So the key changes only once the address has
  // really lost its filters.
  if (clearing && !def.isFiltered(filters)) {
    setClearing(false);
    setResetKey(resetKey + 1);
  }
  return {
    filters,
    resetKey,
    update: (change: Partial<T>, also?: Record<string, string>) =>
      write({ ...filters, ...change }, also),
    clear: () => {
      write(def.empty);
      setClearing(true);
    },
  };
}
