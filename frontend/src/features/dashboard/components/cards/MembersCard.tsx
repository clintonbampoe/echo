import { formatNumber } from "@/lib/format";
import { Aside, DashCard, ProgressBar } from "@/ui";
import type { AgeBand } from "../../api/members";
import { useMix } from "../../hooks/people";
import { useNewMembers } from "../../hooks/numbers";
import { statusRows } from "../../model/members";
import { spanOf } from "../../model/periods";
import { CardFailed } from "./CardFailed";
import { Split } from "./Split";
import styles from "./MembersCard.module.css";

const percent = (n: number, of: number) =>
  of > 0 ? `${Math.round((n / of) * 100)}%` : "0%";

// Members by age: a column for each group, the biggest one dark.
function Ages({ bands }: { bands: readonly AgeBand[] }) {
  const tallest = Math.max(0, ...bands.map((b) => b.count));
  const peak = bands.find((b) => b.count === tallest && tallest > 0);
  return (
    <section aria-label="Age">
      <p className={styles.heading}>Age</p>
      <ul aria-label="Members by age" className={styles.ages}>
        {bands.map((b) => (
          <li key={b.label} className={styles.age}>
            <span
              className={`${styles.column} ${b === peak ? styles.peak : ""}`}
              style={{ height: `${Math.max(6, (b.count / tallest) * 100)}%` }}
              title={`${b.label}: ${formatNumber(b.count)}`}
            />
            <span className={styles.band}>{b.label}</span>
          </li>
        ))}
      </ul>
    </section>
  );
}

// Everyone in the church at a glance: women and men, how old they are, and how many are active. Not the period.
export function MembersCard({ today }: { today: string }) {
  const { mix, failed, retry } = useMix();
  const fresh = useNewMembers(spanOf("this-year", today)).value;
  const total = mix?.total ?? 0;
  const status = statusRows(mix, fresh);

  return (
    <DashCard
      title="Members"
      control={<Aside>{mix ? `${formatNumber(total)} total` : ""}</Aside>}
    >
      {failed ? (
        <CardFailed onRetry={retry} />
      ) : (
        <div className={styles.grid} aria-busy={!mix}>
          <section aria-label="Gender">
            <p className={styles.heading}>Gender</p>
            <Split
              label="Women and men"
              empty="No members yet"
              parts={
                mix
                  ? [
                      {
                        key: "f",
                        name: "Women",
                        value: mix.female,
                        text: formatNumber(mix.female),
                        share: percent(mix.female, total),
                      },
                      {
                        key: "m",
                        name: "Men",
                        value: mix.male,
                        text: formatNumber(mix.male),
                        share: percent(mix.male, total),
                      },
                    ]
                  : []
              }
            />
          </section>
          <Ages bands={mix?.bands ?? []} />
          <section aria-label="Status">
            <p className={styles.heading}>Status</p>
            <ul aria-label="Members by status" className={styles.status}>
              {status.map((s) => (
                <li key={s.key}>
                  <div className={styles.line}>
                    <span>{s.name}</span>
                    <strong>{formatNumber(s.count)}</strong>
                    <span className={styles.share}>
                      {percent(s.count, total)}
                    </span>
                  </div>
                  <ProgressBar
                    label={s.name}
                    value={total > 0 ? s.count / total : 0}
                  />
                </li>
              ))}
            </ul>
          </section>
        </div>
      )}
    </DashCard>
  );
}
