import { formatNumber } from "@/lib/format";
import { useMix } from "../hooks";
import { ageRows, averageAge } from "../people";
import { BarRows } from "./BarRows";
import { CardFailed } from "./CardFailed";
import { DashCard } from "./DashCard";
import styles from "./AgeGroupsCard.module.css";

// How old the members are, in five bands. All of them, whatever their status, so it does not change
// with the period. On a narrow card the average leads before the bars; on a wide one it goes in the foot.
export function AgeGroupsCard() {
  const data = useMix();
  const rows = ageRows(data.mix);
  const total = rows.reduce((n, r) => n + r.count, 0);
  return (
    <DashCard
      title="Age groups"
      hint="All members, not the period"
      to="/members"
      foot={{
        note: averageAge(data.mix)
          ? `Average age ${averageAge(data.mix)}`
          : undefined,
        to: "/members",
        label: "See members",
      }}
    >
      {data.failed ? (
        <CardFailed onRetry={data.retry} />
      ) : (
        <div className={styles.content} aria-busy={!data.mix}>
          {averageAge(data.mix) && (
            <div className={styles.figure}>
              <p className={styles.big}>{averageAge(data.mix)}</p>
              <p className={styles.note}>Average age</p>
            </div>
          )}
          {!data.mix ? (
            <div className={styles.ghosts}>
              {[0, 1, 2, 3, 4].map((i) => (
                <span key={i} className={styles.ghost} />
              ))}
            </div>
          ) : total === 0 ? (
            <p className={styles.empty}>No members yet</p>
          ) : (
            <BarRows
              label="Members by age group"
              shares
              rows={rows.map((r) => ({
                key: r.key,
                name: r.name,
                share: total > 0 ? r.count / total : 0,
                text: formatNumber(r.count),
                sub: `${Math.round((r.count / total) * 100)}%`,
              }))}
            />
          )}
        </div>
      )}
    </DashCard>
  );
}
