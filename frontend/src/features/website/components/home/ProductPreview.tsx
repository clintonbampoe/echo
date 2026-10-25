import styles from "./ProductPreview.module.css";

const SUNDAYS = [38, 52, 46, 60, 55, 68, 74, 82];
const DATES = [
  "Aug 30",
  "Sep 6",
  "Sep 13",
  "Sep 20",
  "Sep 27",
  "Oct 4",
  "Oct 11",
  "Oct 18",
];
const TILES = [
  ["Attendance", "342", "+8%"],
  ["Giving", "GHS 10,400", "+21%"],
  ["Net income", "GHS 13,000", "+38%"],
  ["Visitors", "12", ""],
] as const;
const VISITORS = [
  ["EQ", "Esther Quaye", "First visit · 21 Oct", "First time"],
  ["NO", "Nana Owusu", "Second visit", "Returning"],
  ["KA", "Kojo Antwi", "First visit · 21 Oct", "First time"],
] as const;

// A picture of the app, drawn here rather than screenshotted, so it always matches the
// current look and stays sharp in light and dark.
export function ProductPreview() {
  return (
    <div
      className={styles.window}
      role="img"
      aria-label="Echo dashboard for Grace Chapel: attendance, giving and new visitors"
    >
      <div className={styles.bar}>
        <span className={styles.lights} aria-hidden="true">
          <i />
          <i />
          <i />
        </span>
        <span className={styles.where}>Grace Chapel · Echo</span>
      </div>
      <div className={styles.body} aria-hidden="true">
        <aside className={styles.side}>
          {[
            "Dashboard",
            "Members",
            "Visitors",
            "Attendance",
            "Events",
            "Finance",
            "Tithes",
            "Projects",
          ].map((item, i) => (
            <span key={item} className={i === 0 ? styles.current : undefined}>
              {item}
            </span>
          ))}
        </aside>
        <div className={styles.main}>
          <p className={styles.greeting}>Good morning, Pastor Kwame</p>
          <p className={styles.sub}>12 first-time visitors this week.</p>
          <div className={styles.tiles}>
            {TILES.map(([label, value, change]) => (
              <div key={label} className={styles.tile}>
                <span className={styles.label}>{label}</span>
                <strong>{value}</strong>
                {change && <em>{change}</em>}
              </div>
            ))}
          </div>
          <div className={styles.panels}>
            <div className={styles.panel}>
              <span className={styles.label}>Attendance · Sundays</span>
              <div className={styles.bars}>
                {SUNDAYS.map((h, i) => (
                  <span key={DATES[i]}>
                    <b style={{ height: `${h}%` }} />
                    <small>{DATES[i]}</small>
                  </span>
                ))}
              </div>
            </div>
            <div className={styles.panel}>
              <span className={styles.label}>New visitors</span>
              {VISITORS.map(([initials, name, when, tag]) => (
                <div key={name} className={styles.person}>
                  <span className={styles.avatar}>{initials}</span>
                  <span className={styles.who}>
                    {name}
                    <small>{when}</small>
                  </span>
                  <span className={styles.tag}>{tag}</span>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
