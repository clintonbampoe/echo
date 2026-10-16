import { Icon } from "@/ui";
import type { PersonKind, Session } from "../api";
import { useCheckIns, useSessionBefore } from "../hooks";
import type { Match } from "../matches";
import styles from "./CameLastTime.module.css";

type Props = {
  session: Session;
  kind: PersonKind;
  present: ReadonlySet<string>;
  busy: boolean;
  onMark: (match: Match) => void;
};

const SHOWN = 8;

// The quickest way to take a roll: the people who came to the last session of this type and are not here yet.
export function CameLastTime({ session, ...rest }: Props) {
  const before = useSessionBefore(session.sessionLabelId, session.date);
  return before.data ? <Names previous={before.data.id} {...rest} /> : null;
}

function Names({
  previous,
  kind,
  present,
  busy,
  onMark,
}: Omit<Props, "session"> & { previous: string }) {
  const last = useCheckIns(previous, { kind });
  const people = last.rows
    .filter((c) => !present.has(c.personId))
    .slice(0, SHOWN);
  if (people.length === 0) return null;
  return (
    <section aria-label="Came last time" className={styles.block}>
      <h3 className={styles.title}>Came last time</h3>
      <ul className={styles.chips}>
        {people.map((c) => (
          <li key={c.personId}>
            <button
              type="button"
              className={styles.chip}
              disabled={busy}
              aria-label={`Mark ${c.personName} present`}
              onClick={() => onMark({ id: c.personId, name: c.personName })}
            >
              {c.personName}
              <Icon name="plus" size={12} />
            </button>
          </li>
        ))}
      </ul>
    </section>
  );
}
