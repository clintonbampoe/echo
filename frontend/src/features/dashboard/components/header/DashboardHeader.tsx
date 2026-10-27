import { useCongregation, useMe } from "@/features/account";
import { daysAfter } from "@/lib/dates";
import { firstName, plural } from "@/lib/format";
import { greetingFor } from "../../model/greeting";
import { useProjectTotals } from "../../hooks/projects";
import { useVisitors } from "../../hooks/people";
import type { PeriodKey } from "../../model/periods";
import { PeriodPill } from "../controls/PeriodPill";
import styles from "./DashboardHeader.module.css";

type Props = {
  period: PeriodKey;
  today: string;
  onPeriod: (key: PeriodKey) => void;
};

// The greeting, a line on what needs a look (projects at risk, visitors of the last month) and the period pill.
export function DashboardHeader({ period, today, onPeriod }: Props) {
  const me = useMe().data;
  const church = useCongregation().data;
  const atRisk = useProjectTotals().totals?.atRisk;
  const visitors = useVisitors(daysAfter(today, -29), today).figures?.added;
  const greeting = `${greetingFor(new Date().getHours())}${me ? `, ${firstName(me.name)}` : ""}`;
  const notes = [
    atRisk ? `${plural(atRisk, "project")} at risk` : undefined,
    visitors ? `${plural(visitors, "new visitor")} this month` : undefined,
  ].filter(Boolean);
  return (
    <header className={styles.header}>
      <div>
        <h1 className={styles.title}>{greeting}</h1>
        <p className={styles.text}>
          {notes.length > 0 ? notes.join(" · ") : (church?.name ?? "")}
        </p>
      </div>
      <PeriodPill value={period} today={today} onChange={onPeriod} />
    </header>
  );
}
