import { useState } from "react";
import { useUrlFilters } from "@/lib/useUrlFilters";
import { PageHeader } from "@/ui";
import { RoleBar } from "../components/RoleBar";
import { RoleCard } from "../components/RoleCard";
import { TeamActionHost, type TeamAction } from "../components/TeamActionHost";
import { TeamFilterBar } from "../components/TeamFilterBar";
import { useTeamSummary } from "../hooks";
import { teamFilters, toStatus } from "../model/filters";
import { countOf, roleOrder } from "../model/roles";
import { shareOf, teamLine } from "../model/figures";
import styles from "./TeamPage.module.css";

// Admin only: everyone in the congregation with an account, grouped by role. The server turns anyone else away.
export function TeamPage() {
  const summary = useTeamSummary();
  const { filters, update, clear } = useUrlFilters(teamFilters);
  const [action, setAction] = useState<TeamAction | null>(null);
  const roles = summary.data?.roles;
  const total = summary.data?.total ?? 0;

  return (
    <div className={`${styles.page} anim-rise`}>
      <PageHeader
        title="Team"
        badge="Admin only"
        text={teamLine(summary.data)}
      />
      <RoleBar summary={summary.data} />
      <TeamFilterBar filters={filters} onChange={update} onClear={clear} />
      <div className={styles.cards}>
        {roleOrder.map((role) => (
          <RoleCard
            key={role}
            role={role}
            count={countOf(roles, role)}
            share={shareOf(countOf(roles, role), total)}
            status={toStatus(filters)}
            onAct={(kind, user) => setAction({ kind, user })}
          />
        ))}
      </div>
      <TeamActionHost
        action={action}
        onAct={(kind, user) => setAction({ kind, user })}
        onClose={() => setAction(null)}
      />
    </div>
  );
}
