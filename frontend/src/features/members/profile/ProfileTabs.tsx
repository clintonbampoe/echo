import type { ReactNode } from "react";
import { useSearchParams } from "react-router-dom";
import { countLabel } from "@/lib/paged";
import { Tabs } from "@/ui";
import { PersonAttendance, useAttendance } from "@/features/attendance";
import {
  OrganizedEvents,
  PersonEvents,
  usePersonRegistrations,
} from "@/features/events";
import {
  PersonOrganizations,
  usePersonMemberships,
} from "@/features/organizations";
import { ManagedProjects } from "@/features/projects";
import { MemberTithes, useMemberTithes } from "@/features/tithes";
import styles from "./Tab.module.css";

const badge = (l: { rows: unknown[]; hasMore: boolean; loading: boolean }) =>
  l.loading || l.rows.length === 0
    ? null
    : countLabel(l.rows.length, l.hasMore);

const ids = [
  "details",
  "attendance",
  "events",
  "tithes",
  "organizations",
  "responsibilities",
];

// The tab to show: the one asked for if it exists here, otherwise the first. Details is a tab only when
// the caller has it.
function pickTab(asked: string | null, hasDetails: boolean) {
  const first = hasDetails ? "details" : "attendance";
  if (asked === null || !ids.includes(asked)) return first;
  return asked === "details" && !hasDetails ? first : asked;
}

// The tabs under a member's numbers. Each tab is a piece owned by the feature it is about; this file only
// lays them out. The open tab is kept in the address (?tab=tithes), so the back button, a refresh and a
// shared link all land on the same tab. On a tablet or phone the caller passes the details, which become
// the first tab and the one that opens by default.
export function ProfileTabs({
  id,
  today,
  details,
}: {
  id: string;
  today: string;
  details?: ReactNode;
}) {
  const [params, setParams] = useSearchParams();
  const tab = pickTab(params.get("tab"), details !== undefined);

  // The same queries the tabs use, so the badges cost nothing extra.
  const tabs = [
    ...(details ? [{ id: "details", label: "Details" }] : []),
    { id: "attendance", label: "Attendance", count: badge(useAttendance(id)) },
    { id: "events", label: "Events", count: badge(usePersonRegistrations(id)) },
    { id: "tithes", label: "Tithes", count: badge(useMemberTithes(id)) },
    {
      id: "organizations",
      label: "Organizations",
      count: badge(usePersonMemberships(id)),
    },
    { id: "responsibilities", label: "Responsibilities" },
  ];

  return (
    <section aria-label="Member history">
      <Tabs
        label="Member history"
        tabs={tabs}
        value={tab}
        onChange={(t) => setParams({ tab: t }, { replace: true })}
        idPrefix="member"
      />
      <div
        role="tabpanel"
        id={`member-panel-${tab}`}
        aria-labelledby={`member-tab-${tab}`}
      >
        {tab === "details" && (
          <div className={styles.detailsTab}>{details}</div>
        )}
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
