import { Link } from "react-router-dom";
import { formatNumber } from "@/lib/format";
import { paths } from "@/lib/paths";
import { Chip, Icon } from "@/ui";
import type { EventItem } from "../api";
import {
  capacityLabel,
  capacityShare,
  dateTile,
  eventWhere,
  registeredLabel,
} from "../rows";
import styles from "./EventList.module.css";

type Props = { events: EventItem[]; past: boolean };

function DateTile({ day }: { day: string }) {
  const { month, date } = dateTile(day);
  return (
    <span aria-hidden="true" className={styles.tile}>
      <span className={styles.month}>{month}</span>
      <span className={styles.date}>{date}</span>
    </span>
  );
}

// How many signed up, out of how many it takes. A past event also says how many came, when the list knows.
function Registered({ event: e, past }: { event: EventItem; past: boolean }) {
  const share = capacityShare(e.registrationCount, e.capacity);
  return (
    <span className={styles.fill}>
      <span className={styles.nums}>
        <span>{registeredLabel(e.registrationCount)}</span>
        <span className={styles.cap}>{capacityLabel(e.capacity)}</span>
      </span>
      {share !== null && (
        <span aria-hidden="true" className={styles.track}>
          <span className={styles.bar} style={{ width: `${share * 100}%` }} />
        </span>
      )}
      {past && e.attendanceCount !== undefined && (
        <span className={styles.sub}>
          {formatNumber(e.attendanceCount)} checked in
        </span>
      )}
    </span>
  );
}

// One row per event. The whole row opens the event; it is edited and deleted on its own page.
export function EventList({ events, past }: Props) {
  return (
    <ul
      aria-label={past ? "Past events" : "Upcoming events"}
      className={styles.list}
    >
      {events.map((e) => (
        <li key={e.id} className={styles.item}>
          <Link to={paths.event(e.id)} className={styles.row}>
            <DateTile day={e.startDate} />
            <span className={styles.main}>
              <span className={styles.top}>
                <span className={styles.name}>{e.name}</span>
                <Chip>{e.organizationName}</Chip>
              </span>
              <span className={styles.when}>{eventWhere(e)}</span>
            </span>
            <Registered event={e} past={past} />
            <Icon name="arrow-right" size={16} className={styles.go} />
          </Link>
        </li>
      ))}
    </ul>
  );
}
