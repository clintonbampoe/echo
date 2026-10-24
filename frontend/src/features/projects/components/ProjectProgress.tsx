import { formatDate, formatMoney } from "@/lib/format";
import { Chip, ProgressBar } from "@/ui";
import type { Project } from "../api";
import { remaining } from "../contributionView";
import { statusLabel, statusTone } from "../labels";
import { percentRaised } from "../view";
import styles from "./ProjectProgress.module.css";

// How far along a project is: the share of the target raised, its status, when it ends, and a bar. Past the target
// the bar is full and the percent says how far past.
export function ProjectProgress({ project: p }: { project: Project }) {
  const percent = percentRaised(p);
  const left = remaining(p);
  return (
    <section aria-label="Raised against target" className={styles.card}>
      <div className={styles.top}>
        <p className={styles.percent}>
          {percent}%<span className={styles.funded}> funded</span>
        </p>
        {p.status !== "OnTrack" && (
          <Chip tone={statusTone[p.status]}>{statusLabel[p.status]}</Chip>
        )}
        {p.endDate && (
          <p className={styles.ends}>Ends {formatDate(p.endDate)}</p>
        )}
      </div>
      <p className={styles.left}>
        {left.label} {formatMoney(left.amount)}
      </p>
      <ProgressBar
        value={Math.min(percent, 100) / 100}
        label="Raised against target"
      />
    </section>
  );
}
