import type { RouteObject } from "react-router-dom";

const records = async () => ({
  Component: (await import("./components/RecordsPage")).RecordsPage,
});

// The pages load on demand, so the attendance code only downloads when someone opens Attendance.
export const attendanceRoutes: RouteObject[] = [
  {
    path: "attendance",
    lazy: async () => ({
      Component: (await import("./components/AttendancePage")).AttendancePage,
    }),
  },
  { path: "attendance/records", lazy: records },
  { path: "attendance/records/people", lazy: records },
  {
    path: "attendance/labels",
    lazy: async () => ({
      Component: (await import("./components/SessionTypesPage"))
        .SessionTypesPage,
    }),
  },
  {
    path: "attendance/sessions/:id",
    lazy: async () => ({
      Component: (await import("./components/SessionPage")).SessionPage,
    }),
  },
];
