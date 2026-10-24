import { useNavigate } from "react-router-dom";
import { paths } from "@/lib/paths";
import { LinkButton, PageHeader, Tabs } from "@/ui";
import styles from "./AttendanceHeader.module.css";

export type AttendanceTab = "day" | "records";

const tabs = [
  { id: "day", label: "Day" },
  { id: "records", label: "Records" },
];

// The top of both Attendance tabs: the title, the two buttons, and the tabs (Day for working on one day, Records
// for looking back). Each tab is its own address, so the page body of the open one has `id="attendance-panel-{tab}"`.
export function AttendanceHeader({
  tab,
  onNew,
}: {
  tab: AttendanceTab;
  onNew: () => void;
}) {
  const navigate = useNavigate();
  return (
    <>
      <PageHeader
        title="Attendance"
        action={
          <LinkButton to={paths.sessionTypes} variant="secondary">
            Session types
          </LinkButton>
        }
        add={{ label: "New session", onClick: onNew }}
      />
      <div className={styles.tabs}>
        <Tabs
          label="Attendance views"
          idPrefix="attendance"
          tabs={tabs}
          value={tab}
          onChange={(id) =>
            navigate(
              id === "records" ? paths.attendanceRecords() : paths.attendance,
            )
          }
        />
      </div>
    </>
  );
}
