import type { ReactNode } from "react";
import { Link } from "react-router-dom";
import { paths } from "@/lib/paths";
import { DateTile } from "@/ui";
import type { EventItem } from "../api";
import { eventSub, fillOf } from "../rows";
import styles from "./EventList.module.css";

type Props = { events: EventItem[]; past: boolean; footer?: ReactNode };

// The right of a row: what it measures, the share and the room, and a bar for how far along that is.
function Fill({ event: e, past }: { event: EventItem; past: boolean }) {
  const { label, text, share } = fillOf(e, past);
  return (
    <span className={styles.fill}>
      <span className={styles.nums}>
        <span className={styles.label}>{label}</span>
        <strong>{text}</strong>
      </span>
      {share !== null && (
        <span aria-hidden="true" className={styles.track}>
          <span className={styles.bar} style={{ width: `${share * 100}%` }} />
        </span>
      )}
    </span>
  );
}

// One row per event. The whole row opens the event; it is edited and deleted on its own page.
export function EventList({ events, past, footer }: Props) {
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
                <span className={styles.when}>{eventSub(e)}</span>
              </span>
              <Fill event={e} past={past} />
            </Link>
          </li>
        ))}
      </ul>
      {footer}
    </div>
  );
}
