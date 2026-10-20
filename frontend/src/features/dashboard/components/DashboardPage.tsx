import { useCongregation, useMe } from "@/features/account";
import type { CSSProperties, ReactNode } from "react";
import { todayText } from "@/lib/dates";
import { firstName } from "@/lib/format";
import { Pager, useMedia } from "@/ui";
import { greetingFor } from "../greeting";
import { periodOf, spanOf, spokenSpan, type PeriodKey } from "../periods";
import { usePeriod } from "../usePeriod";
import { AgeGroupsCard } from "./AgeGroupsCard";
import { AssetsCard } from "./AssetsCard";
import { AttendanceCard } from "./AttendanceCard";
import { EventCalendarCard } from "./EventCalendarCard";
import { GenderCard } from "./GenderCard";
import { MemberGrowthCard } from "./MemberGrowthCard";
import { MoneyRow } from "./MoneyRow";
import { NumberCards } from "./NumberCards";
import { PeriodPill } from "./PeriodPill";
import { ProjectsAtRiskCard } from "./ProjectsAtRiskCard";
import { ProjectsCard } from "./ProjectsCard";
import { QuickActions } from "./QuickActions";
import { RecentVisitorsCard } from "./RecentVisitorsCard";
import { TithesCard } from "./TithesCard";
import { UpcomingEventsCard } from "./UpcomingEventsCard";
import { VisitorsJoinedCard } from "./VisitorsJoinedCard";
import styles from "./DashboardPage.module.css";

// The cards of one theme, with the label that sits over them on a phone. The cards are the same on
// a wide screen, where the label makes way for the pairing of the rows.
type Group = { label: string; children: ReactNode };

const groups = (today: string, period: PeriodKey): readonly Group[] => [
  {
    label: "Money",
    children: <MoneyRow today={today} />,
  },
  {
    label: "This week",
    children: (
      <>
        <AttendanceCard today={today} />
        <TithesCard today={today} />
      </>
    ),
  },
  {
    label: "Projects and assets",
    children: (
      <>
        <Pager label="Projects" titles={["Projects", "Projects at risk"]}>
          <ProjectsCard />
          <ProjectsAtRiskCard />
        </Pager>
        <AssetsCard split />
      </>
    ),
  },
  {
    label: "Events",
    children: (
      <Pager
        label="Events"
        titles={["Upcoming events", "Event calendar"]}
      >
        <UpcomingEventsCard today={today} />
        <EventCalendarCard today={today} />
      </Pager>
    ),
  },
  {
    label: "Members",
    children: (
      <Pager
        label="Members"
        titles={["Member growth", "Gender", "Age groups"]}
      >
        <MemberGrowthCard today={today} />
        <GenderCard />
        <AgeGroupsCard />
      </Pager>
    ),
  },
  {
    label: "Visitors",
    children: (
      <Pager
        label="Visitors"
        titles={["Recent visitors", "Visitors who joined"]}
      >
        <RecentVisitorsCard today={today} />
        <VisitorsJoinedCard period={period} today={today} />
      </Pager>
    ),
  },
];

// The rows of a wide screen, each its own answer to what the cards are about, drawn at the height the
// boards give that row: the tithes row is the tallest, the visitors row the shortest.
type Row = { height: number; node: ReactNode };

const rows = (today: string, period: PeriodKey): readonly Row[] => [
  { height: 460, node: <MoneyRow today={today} /> },
  { height: 316, node: <AttendanceCard today={today} /> },
  {
    height: 534,
    node: (
      <Pager
        label="Tithes and projects"
        titles={["Tithes", "Projects"]}
        columns="3fr 2fr"
      >
        <TithesCard today={today} />
        <ProjectsCard stacked />
      </Pager>
    ),
  },
  { height: 368, node: <AssetsCard /> },
  {
    height: 418,
    node: (
      <Pager
        label="Members and visitors"
        titles={["Member growth", "Visitors who joined"]}
        columns="3fr 2fr"
      >
        <MemberGrowthCard today={today} />
        <VisitorsJoinedCard period={period} today={today} />
      </Pager>
    ),
  },
  {
    height: 343,
    node: (
      <Pager label="Members" titles={["Gender", "Age groups"]}>
        <GenderCard />
        <AgeGroupsCard />
      </Pager>
    ),
  },
  {
    height: 461,
    node: (
      <Pager
        label="Events"
        titles={["Upcoming events", "Event calendar"]}
        columns="3fr 2fr"
      >
        <UpcomingEventsCard today={today} />
        <EventCalendarCard today={today} />
      </Pager>
    ),
  },
  {
    height: 265,
    node: (
      <Pager
        label="Recent visitors and projects at risk"
        titles={["Recent visitors", "Projects at risk"]}
      >
        <RecentVisitorsCard today={today} />
        <ProjectsAtRiskCard />
      </Pager>
    ),
  },
];

export function DashboardPage() {
  const me = useMe().data;
  const church = useCongregation().data;
  const [period, setPeriod] = usePeriod();
  const today = todayText();
  const greeting = `${greetingFor(new Date().getHours())}${me ? `, ${firstName(me.name)}` : ""}`;
  const when = `${periodOf(period).label}, ${spokenSpan(spanOf(period, today))}.`;
  // One tree or the other: on a phone the cards are re-dealt into the themed groups. The queries are
  // keyed the same, so nothing is asked for twice, and a switch at the boundary re-deals it once.
  const narrow = useMedia("(max-width: 900px)");

  return (
    <div className="anim-rise">
      <header className={styles.header}>
        <div>
          <h1 className={styles.title}>{greeting}</h1>
          <p className={styles.text}>
            <span className={styles.when}>
              {when}
              {church ? ` Here is how ${church.name} is doing.` : ""}
            </span>
            {church && <span className={styles.church}>{church.name}</span>}
          </p>
        </div>
        <div className={styles.period}>
          <PeriodPill value={period} today={today} onChange={setPeriod} />
        </div>
      </header>
      <NumberCards period={period} today={today} />
      <QuickActions />

      <div className={styles.charts}>
        {narrow
          ? groups(today, period).map((g) => (
              <section key={g.label} className={styles.group}>
                <h2 className={styles.groupTitle}>{g.label}</h2>
                {g.children}
              </section>
            ))
          : rows(today, period).map((r, i) => (
              <div
                key={i}
                style={{ "--dash-h": `${r.height}px` } as CSSProperties}
              >
                {r.node}
              </div>
            ))}
      </div>
    </div>
  );
}
