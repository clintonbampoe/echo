import { formatMoney } from "@/lib/format";
import { ProgressBar } from "@/ui";
import type { Project } from "../api";
import { remaining } from "../contributionView";
import { percentRaised } from "../view";
import styles from "./ProjectProgress.module.css";

// How far along a project is: what has been raised against the target, what is left, and a bar. Past the target the
// bar is full, the percent says how far past, and "left" turns into how much over.
export function ProjectProgress({ project: p }: { project: Project }) {
  const percent = percentRaised(p);
  const left = remaining(p);
  return (
    <section aria-label="Raised against target" className={styles.card}>
      <div className={styles.top}>
        <div>
          <p className={styles.label}>Raised</p>
          <p className={styles.raised}>
            {formatMoney(p.raisedAmount)}
            <span className={styles.of}>of {formatMoney(p.targetAmount)}</span>
          </p>
        </div>
        <div className={styles.left}>
          <p className={styles.label}>{left.label}</p>
          <p className={styles.leftValue}>{formatMoney(left.amount)}</p>
        </div>
      </div>
      <div className={styles.bar}>
        <ProgressBar value={percent / 100} label="Raised against target" />
        <span className={styles.percent}>{percent}%</span>
      </div>
    </section>
  );
}
