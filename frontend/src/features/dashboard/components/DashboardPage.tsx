import type { ReactNode } from "react";
import { todayText } from "@/lib/dates";
import { Pager, useMedia } from "@/ui";
import { usePeriod } from "../usePeriod";
import { AssetsCard } from "./AssetsCard";
import { AttendanceCard } from "./AttendanceCard";
import { DashboardHeader } from "./DashboardHeader";
import { EventCalendarCard } from "./EventCalendarCard";
import { MemberGrowthCard } from "./MemberGrowthCard";
import { MembersCard } from "./MembersCard";
import { MoneyCard } from "./MoneyCard";
import { NumberCards } from "./NumberCards";
import { ProjectsCard } from "./ProjectsCard";
import { QuickActions } from "./QuickActions";
import { TithesCard } from "./TithesCard";
import { UpcomingEventsCard } from "./UpcomingEventsCard";
import { VisitorsCard } from "./VisitorsCard";
import styles from "./DashboardPage.module.css";

// A row of cards: side by side on a wide screen, pages to swipe through on a phone, where a label heads them.
type Group = { label: string; titles: string[]; cards: ReactNode[] };

const groups = (today: string): Group[] => [
  {
    label: "This week",
    titles: ["Attendance", "Visitors"],
    cards: [
      <AttendanceCard today={today} title="Attendance" />,
      <VisitorsCard today={today} />,
    ],
  },
  { label: "Money", titles: ["Money"], cards: [<MoneyCard today={today} />] },
  {
    label: "Members",
    titles: ["Tithes", "Member growth", "Members"],
    cards: [
      <TithesCard today={today} />,
      <MemberGrowthCard today={today} />,
      <MembersCard today={today} />,
    ],
  },
  {
    label: "Projects and assets",
    titles: ["Projects", "Assets"],
    cards: [<ProjectsCard />, <AssetsCard />],
  },
  {
    label: "Events",
    titles: ["Upcoming events", "Event calendar"],
    cards: [
      <UpcomingEventsCard today={today} />,
      <EventCalendarCard today={today} />,
    ],
  },
];

// The rows of a wide screen, in the order the board draws them.
type Row = { label: string; columns?: string; cards: ReactNode[] };

const rows = (today: string): Row[] => [
  {
    label: "Attendance and visitors",
    columns: "3fr 2fr",
    cards: [
      <AttendanceCard today={today} title="Attendance" />,
      <VisitorsCard today={today} />,
    ],
  },
  { label: "Money", columns: "1fr", cards: [<MoneyCard today={today} />] },
  {
    label: "Tithes and member growth",
    columns: "3fr 2fr",
    cards: [<TithesCard today={today} />, <MemberGrowthCard today={today} />],
  },
  { label: "Members", columns: "1fr", cards: [<MembersCard today={today} />] },
  {
    label: "Projects and assets",
    cards: [<ProjectsCard />, <AssetsCard />],
  },
  {
    label: "Events",
    columns: "3fr 2fr",
    cards: [
      <UpcomingEventsCard today={today} />,
      <EventCalendarCard today={today} />,
    ],
  },
];

export function DashboardPage() {
  const [period, setPeriod] = usePeriod();
  const today = todayText();
  // One tree or the other: on a phone the cards are re-dealt into themed groups. The queries are keyed the
  // same, so nothing is asked for twice.
  const narrow = useMedia("(max-width: 900px)");

  return (
    <div className="anim-rise">
      <DashboardHeader period={period} today={today} onPeriod={setPeriod} />
      <NumberCards period={period} today={today} />
      <QuickActions />
      <div className={styles.charts}>
        {narrow
          ? groups(today).map((g) => (
              <section key={g.label} className={styles.group}>
                <h2 className={styles.groupTitle}>{g.label}</h2>
                <Pager label={g.label} titles={g.titles}>
                  {g.cards.map((c, i) => (
                    <div key={g.titles[i]}>{c}</div>
                  ))}
                </Pager>
              </section>
            ))
          : rows(today).map((r) => (
              <Pager
                key={r.label}
                label={r.label}
                titles={r.cards.map((_, i) => `${r.label} ${i + 1}`)}
                columns={r.columns}
              >
                {r.cards.map((c, i) => (
                  <div key={i}>{c}</div>
                ))}
              </Pager>
            ))}
      </div>
    </div>
  );
}
