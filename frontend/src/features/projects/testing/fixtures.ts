import type { Contribution } from "../api/contributions";
import type { Project } from "../api/projects";

export const aProject = (over: Partial<Project> = {}): Project => ({
  id: "p1",
  name: "Church roof renovation",
  categoryId: 1,
  categoryName: "Building",
  managerId: "m1",
  managerName: "Ama Boateng",
  status: "AtRisk",
  targetAmount: 80000,
  raisedAmount: 48200,
  startDate: "2026-03-01",
  endDate: "2026-12-31",
  description: "Fix the roof.",
  createdAt: "2026-01-01T00:00:00Z",
  ...over,
});

export const aContribution = (
  over: Partial<Contribution> = {},
): Contribution => ({
  id: "c1",
  projectId: "p1",
  projectName: "Roof",
  amount: 4200,
  dateContributed: "2026-10-11T00:00:00",
  paymentMethod: "MobileMoney",
  description: "Sunday appeal",
  createdAt: "2026-10-11T00:00:00Z",
  ...over,
});
