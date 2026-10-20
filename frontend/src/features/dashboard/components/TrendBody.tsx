import type { ReactNode } from "react";
import styles from "./TrendBody.module.css";

type Props = {
  chart: ReactNode;
  // Three figures under the chart. A missing value shows as a dash.
  figures: readonly { label: string; value?: string }[];
  // One line under them. Its slot is always there, so the card does not move when it is empty.
  footer: string;
  busy: boolean;
};

export function TrendBody({ chart, figures, footer, busy }: Props) {
  return (
    <div className={styles.content} aria-busy={busy}>
      <div className={styles.chart}>{chart}</div>
      <div role="group" aria-label="Summary" className={styles.trio}>
        {figures.map((f) => (
          <div key={f.label}>
            <p className={styles.label}>{f.label}</p>
            <p className={styles.value}>{f.value ?? "–"}</p>
          </div>
        ))}
      </div>
      <p className={styles.footer}>{footer || "\u00a0"}</p>
    </div>
  );
}
