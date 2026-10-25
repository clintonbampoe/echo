import type { EventItem } from "../api/events";

export const anEvent = (over: Partial<EventItem> = {}): EventItem => ({
  id: "e1",
  name: "Youth Rally",
  organizationId: "o1",
  organizationName: "Youth Ministry",
  organizerId: "m1",
  organizerName: "Ama Mensah",
  startDate: "2099-10-17",
  endDate: "2099-10-17",
  startTime: "16:00:00",
  location: "Main Auditorium",
  capacity: 150,
  createdAt: "2099-01-01T00:00:00Z",
  registrationCount: 84,
  ...over,
});
