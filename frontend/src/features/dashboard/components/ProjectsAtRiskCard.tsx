import { Chip } from "@/ui";
import { useProjectsAtRisk } from "../hooks";
import { atRiskWords, riskRows } from "../projects";
import { CardFailed } from "./CardFailed";
import { DashCard, SeeLink } from "./DashCard";
import styles from "./ProjectsAtRiskCard.module.css";

// The projects whose status the server has marked At Risk, all time and however they are going.
// A narrow card leads with "3 of 5"; a wide one is just the list.
export function ProjectsAtRiskCard() {
  const data = useProjectsAtRisk();
  const rows = riskRows(data.projects);
  const figure = atRiskWords(data.totals);
  return (
    <DashCard
      title="Projects at risk"
      to="/projects"
      control={<SeeLink to="/projects">All projects</SeeLink>}
    >
      {data.failed ? (
        <CardFailed onRetry={data.retry} />
      ) : (
        <div className={styles.content} aria-busy={!data.projects}>
          {figure && (
            <div className={styles.figure}>
              <p className={styles.big}>{figure}</p>
              <p className={styles.note}>of every project, all time</p>
            </div>
          )}
          {!data.projects ? (
            <ul aria-busy="true" className={styles.list}>
              {[0, 1, 2].map((i) => (
                <li key={i} className={styles.row}>
                  <span className={styles.ghost} />
                </li>
              ))}
            </ul>
          ) : rows.length === 0 ? (
            <p className={styles.empty}>No projects at risk</p>
          ) : (
            <ul className={styles.list}>
              {rows.map((r) => (
                <li key={r.key} className={styles.row}>
                  <div className={styles.text}>
                    <strong className={styles.name}>{r.name}</strong>
                    <span className={styles.when}>{r.note}</span>
                  </div>
                  <Chip tone="warning">At risk</Chip>
                </li>
              ))}
            </ul>
          )}
        </div>
      )}
    </DashCard>
  );
}
