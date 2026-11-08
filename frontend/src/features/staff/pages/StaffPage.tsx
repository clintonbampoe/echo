import { useState } from "react";
import { useUrlFilters } from "@/lib/useUrlFilters";
import { PageHeader } from "@/ui";
import { RoleCard } from "../components/RoleCard";
import { RoleOverview } from "../components/RoleOverview";
import {
  StaffActionHost,
  type StaffAction,
} from "../components/StaffActionHost";
import { StaffFilterBar } from "../components/StaffFilterBar";
import { useStaffSummary } from "../hooks";
import { staffLine } from "../model/figures";
import { staffFilters, toQuery } from "../model/filters";
import { roleOrder } from "../model/roles";
import styles from "./StaffPage.module.css";

// Admin only: everyone in the congregation with an account, grouped by role. The server turns anyone else away.
export function StaffPage() {
  const summary = useStaffSummary();
  const { filters, resetKey, update, clear } = useUrlFilters(staffFilters);
  const [action, setAction] = useState<StaffAction | null>(null);
  const query = toQuery(filters);
  // "See all" keeps the search and status, so the role's page opens already narrowed.
  const search = staffFilters.write(filters).toString();

  return (
    <div className={`${styles.page} anim-rise`}>
      <PageHeader
        title="Staff"
        badge="Admin only"
        text={staffLine(summary.data)}
      />
      <RoleOverview summary={summary.data} />
      <StaffFilterBar
        filters={filters}
        searchLabel="Search staff"
        resetKey={resetKey}
        onChange={update}
        onClear={clear}
      />
      <div className={styles.cards}>
        {roleOrder.map((role) => (
          <RoleCard
            key={role}
            role={role}
            query={query}
            search={search}
            onAct={(kind, user) => setAction({ kind, user })}
          />
        ))}
      </div>
      <StaffActionHost
        action={action}
        onAct={(kind, user) => setAction({ kind, user })}
        onClose={() => setAction(null)}
      />
    </div>
  );
}
