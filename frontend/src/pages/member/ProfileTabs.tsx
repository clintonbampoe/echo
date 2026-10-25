import type { ReactNode } from "react";
import { useSearchParams } from "react-router-dom";
import { Tabs } from "@/ui";
import { PersonAttendance } from "@/features/attendance";
import { OrganizedEvents, PersonEvents } from "@/features/events";
import { PersonOrganizations } from "@/features/organizations";
import { ManagedProjects } from "@/features/projects";
import { MemberTithes } from "@/features/tithes";
import styles from "./ProfileTabs.module.css";

const ids = [
  "details",
  "attendance",
  "events",
  "tithes",
  "organizations",
  "responsibilities",
];

// The tab to show: the one asked for if it exists, otherwise Details.
const pickTab = (asked: string | null) =>
  asked !== null && ids.includes(asked) ? asked : "details";

// The tabs under a member's numbers. Each tab is a piece owned by the feature it is about; this file only
// lays them out. The open tab is kept in the address (?tab=tithes), so the back button, a refresh and a
// shared link all land on the same tab. Details is first and opens by default.
export function ProfileTabs({
  id,
  today,
  details,
}: {
  id: string;
  today: string;
  details: ReactNode;
}) {
  const [params, setParams] = useSearchParams();
  const tab = pickTab(params.get("tab"));

  const tabs = [
    { id: "details", label: "Details" },
    { id: "attendance", label: "Attendance" },
    { id: "events", label: "Events" },
    { id: "tithes", label: "Tithes" },
    { id: "organizations", label: "Organizations" },
    { id: "responsibilities", label: "Responsibilities" },
  ];

  return (
    <section aria-label="Member history" className={styles.tabs}>
      <Tabs
        label="Member history"
        tabs={tabs}
        value={tab}
        onChange={(t) => setParams({ tab: t }, { replace: true })}
        idPrefix="member"
      />
      <div
        key={tab}
        className={`${styles.panel} anim-rise`}
        role="tabpanel"
        id={`member-panel-${tab}`}
        aria-labelledby={`member-tab-${tab}`}
      >
        {tab === "details" && details}
        {tab === "attendance" && <PersonAttendance personId={id} />}
        {tab === "events" && <PersonEvents personId={id} />}
        {tab === "tithes" && <MemberTithes memberId={id} today={today} />}
        {tab === "organizations" && <PersonOrganizations personId={id} />}
        {tab === "responsibilities" && (
          <>
            <ManagedProjects personId={id} />
            <OrganizedEvents personId={id} />
          </>
        )}
      </div>
    </section>
  );
}
