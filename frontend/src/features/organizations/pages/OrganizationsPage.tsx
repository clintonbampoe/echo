import { useState } from "react";
import { useUrlFilters } from "@/lib/useUrlFilters";
import { Button, EmptyState, Icon, ListBody, PageHeader } from "@/ui";
import { OrganizationFilterBar } from "../components/list/OrganizationFilterBar";
import {
  organizationFilters,
  toApiFilters,
  toSummaryFilters,
} from "../model/filters";
import { useOrganizations, useOrganizationSummary } from "../hooks/queries";
import { OrganizationList } from "../components/list/OrganizationList";
import { OrganizationSheet } from "../components/form/OrganizationSheet";
import { OrganizationSummary } from "../components/list/OrganizationSummary";

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
        add={{ label: "New organization", onClick: () => setAdding(true) }}
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
          <OrganizationSummary summary={summary.data} />
          <OrganizationFilterBar
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
