import { useCongregation, useMe } from "@/features/account";
import { useMedia } from "@/ui";
import { daysAfter } from "@/lib/dates";
import { useProjectTotals } from "../../hooks/projects";
import { useVisitors } from "../../hooks/people";
import type { PeriodKey } from "../../model/periods";
import { headline } from "../../model/headline";
import { PeriodPill } from "../controls/PeriodPill";
import styles from "./DashboardHeader.module.css";

type Props = {
  period: PeriodKey;
  today: string;
  onPeriod: (key: PeriodKey) => void;
  // A church that has recorded nothing yet is welcomed, and told how much is left to set up.
  welcome?: { left: number };
};

// The greeting, a line on what needs a look (projects at risk, visitors of the last month) and the period pill.
export function DashboardHeader({ period, today, onPeriod, welcome }: Props) {
  const me = useMe().data;
  const church = useCongregation().data;
  const atRisk = useProjectTotals().totals?.atRisk;
  const visitors = useVisitors(daysAfter(today, -29), today).figures?.added;
  const phone = useMedia("(max-width: 700px)");
  const { title, line } = headline({
    welcomeLeft: welcome?.left,
    phone,
    hour: new Date().getHours(),
    fullName: me?.name,
    church: church?.name,
    atRisk,
    newVisitors: visitors,
  });
  return (
    <header className={styles.header}>
      <div>
        <h1 className={styles.title} data-welcome={welcome ? "" : undefined}>
          {title}
        </h1>
        <p className={styles.text} data-welcome={welcome ? "" : undefined}>
          {line}
        </p>
      </div>
      {!welcome && (
        <PeriodPill value={period} today={today} onChange={onPeriod} />
      )}
    </header>
  );
}
