import { formatDayMonth } from "@/lib/format";
import { todayText } from "@/lib/dates";
import { Avatar } from "@/ui";
import type { PersonKind } from "../../api/checkIns";
import type { Session } from "../../api/sessions";
import { useCheckIns, useSessionBefore } from "../../hooks/queries";
import type { Match } from "../../model/matches";
import styles from "./CameLastTime.module.css";

type Props = {
  session: Session;
  kind: PersonKind;
  present: ReadonlySet<string>;
  busy: boolean;
  onMark: (match: Match) => void;
  // Marks everyone listed, for a roll that is the same as last time.
  onMarkAll: (matches: Match[]) => void;
};

const SHOWN = 8;

// The quickest way to take a roll: the people who came to the last session of this type and are not here yet.
export function CameLastTime({ session, ...rest }: Props) {
  const before = useSessionBefore(session.sessionLabelId, session.date);
  return before.data ? (
    <Names
      today={session.date === todayText()}
      previous={before.data}
      {...rest}
    />
  ) : null;
}

type NamesProps = Omit<Props, "session"> & {
  previous: Session;
  today: boolean;
};

function Names({
  previous,
  today,
  kind,
  present,
  busy,
  onMark,
  onMarkAll,
}: NamesProps) {
  const last = useCheckIns(previous.id, { kind });
  const people = last.rows
    .filter((c) => !present.has(c.personId))
    .slice(0, SHOWN);
  if (people.length === 0) return null;
  const matches = people.map((c) => ({ id: c.personId, name: c.personName }));
  return (
    <section aria-label="Came last time" className={styles.block}>
      <header className={styles.head}>
        <div>
          <h3 className={styles.title}>Came last time</h3>
          <p className={styles.help}>
            At {previous.sessionLabelName} on {formatDayMonth(previous.date)}{" "}
            and not marked yet{today ? " today" : ""}.
          </p>
        </div>
        <button
          type="button"
          className={styles.all}
          disabled={busy}
          onClick={() => onMarkAll(matches)}
        >
          Mark all {people.length}
        </button>
      </header>
      <ul className={styles.list}>
        {people.map((c, i) => (
          <li key={c.personId} className={`${styles.row} liquid-row`}>
            <Avatar name={c.personName} size={36} />
            <span className={styles.name}>{c.personName}</span>
            <button
              type="button"
              className={styles.mark}
              disabled={busy}
              aria-label={`Mark ${c.personName} present`}
              style={{ "--i": i } as React.CSSProperties}
              onClick={() => onMark({ id: c.personId, name: c.personName })}
            >
              Mark
            </button>
          </li>
        ))}
      </ul>
    </section>
  );
}
