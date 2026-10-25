import { Link } from "react-router-dom";
import { formatDayTime } from "@/lib/format";
import { DashCard, DateTile, SeeLink } from "@/ui";
import type { DashEvent } from "../api";
import { useUpcomingEvents } from "../hooks";
import { CardFailed } from "./CardFailed";
import styles from "./UpcomingEventsCard.module.css";

// One event: its day in a tile, its name, and when and where.
function Row({ event: e }: { event: DashEvent }) {
  return (
    <li>
      <Link to={`/events/${e.id}`} className={styles.row}>
        <DateTile date={e.startDate} />
        <span className={styles.text}>
          <span className={styles.name}>{e.name}</span>
          <span className={styles.when}>
            {formatDayTime(e.startDate, e.startTime)}
          </span>
        </span>
      </Link>
    </li>
  );
}

// The next five events, soonest first.
export function UpcomingEventsCard({ today }: { today: string }) {
  const data = useUpcomingEvents(today);
  return (
    <DashCard
      title="Upcoming events"
      control={<SeeLink to="/events">View all</SeeLink>}
    >
      {data.failed ? (
        <CardFailed onRetry={data.retry} />
      ) : data.events?.length === 0 ? (
        <p className={styles.empty}>No upcoming events</p>
      ) : (
        <ul aria-busy={!data.events} className={styles.list}>
          {(data.events ?? []).map((e) => (
            <Row key={e.id} event={e} />
          ))}
        </ul>
      )}
    </DashCard>
  );
}
