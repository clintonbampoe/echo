import { useId, useState, type ReactNode } from "react";
import { Icon } from "./Icon";
import { SearchBox } from "./SearchBox";
import styles from "./FilterBar.module.css";

type Props = {
  // Names the search box, e.g. "Search members".
  searchLabel: string;
  search: string;
  onSearch: (value: string) => void;
  // Changes when the filters are cleared, so the search box empties too.
  resetKey: number;
  // How many filters (not counting the search box) are narrowing the list.
  active: number;
  // True when anything at all, search included, is narrowing the list.
  filtered: boolean;
  onClear: () => void;
  // The filter pills of this list.
  children: ReactNode;
  // Sits at the right end of the row on a wide screen, e.g. the time range.
  trailing?: ReactNode;
};

// The bar above a list. On a wide screen everything sits in one row. On a phone or tablet the pills fold
// away behind a "Filters" button, so the list is the first thing people see.
export function FilterBar({
  searchLabel,
  search,
  onSearch,
  resetKey,
  active,
  filtered,
  onClear,
  children,
  trailing,
}: Props) {
  const [open, setOpen] = useState(false);
  // True once the panel has finished opening. Until then it clips its content so nothing spills over the list;
  // afterwards it lets a menu (opened with a mouse on a narrow window) hang below the pills.
  const [settled, setSettled] = useState(false);
  const panelId = useId();

  function toggle() {
    setOpen(!open);
    setSettled(false);
  }

  return (
    <div className={styles.bar}>
      <div className={styles.top}>
        <SearchBox
          key={resetKey}
          label={searchLabel}
          value={search}
          onCommit={onSearch}
          className={styles.search}
        />
        <button
          type="button"
          aria-expanded={open}
          aria-controls={panelId}
          className={styles.toggle}
          onClick={toggle}
        >
          <Icon name="filter" size={18} />
          Filters
          {active > 0 && (
            <span className={styles.count} aria-label={`${active} applied`}>
              {active}
            </span>
          )}
        </button>
      </div>
      {filtered && (
        <p className={styles.applied}>
          Filters applied
          <button type="button" className={styles.link} onClick={onClear}>
            Clear all
          </button>
        </p>
      )}
      <div
        id={panelId}
        className={`${styles.panel} ${open ? styles.open : ""} ${open && settled ? styles.settled : ""}`}
        onTransitionEnd={(e) =>
          e.target === e.currentTarget && setSettled(open)
        }
      >
        <div className={styles.pills}>
          {children}
          {trailing && <div className={styles.trailing}>{trailing}</div>}
        </div>
      </div>
    </div>
  );
}
