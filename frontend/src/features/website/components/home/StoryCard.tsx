import styles from "./StoryCard.module.css";

const MONTHS = ["Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct"];
const INCOME = [48, 62, 55, 70, 64, 78, 72, 86];
const EXPENSES = [30, 36, 28, 40, 34, 38, 36, 42];

// The small picture beside each story on the home page.
export function StoryCard({ number }: { number: string }) {
  if (number === "01") return <Attendance />;
  if (number === "02") return <Money />;
  return <Announcement />;
}

function Attendance() {
  const rows = [
    ["AM", "Abena Mensah", "Accra", ""],
    ["KO", "Kofi Owusu", "Kumasi", ""],
    ["EQ", "Esther Quaye", "First visit", "First time"],
    ["DA", "Daniel Asante", "Tema", ""],
  ];
  return (
    <div
      className={styles.card}
      role="img"
      aria-label="Taking attendance: 342 present, 317 members and 25 visitors"
    >
      <p className={styles.small}>Sunday Service · 9:00 am</p>
      <p className={styles.big}>
        342 <span>present</span>
      </p>
      <p className={styles.small}>317 members · 25 visitors</p>
      <div className={styles.list} aria-hidden="true">
        {rows.map(([initials, name, place, tag]) => (
          <div key={name} className={styles.line}>
            <span className={styles.avatar}>{initials}</span>
            <span className={styles.grow}>
              {name}
              <small>{place}</small>
            </span>
            {tag && <span className={styles.tag}>{tag}</span>}
          </div>
        ))}
      </div>
    </div>
  );
}

function Money() {
  return (
    <div
      className={styles.card}
      role="img"
      aria-label="October net income of GHS 13,000: income 34,000, expenses 21,000"
    >
      <p className={styles.small}>October · net</p>
      <p className={styles.big}>
        +13,000 <span>GHS</span>
      </p>
      <p className={styles.small}>Income 34,000 · Expenses 21,000</p>
      <div className={styles.chart} aria-hidden="true">
        {MONTHS.map((month, i) => (
          <span key={month}>
            <b style={{ height: `${INCOME[i]}%` }} />
            <i style={{ height: `${EXPENSES[i]}%` }} />
            <small>{month}</small>
          </span>
        ))}
      </div>
    </div>
  );
}

function Announcement() {
  return (
    <div className={styles.card}>
      <p className={styles.small}>Message for your announcement</p>
      <p className={styles.message}>
        This week we received <strong>GHS 12,400</strong>. GHS 9,800 came from
        tithes and GHS 2,600 from offerings. We spent GHS 8,700, leaving GHS
        3,700 for the work ahead. Thank you for giving.
      </p>
      <div className={styles.facts} aria-hidden="true">
        <span>
          Came in <strong>GHS 12,400</strong>
        </span>
        <span>
          Went out <strong>GHS 8,700</strong>
        </span>
        <span>
          Left over <strong>GHS 3,700</strong>
        </span>
      </div>
    </div>
  );
}
