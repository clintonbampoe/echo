// A visitor for tests, with only what the API always sends. Override what the test is about.
import type { Visitor } from "../api/visitors";

export const aVisitor = (over: Partial<Visitor> = {}): Visitor => ({
  id: "v1",
  name: "Esi Mensah",
  firstName: "Esi",
  lastName: "Mensah",
  createdAt: "2026-09-07T10:00:00Z",
  visitCount: 1,
  ...over,
});
