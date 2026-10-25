import { formatNumber } from "@/lib/format";
import { paths } from "@/lib/paths";
import { Aside, DashCard, DistributionBar, ProgressBar } from "@/ui";
import type { AgeBand } from "../../api/members";
import { useMix } from "../../hooks/people";
import { useNewMembers } from "../../hooks/numbers";
import { statusRows } from "../../model/members";
import { spanOf } from "../../model/periods";
import { CardFailed } from "./CardFailed";
import styles from "./MembersCard.module.css";

const PART_TITLE = { gender: "Gender", age: "Age", status: "Status" } as const;
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

export type MembersPart = "gender" | "age" | "status";
const shows = (part: MembersPart | undefined, name: MembersPart) =>
  !part || part === name;

type Mix = NonNullable<ReturnType<typeof useMix>["mix"]>;

function Genders({ mix, total }: { mix: Mix | undefined; total: number }) {
  const side = (key: string, label: string, gender: string, n: number) => ({
    key,
    label,
    to: `${paths.members}?gender=${gender}`,
    value: n,
    text: formatNumber(n),
    share: percent(n, total),
  });
  return (
    <section aria-label="Gender">
      <p className={styles.heading}>Gender</p>
      <DistributionBar
        label="Women and men"
        empty="No members yet"
        parts={
          mix
            ? [
                side("f", "Women", "Female", mix.female),
                side("m", "Men", "Male", mix.male),
              ]
            : []
        }
      />
    </section>
  );
}

function Statuses({
  rows,
  total,
}: {
  rows: ReturnType<typeof statusRows>;
  total: number;
}) {
  return (
    <section aria-label="Status">
      <p className={styles.heading}>Status</p>
      <ul aria-label="Members by status" className={styles.status}>
        {rows.map((s) => (
          <li key={s.key}>
            <div className={styles.line}>
              <span>{s.name}</span>
              <strong>{formatNumber(s.count)}</strong>
              <span className={styles.share}>{percent(s.count, total)}</span>
            </div>
            <ProgressBar
              label={s.name}
              value={total > 0 ? s.count / total : 0}
            />
          </li>
        ))}
      </ul>
    </section>
  );
}

function MembersBody({
  mix,
  part,
  status,
}: {
  mix: Mix | undefined;
  part: MembersPart | undefined;
  status: ReturnType<typeof statusRows>;
}) {
  const total = mix?.total ?? 0;
  return (
    <div className={styles.grid} aria-busy={!mix}>
      {shows(part, "gender") && <Genders mix={mix} total={total} />}
      {shows(part, "age") && <Ages bands={mix?.bands ?? []} />}
      {shows(part, "status") && <Statuses rows={status} total={total} />}
    </div>
  );
}

// Everyone in the church at a glance: women and men, how old they are, and how many are active. Not the period.
// On a phone the card is three small ones, one for each part (`part`), so each has its own page.
export function MembersCard({
  today,
  part,
}: {
  today: string;
  part?: MembersPart;
}) {
  const { mix, failed, retry } = useMix();
  const fresh = useNewMembers(spanOf("this-year", today)).value;
  const total = mix?.total ?? 0;
  const status = statusRows(mix, fresh);

  return (
    <DashCard
      title={part ? PART_TITLE[part] : "Members"}
      control={<Aside>{mix ? `${formatNumber(total)} total` : ""}</Aside>}
    >
      {failed ? (
        <CardFailed onRetry={retry} />
      ) : (
        <MembersBody mix={mix} part={part} status={status} />
      )}
    </DashCard>
  );
}
