import { useEffect, useState } from "react";
import { useDebounced } from "@/lib/useDebounced";
import { SearchField } from "./SearchField";

type Props = {
  // Names the box for screen readers and is its placeholder, e.g. "Search members".
  label: string;
  // What the list is currently searched by.
  value: string;
  onCommit: (value: string) => void;
  className?: string;
};

// Shows what was typed straight away and tells the page once typing pauses, so the list
// is not asked for a new page on every key. To empty it from outside, give it a new `key`.
export function SearchBox({ label, value, onCommit, className }: Props) {
  const [draft, setDraft] = useState(value);
  const settled = useDebounced(draft);
  useEffect(() => {
    if (settled !== value) onCommit(settled);
  }, [settled, value, onCommit]);
  return (
    <SearchField
      label={label}
      placeholder={label}
      value={draft}
      onChange={(e) => setDraft(e.target.value)}
      className={className}
    />
  );
}
