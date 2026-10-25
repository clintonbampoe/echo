import { api } from "@/lib/api";
import { ALL_TIME } from "@/lib/dates";

// A project and what it has raised. All time, and the first 50 projects the server sends: a church with more
// than that would see only those.
export type DashProject = {
  id: string;
  name: string;
  raised: number;
  target: number;
  atRisk: boolean;
};

export const getProjects = (signal?: AbortSignal) =>
  api
    .get<{
      data: {
        id: string;
        name: string;
        raisedAmount: number;
        targetAmount: number;
        status: string;
      }[];
    }>("/v1/projects", {
      query: { from: ALL_TIME.from, to: ALL_TIME.to, pageSize: 50 },
      signal,
    })
    .then((r) =>
      r.data.map((p): DashProject => ({
        id: p.id,
        name: p.name,
        raised: p.raisedAmount,
        target: p.targetAmount,
        atRisk: p.status === "AtRisk",
      })),
    );

// What the projects-at-risk card says beside its rows: of every project, how many are At Risk. All time.
export type ProjectTotals = { total: number; atRisk: number };

export const getProjectTotals = (signal?: AbortSignal) =>
  api
    .get<{ totalProjects: number; atRiskCount: number }>(
      "/v1/projects/summary",
      { signal },
    )
    .then((r): ProjectTotals => ({
      total: r.totalProjects,
      atRisk: r.atRiskCount,
    }));
