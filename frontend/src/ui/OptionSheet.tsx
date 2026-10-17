import { useState } from "react";
import { BottomSheet } from "./BottomSheet";
import { Icon } from "./Icon";
import { filterOptions, SEARCH_MIN } from "./options";
import { SearchField } from "./SearchField";
import type { Option } from "./selectTypes";
import styles from "./OptionSheet.module.css";

type Props = {
  open: boolean;
  onClose: () => void;
  // The field's or filter's name, e.g. "Region".
  title: string;
  options: readonly Option[];
  value: string;
  onPick: (value: string) => void;
};

// The choice list for a touch screen: a bottom sheet with 52px rows. A long list gets a search box first.
export function OptionSheet({ open, onClose, title, ...body }: Props) {
  return (
    <BottomSheet open={open} onClose={onClose} title={title}>
      <Body title={title} onClose={onClose} {...body} />
    </BottomSheet>
  );
}

// Lives inside the sheet, so what was typed in the search box is gone when the sheet closes.
function Body({ title, options, value, onPick, onClose }: Omit<Props, "open">) {
  const [query, setQuery] = useState("");
  const shown = filterOptions(options, query);

  return (
    <>
      {options.length >= SEARCH_MIN && (
        <SearchField
          label={`Search ${title.toLowerCase()}`}
          placeholder="Search"
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          autoComplete="off"
          className={styles.search}
        />
      )}
      <div role="listbox" aria-label={title} className={styles.list}>
        {shown.map((o) => (
          <button
            key={o.value}
            type="button"
            role="option"
            aria-selected={o.value === value}
            className={`${styles.row} ${o.value === value ? styles.selected : ""}`}
            onClick={() => {
              onPick(o.value);
              onClose();
            }}
          >
            <span>{o.label}</span>
            {o.value === value && (
              <Icon name="check" size={18} className={styles.check} />
            )}
          </button>
        ))}
      </div>
      {!shown.length && (
        <p role="status" className={styles.none}>
          No matches
        </p>
      )}
    </>
  );
}
