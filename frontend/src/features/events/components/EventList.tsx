import type { ReactNode } from "react";
import { Link } from "react-router-dom";
import { paths } from "@/lib/paths";
import { ActionMenu, DateTile } from "@/ui";
import type { EventItem } from "../api";
import { countOf, fillOf } from "../rows";
import styles from "./EventList.module.css";

type Props = {
  events: EventItem[];
  past: boolean;
  footer?: ReactNode;
  onEdit: (event: EventItem) => void;
  onDelete: (event: EventItem) => void;
};

// The right of a row: the share and the room, and a bar for how far along that is. On a phone the bar goes and the
// room drops under the share.
function Fill({ event: e, past }: { event: EventItem; past: boolean }) {
  const { label, text, share } = fillOf(e, past);
  const [first = text, room] = text.split(" of ");
  return (
    <span className={styles.fill}>
      <span className={styles.nums}>
        <span className={styles.label}>{label}</span>
        <span className={styles.value}>
          <strong>{first}</strong>
          {room && <strong className={styles.room}> of {room}</strong>}
        </span>
      </span>
      {share !== null && (
        <span aria-hidden="true" className={styles.track}>
          <span className={styles.bar} style={{ width: `${share * 100}%` }} />
        </span>
      )}
    </span>
  );
}

// One row per event, all in one card. The row opens the event; the ⋯ menu edits or deletes it.
export function EventList({ events, past, footer, onEdit, onDelete }: Props) {
  return (
    <div className={styles.card}>
      <ul
        aria-label={past ? "Past events" : "Upcoming events"}
        className={styles.list}
      >
        {events.map((e) => (
          <li key={e.id} className={styles.item}>
            <Link to={paths.event(e.id)} className={styles.row}>
              <DateTile date={e.startDate} />
              <span className={styles.main}>
                <span className={styles.name}>{e.name}</span>
                <span className={styles.when}>{countOf(e, past)}</span>
              </span>
              <Fill event={e} past={past} />
            </Link>
            <span className={styles.menu}>
              <ActionMenu
                label={`Actions for ${e.name}`}
                title={e.name}
                items={[
                  { label: "Edit", onSelect: () => onEdit(e) },
                  {
                    label: "Delete",
                    danger: true,
                    onSelect: () => onDelete(e),
                  },
                ]}
              />
            </span>
          </li>
        ))}
      </ul>
      {footer}
    </div>
  );
}
