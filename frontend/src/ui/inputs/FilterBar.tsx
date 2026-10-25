import { useId, useState, type ReactNode } from "react";
import { Icon } from "../layout/Icon";
import { SearchBox } from "./SearchBox";
import styles from "./FilterBar.module.css";

type Props = {
  // Names the search box, e.g. "Search members". Left out by a list the server cannot search.
  searchLabel?: string;
  search?: string;
  onSearch?: (value: string) => void;
  // Takes the search box's place when there is none, e.g. an Income / Expenses switch.
  leading?: ReactNode;
  // Changes when the filters are cleared, so the search box empties too. A bar with no search has none.
  resetKey?: number;
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

type LeadProps = Pick<
  Props,
  "searchLabel" | "search" | "onSearch" | "leading"
> & { resetKey: number };

// The left of the bar: the search box, or what a list the server cannot search puts in its place.
function Lead({ searchLabel, search, onSearch, resetKey, leading }: LeadProps) {
  if (searchLabel === undefined || search === undefined || !onSearch)
    return leading ? <div className={styles.search}>{leading}</div> : null;
  return (
    <SearchBox
      key={resetKey}
      label={searchLabel}
      value={search}
      onCommit={onSearch}
      className={styles.search}
    />
  );
}

// The bar above a list. On a wide screen everything sits in one row. On a phone or tablet the pills fold
// away behind a "Filters" button, so the list is the first thing people see.
export function FilterBar({
  searchLabel,
  search,
  onSearch,
  leading,
  resetKey = 0,
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
        <Lead
          searchLabel={searchLabel}
          search={search}
          onSearch={onSearch}
          resetKey={resetKey}
          leading={leading}
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
