// Where each kind of record lives. Lists link to these, so the address of a page is written once.
// A page that is not built yet is a placeholder in app/routes.tsx, so a link never ends on a 404.
export type MemberTab =
  "attendance" | "events" | "tithes" | "organizations" | "responsibilities";

export const paths = {
  members: "/members",
  visitors: "/visitors",
  organizations: "/organizations",
  tithes: "/tithes",
  finance: "/finance",
  financeCategories: "/finance/categories",
  events: "/events",
  eventsPast: "/events/past",
  attendance: "/attendance",
  // The Attendance page on one day.
  attendanceDay: (day: string) => `/attendance?day=${day}`,
  // The Attendance records over a period: the sessions held, or (`people`) every check-in.
  attendanceRecords: (view?: "people") =>
    view ? `/attendance/records/${view}` : "/attendance/records",
  sessionTypes: "/attendance/labels",
  // The Tithes page with one tithe open to read.
  tithe: (id: string) => `/tithes?tithe=${id}`,
  // A member's page, optionally on one of its tabs. A link from a record's list goes to the tab about that
  // record: a tithe to Tithes, an organization's member to Organizations, and so on.
  member: (id: string, tab?: MemberTab) =>
    `/members/${id}${tab ? `?tab=${tab}` : ""}`,
  visitor: (id: string) => `/visitors/${id}`,
  event: (id: string) => `/events/${id}`,
  organization: (id: string) => `/organizations/${id}`,
  session: (id: string) => `/attendance/sessions/${id}`,
  project: (id: string) => `/projects/${id}`,
};
