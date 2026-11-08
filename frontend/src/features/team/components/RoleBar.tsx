import { DistributionBar, ListHeader } from "@/ui";
import type { TeamSummary } from "../api/team";
import { roleParts } from "../model/roles";
import styles from "./RoleBar.module.css";

// One bar of everyone, split by role. The legend underneath is the real content; the bar is its picture.
export function RoleBar({ summary }: { summary: TeamSummary | undefined }) {
  return (
    <section className={styles.card} aria-label="Roles">
      <ListHeader title="Roles" />
      <div className={styles.body}>
        {summary ? (
          <DistributionBar
            label="People by role"
            parts={roleParts(summary.roles)}
            empty="Nobody has joined yet."
          />
        ) : (
          <span className={`skeleton ${styles.skeleton}`} aria-hidden="true" />
        )}
      </div>
    </section>
  );
}
