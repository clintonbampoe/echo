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
    update: (change: Partial<T>) =>
      setParams(def.write({ ...filters, ...change }), { replace: true }),
    clear: () => {
      setParams(def.write(def.empty), { replace: true });
      setClearing(true);
    },
  };
}
