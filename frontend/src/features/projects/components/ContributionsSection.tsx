import type { UseQueryResult } from "@tanstack/react-query";
import type { Paged } from "@/lib/paged";
import { Button, EmptyState, FilterSelect, Icon } from "@/ui";
import type { Contribution, ContributionSummary as Summary } from "../api";
import {
  contributionFilters,
  contributionStats,
  givenOptions,
  methodOptions,
  type GivenFilters,
} from "../contributionView";
import type { ContributionStart } from "./ContributionDialogs";
import { ContributionList } from "./ContributionList";
import styles from "./ContributionsSection.module.css";
import { ContributionSummary } from "./ContributionSummary";

type Props = {
  filters: GivenFilters;
  onChange: (change: Partial<GivenFilters>) => void;
  list: Paged<Contribution>;
  summary: UseQueryResult<Summary>;
  onRecord: () => void;
  onAction: (contribution: Contribution, start: ContributionStart) => void;
};

// What has been given towards the project: the numbers, a Method and period filter, and every gift. With none at all
// the whole section gives way to an invitation, because there is nothing to filter.
export function ContributionsSection({
  filters,
  onChange,
  list,
  summary,
  onRecord,
  onAction,
}: Props) {
  const none =
    !list.loading &&
    !list.failed &&
    list.rows.length === 0 &&
    !contributionFilters.isFiltered(filters);
  if (none) {
    return (
      <EmptyState
        compact
        icon="projects"
        title="No contributions yet"
        action={
          <Button onClick={onRecord}>
            <Icon name="plus" size={16} />
            Record contribution
          </Button>
        }
      >
        Record what comes in towards this project. Contributions are not linked
        to a member.
      </EmptyState>
    );
  }
  return (
    <section aria-label="Contributions">
      <h2 className={styles.title}>Contributions</h2>
      <ContributionSummary
        stats={contributionStats(summary.data)}
        busy={summary.isPending}
      />
      <div className={styles.bar}>
        <FilterSelect
          label="Method"
          options={methodOptions}
          value={filters.method}
          onChange={(method) => onChange({ method })}
        />
        <FilterSelect
          label="All time"
          options={givenOptions}
          value={filters.window}
          quiet
          onChange={(window) => onChange({ window })}
        />
      </div>
      <ContributionList list={list} onAction={onAction} />
    </section>
  );
}
