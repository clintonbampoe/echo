import { Link } from "react-router-dom";
import { formatNumber } from "@/lib/format";
import type { DashEvent } from "../api";
import { tileOf, whenOf } from "../events";
import { useUpcomingEvents } from "../hooks";
import { CardFailed } from "./CardFailed";
import { DashCard } from "./DashCard";
import styles from "./UpcomingEventsCard.module.css";

// One event: a link to it, with the day in a square, its name, and when. A wide card adds how many registered.
function Row({ event: e }: { event: DashEvent }) {
  const tile = tileOf(e.startDate);
  return (
    <li className={styles.row}>
      <Link to={`/events/${e.id}`} className={styles.link}>
        <span className={styles.tile} aria-hidden="true">
          <i>{tile.month}</i>
          <b>{tile.date}</b>
        </span>
        <span className={styles.text}>
          <strong className={styles.name}>{e.name}</strong>
          <span className={styles.when}>
            {whenOf(e)}
            <span className={styles.reg}>
              . {formatNumber(e.registrationCount)} registered
            </span>
          </span>
        </span>
      </Link>
    </li>
  );
}

function Rows({ events }: { events: DashEvent[] | undefined }) {
  if (!events)
    return (
      <ul aria-busy="true" className={styles.list}>
        {[0, 1, 2].map((i) => (
          <li key={i} className={styles.row}>
            <span className={styles.ghost} />
          </li>
        ))}
      </ul>
    );
  if (events.length === 0)
    return <p className={styles.empty}>No upcoming events</p>;
  return (
    <ul className={styles.list}>
      {events.map((e) => (
        <Row key={e.id} event={e} />
      ))}
    </ul>
  );
}

// The next events, soonest first. Five rows on a wide card, three on a narrow one.
export function UpcomingEventsCard({ today }: { today: string }) {
  const data = useUpcomingEvents(today);
  return (
    <DashCard taller title="Upcoming events" to="/events">
      {data.failed ? (
        <CardFailed onRetry={data.retry} />
      ) : (
        <Rows events={data.events} />
      )}
    </DashCard>
  );
}
