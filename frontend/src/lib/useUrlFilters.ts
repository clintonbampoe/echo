import { useState } from "react";
import { useSearchParams } from "react-router-dom";
import type { FilterValues, UrlFilters } from "./urlFilters";

// The filters of a list page, kept in the address. `update` changes some of them, `clear` resets all of
// them. `resetKey` changes on every clear: give it as the `key` of a search box so the box empties too.
export function useUrlFilters<T extends FilterValues>(def: UrlFilters<T>) {
  const [params, setParams] = useSearchParams();
  const [resetKey, setResetKey] = useState(0);
  const filters = def.read(params);
  return {
    filters,
    resetKey,
    update: (change: Partial<T>) =>
      setParams(def.write({ ...filters, ...change }), { replace: true }),
    clear: () => {
      setParams(def.write(def.empty), { replace: true });
      setResetKey(resetKey + 1);
    },
  };
}
