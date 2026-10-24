import { Link } from "react-router-dom";
import { Avatar, Icon } from "@/ui";
import type { RecentVisitor } from "../api";
import { useRecentVisitors } from "../hooks";
import { addedAgo } from "../people";
import { CardFailed } from "./CardFailed";
import { DashCard, SeeLink } from "./DashCard";
import styles from "./RecentVisitorsCard.module.css";

// How many rows the card keeps: the three newest, whatever the period says.
const ROWS = 3;

// One visitor: a link to their record, with the monogram, their name and when they were added.
// On a wide card the whole record is offered by a button; on a narrow one a chevron leads.
function Row({ visitor: v, today }: { visitor: RecentVisitor; today: string }) {
  return (
    <li className={styles.row}>
      <Link to={`/visitors/${v.id}`} className={styles.link}>
        <Avatar name={v.name} />
        <span className={styles.text}>
          <strong className={styles.name}>{v.name}</strong>
          <span className={styles.when}>{addedAgo(v, today)}</span>
        </span>
        <span className={styles.open}>
          <span className={styles.word}>View</span>
          <Icon name="chevron-right" size={14} className={styles.go} />
        </span>
      </Link>
    </li>
  );
}

function Rows({
  visitors,
  today,
}: {
  visitors?: RecentVisitor[];
  today: string;
}) {
  if (!visitors)
    return (
      <ul aria-busy="true" className={styles.list}>
        {[0, 1, 2].map((i) => (
          <li key={i} className={styles.row}>
            <span className={styles.ghost} />
          </li>
        ))}
      </ul>
    );
  if (visitors.length === 0)
    return <p className={styles.empty}>No visitors in the last 6 months</p>;
  return (
    <ul className={styles.list}>
      {visitors.slice(0, ROWS).map((v) => (
        <Row key={v.id} visitor={v} today={today} />
      ))}
    </ul>
  );
}

// The visitors added last, however long ago. Three of them, newest first. Not the period.
export function RecentVisitorsCard({ today }: { today: string }) {
  const data = useRecentVisitors(today);
  const visitors = data.visitors
    ? [...data.visitors].sort((a, b) => b.added.localeCompare(a.added))
    : undefined;
  return (
    <DashCard
      title="Recent visitors"
      to="/visitors"
      control={<SeeLink to="/visitors">All visitors</SeeLink>}
    >
      {data.failed ? (
        <CardFailed onRetry={data.retry} />
      ) : (
        <Rows visitors={visitors} today={today} />
      )}
    </DashCard>
  );
}
