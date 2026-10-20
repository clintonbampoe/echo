import { formatNumber } from "@/lib/format";
import { useVisitors } from "../hooks";
import { joinedLabel, visitorRows } from "../people";
import { periodOf, spanOf, type PeriodKey } from "../periods";
import { BarRows } from "./BarRows";
import { CardFailed } from "./CardFailed";
import { DashCard } from "./DashCard";
import styles from "./VisitorsJoinedCard.module.css";

type Props = { period: PeriodKey; today: string };

const text = (n?: number) => (n === undefined ? "–" : formatNumber(n));

// Visitors added in the period, and how many of the visitors who became members did so in it.
export function VisitorsJoinedCard({ period, today }: Props) {
  const span = spanOf(period, today);
  const data = useVisitors(span.from, span.to);
  const rows = visitorRows(data.figures);

  return (
    <DashCard
      tall
      title="Visitors who joined"
      hint="Follows the period"
      to="/visitors"
    >
      {data.failed ? (
        <CardFailed onRetry={data.retry} />
      ) : (
        <div className={styles.content} aria-busy={!data.figures}>
          <p className={styles.big}>{text(data.figures?.joined)}</p>
          <p className={styles.note}>
            {joinedLabel(data.figures?.joined)},{" "}
            {periodOf(period).label.toLowerCase()}
          </p>
          <BarRows
            label="Visitors in the period"
            rows={rows.map((r) => ({
              key: r.key,
              name: r.name,
              share: r.share,
              text: text(r.value),
            }))}
          />
          <p className={styles.footer}>Counted when marked as a member</p>
        </div>
      )}
    </DashCard>
  );
}
