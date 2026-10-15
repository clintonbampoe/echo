import { useState } from "react";
import { useUrlFilters } from "@/lib/useUrlFilters";
import { Button, EmptyState, Icon, ListBody, PageHeader } from "@/ui";
import { FilterBar } from "./FilterBar";
import { organizationFilters, toApiFilters, toSummaryFilters } from "./filters";
import { useOrganizations, useOrganizationSummary } from "./hooks";
import { OrganizationList } from "./OrganizationList";
import { OrganizationSheet } from "./OrganizationSheet";
import { SummaryStrip } from "./SummaryStrip";

// The organizations list: header, numbers, filters and the cards. The page only decides what a card is.
export function OrganizationsPage() {
  const { filters, update, clear, resetKey } =
    useUrlFilters(organizationFilters);
  const [adding, setAdding] = useState(false);
  const list = useOrganizations(toApiFilters(filters));
  const summary = useOrganizationSummary(toSummaryFilters(filters));
  const filtered = organizationFilters.isFiltered(filters);
  const none =
    !list.loading && !list.failed && list.rows.length === 0 && !filtered;
  const add = (
    <Button onClick={() => setAdding(true)}>
      <Icon name="plus" size={16} />
      New organization
    </Button>
  );

  return (
    <div className="anim-rise">
      <PageHeader
        title="Organizations"
        text="Ministries, groups and teams, and the people in them."
        action={add}
      />
      {none ? (
        <EmptyState
          icon="organizations"
          title="Create your first organization"
          action={add}
        >
          Organizations are the ministries, groups and teams in your church,
          like the choir, ushers or youth. Add one, then add its members.
        </EmptyState>
      ) : (
        <>
          <SummaryStrip summary={summary.data} />
          <FilterBar
            filters={filters}
            onChange={update}
            resetKey={resetKey}
            onClear={clear}
          />
          <ListBody
            list={list}
            icon="organizations"
            noun={["organization", "organizations"]}
            emptyText="Organizations you add will appear here."
            filtered={filtered}
            onClear={clear}
          >
            {(organizations) => (
              <OrganizationList organizations={organizations} />
            )}
          </ListBody>
        </>
      )}
      <OrganizationSheet open={adding} onClose={() => setAdding(false)} />
    </div>
  );
}
