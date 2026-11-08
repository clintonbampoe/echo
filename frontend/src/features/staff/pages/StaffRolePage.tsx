import { useState } from "react";
import { Link, Navigate, useParams } from "react-router-dom";
import type { UserRole } from "@/features/account";
import { useUrlFilters } from "@/lib/useUrlFilters";
import { Icon, ListBody, PageHeader } from "@/ui";
import { RoleStrip } from "../components/RoleStrip";
import {
  StaffActionHost,
  type StaffAction,
} from "../components/StaffActionHost";
import { StaffFilterBar } from "../components/StaffFilterBar";
import { StaffRoleList } from "../components/StaffRoleList";
import { useRoleList, useStaffSummary } from "../hooks";
import { staffFilters, toQuery } from "../model/filters";
import { roleFromSlug, rolePlural } from "../model/roles";
import styles from "./StaffRolePage.module.css";

// /staff/clerk: everyone who holds one role, with that role's head counts and a search of its own. An address that
// names no role goes back to Staff.
export function StaffRolePage() {
  const role = roleFromSlug(useParams().role);
  return role ? <RolePageBody role={role} /> : <Navigate to="/staff" replace />;
}

function RolePageBody({ role }: { role: UserRole }) {
  const summary = useStaffSummary();
  const { filters, resetKey, update, clear } = useUrlFilters(staffFilters);
  const [action, setAction] = useState<StaffAction | null>(null);
  const list = useRoleList(role, toQuery(filters));
  const plural = rolePlural(role);

  return (
    <div className={`${styles.page} anim-rise`}>
      <Link to="/staff" className={styles.back}>
        <Icon name="chevron-left" size={16} />
        Staff
      </Link>
      <PageHeader title={plural} badge="Admin only" />
      <RoleStrip role={role} summary={summary.data} />
      <StaffFilterBar
        filters={filters}
        searchLabel={`Search ${plural.toLowerCase()}`}
        resetKey={resetKey}
        onChange={update}
        onClear={clear}
      />
      <ListBody
        list={list}
        icon="team"
        noun={["person", "people"]}
        emptyText={`People you give the ${plural.toLowerCase().slice(0, -1)} role will appear here.`}
        filtered={staffFilters.isFiltered(filters)}
        noMatchText="Try a different search or status."
        onClear={clear}
        footerInside
        loaded
      >
        {(rows, footer) => (
          <StaffRoleList
            role={role}
            rows={rows}
            footer={footer}
            onAct={(kind, user) => setAction({ kind, user })}
          />
        )}
      </ListBody>
      <StaffActionHost
        action={action}
        onAct={(kind, user) => setAction({ kind, user })}
        onClose={() => setAction(null)}
      />
    </div>
  );
}
