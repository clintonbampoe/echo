import type { ReactNode } from "react";
import { DatePill, FilterBar, FilterSelect } from "@/ui";
import type { SessionLabel } from "../api";
import {
  countActive,
  kindOptions,
  statusOptions,
  typeOptions,
  windowOptions,
  type RecordValues,
} from "../records";
import styles from "./RecordsFilters.module.css";

type Props = {
  filters: RecordValues;
  people: boolean;
  labels: readonly SessionLabel[];
  onChange: (change: Partial<RecordValues>) => void;
  // Changes when the filters are cleared, so the search box empties too.
  resetKey: number;
  narrowed: boolean;
  onClear: () => void;
  // The Sessions / People switch, which sits right after the search box.
  view: ReactNode;
};

// The Records filters: search, the type of session, then status (for sessions) or kind (for people), with the days
// at the far right. A custom range adds two date pills after it.
export function RecordsFilters({
  filters,
  people,
  labels,
  onChange,
  resetKey,
  narrowed,
  onClear,
  view,
}: Props) {
  return (
    <FilterBar
      searchLabel={people ? "Search people" : "Search sessions"}
      search={filters.name}
      onSearch={(name) => onChange({ name })}
      resetKey={resetKey}
      active={countActive(filters, people)}
      filtered={narrowed}
      onClear={onClear}
      trailing={
        <span className={styles.window}>
          <FilterSelect
            label="This month"
            options={windowOptions}
            value={filters.window}
            quiet
            onChange={(window) => onChange({ window })}
          />
          {filters.window === "custom" && (
            <>
              <DatePill
                label="From"
                value={filters.from}
                range={{ from: filters.from, to: filters.to }}
                onChange={(from) => onChange({ from })}
              />
              <DatePill
                label="To"
                value={filters.to}
                range={{ from: filters.from, to: filters.to }}
                onChange={(to) => onChange({ to })}
              />
            </>
          )}
        </span>
      }
    >
      {view}
      <FilterSelect
        label="Type"
        options={typeOptions(labels)}
        value={filters.type}
        onChange={(type) => onChange({ type })}
      />
      {people ? (
        <FilterSelect
          label="Kind"
          options={kindOptions}
          value={filters.kind}
          onChange={(kind) => onChange({ kind })}
        />
      ) : (
        <FilterSelect
          label="Status"
          options={statusOptions}
          value={filters.status}
          onChange={(status) => onChange({ status })}
        />
      )}
    </FilterBar>
  );
}
