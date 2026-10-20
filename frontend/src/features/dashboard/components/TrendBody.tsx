import type { ReactNode } from "react";
import styles from "./TrendBody.module.css";

type Props = {
  chart: ReactNode;
  // A narrow card leads with one figure over the chart, the stretch it speaks about, and a line naming it.
  lead?: { value?: string; note?: string };
  // Three figures under the chart. A missing value shows as a dash.
  figures: readonly { label: string; value?: string }[];
  busy: boolean;
};

// A trend card's insides: the chart, and what it says about one stretch. A wide card puts the three
// figures in a strip under the chart; a narrow one leads with one of them above it.
export function TrendBody({ chart, lead, figures, busy }: Props) {
  return (
    <div className={styles.content} aria-busy={busy}>
      {lead && (
        <div className={styles.lead}>
          <p className={styles.big}>{lead.value ?? "–"}</p>
          <p className={styles.note}>{lead.note ?? "\u00a0"}</p>
        </div>
      )}
      <div className={styles.chart}>{chart}</div>
      <div role="group" aria-label="Summary" className={styles.trio}>
        {figures.map((f) => (
          <div key={f.label}>
            <p className={styles.label}>{f.label}</p>
            <p className={styles.value}>{f.value ?? "–"}</p>
          </div>
        ))}
      </div>
    </div>
  );
}
