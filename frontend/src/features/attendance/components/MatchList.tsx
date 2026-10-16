import { Avatar, Button, Icon } from "@/ui";
import { standing, type Match, type Standing } from "../matches";
import styles from "./MatchList.module.css";

type Props = {
  matches: Match[];
  now: ReadonlySet<string>;
  already: ReadonlySet<string>;
  busy: boolean;
  onMark: (match: Match) => void;
};

const SHOWN = 5;

// The people a search found, each with a button to mark them present. Someone already marked says so instead.
export function MatchList({ matches, now, already, busy, onMark }: Props) {
  const shown = matches.slice(0, SHOWN);
  return (
    <>
      <ul aria-label="Matching people" className={styles.list}>
        {shown.map((m) => (
          <li key={m.id} className={styles.row}>
            <Avatar name={m.name} size={32} />
            <span className={styles.who}>
              <span className={styles.name}>{m.name}</span>
              {m.phoneNumber && (
                <span className={styles.phone}>{m.phoneNumber}</span>
              )}
            </span>
            <Action
              standing={standing(m.id, now, already)}
              busy={busy}
              name={m.name}
              onMark={() => onMark(m)}
            />
          </li>
        ))}
      </ul>
      {matches.length > SHOWN && (
        <p className={styles.note}>
          Showing the top {SHOWN} matches. Keep typing to narrow it down.
        </p>
      )}
    </>
  );
}

function Action({
  standing: s,
  busy,
  name,
  onMark,
}: {
  standing: Standing;
  busy: boolean;
  name: string;
  onMark: () => void;
}) {
  if (s === "now")
    return (
      <span className={`${styles.state} ${styles.now}`}>
        <Icon name="check" size={14} /> Present
      </span>
    );
  if (s === "already")
    return (
      <span className={`${styles.state} ${styles.already}`}>
        Already present
      </span>
    );
  return (
    <Button
      size="sm"
      disabled={busy}
      aria-label={`Mark ${name} present`}
      onClick={onMark}
    >
      Mark present
    </Button>
  );
}
