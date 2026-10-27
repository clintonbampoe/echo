import type { CheckIn } from "../api/checkIns";
import type { SessionLabel } from "../api/sessionLabels";
import type { Session } from "../api/sessions";

export const aSessionLabel = (
  over: Partial<SessionLabel> = {},
): SessionLabel => ({
  id: 3,
  name: "Sunday Service",
  category: "Service",
  ...over,
});

export const aSession = (over: Partial<Session> = {}): Session => ({
  id: "s1",
  sessionLabelId: 3,
  sessionLabelName: "Sunday Service",
  category: "Service",
  date: "2026-10-11",
  name: null,
  topic: "Walking in Faith",
  startsAt: "08:00:00",
  endsAt: "10:30:00",
  status: "Completed",
  createdByName: "Ama Serwaa Boateng",
  createdAt: "",
  attendanceCount: 2,
  ...over,
});

export const aCheckIn = (over: Partial<CheckIn> = {}): CheckIn => ({
  id: "c1",
  sessionId: "s1",
  sessionDate: "2026-10-11",
  sessionLabelName: "Sunday Service",
  checkedInAt: "2026-10-11T09:04:00",
  personId: "m1",
  personName: "Kwame Asante",
  personKind: "Member",
  ...over,
});
