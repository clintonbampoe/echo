import { useCongregation, useMe } from "@/features/account";
import { todayText } from "@/lib/dates";
import { firstName } from "@/lib/format";
import { Pager } from "@/ui";
import { greetingFor } from "../greeting";
import { periodOf, spanOf, spokenSpan } from "../periods";
import { usePeriod } from "../usePeriod";
import { AssetsCard } from "./AssetsCard";
import { AttendanceCard } from "./AttendanceCard";
import { EventCalendarCard } from "./EventCalendarCard";
import { GenderCard } from "./GenderCard";
import { MemberGrowthCard } from "./MemberGrowthCard";
import { MoneyRow } from "./MoneyRow";
import { NumberCards } from "./NumberCards";
import { PeriodPill } from "./PeriodPill";
import { ProjectsCard } from "./ProjectsCard";
import { QuickActions } from "./QuickActions";
import { TithesCard } from "./TithesCard";
import { UpcomingEventsCard } from "./UpcomingEventsCard";
import { VisitorsJoinedCard } from "./VisitorsJoinedCard";
import styles from "./DashboardPage.module.css";

// The first page after signing in: a greeting and the period, then the cards and shortcuts.
export function DashboardPage() {
  const me = useMe().data;
  const church = useCongregation().data;
  const [period, setPeriod] = usePeriod();
  const today = todayText();
  const greeting = `${greetingFor(new Date().getHours())}${me ? `, ${firstName(me.name)}` : ""}`;
  const when = `${periodOf(period).label}, ${spokenSpan(spanOf(period, today))}.`;
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
        <PeriodPill value={period} today={today} onChange={setPeriod} />
      </header>
      <NumberCards period={period} today={today} />
      <QuickActions />
      <div className={styles.charts}>
        <MoneyRow today={today} />
        <AttendanceCard today={today} />
        <Pager
          label="Tithes and projects"
          titles={["Tithes", "Projects"]}
          columns="3fr 2fr"
        >
          <TithesCard today={today} />
          <ProjectsCard />
        </Pager>
        <AssetsCard />
        <Pager
          label="Members and visitors"
          titles={["Member growth", "Visitors who joined"]}
          columns="3fr 2fr"
        >
          <MemberGrowthCard today={today} />
          <VisitorsJoinedCard period={period} today={today} />
        </Pager>
        <Pager label="Members" titles={["Gender"]}>
          <GenderCard />
        </Pager>
        <Pager label="Events" titles={["Upcoming events", "Event calendar"]}>
          <UpcomingEventsCard today={today} />
          <EventCalendarCard today={today} />
        </Pager>
      </div>
    </div>
  );
}
