// The summary calls the dashboard makes. No React in this file. Each one answers with what a card needs,
// so a card never depends on the rest of a feature's types.
import { api } from "@/lib/api";
import { ALL_TIME } from "@/lib/dates";

const get = <T>(path: string, from: string, to: string, signal?: AbortSignal) =>
  api.get<T>(path, { query: { from, to }, signal });

// Members who joined in the range. From 2000 to today it is everyone. Members with no joined date are in
// every range, which is why the dashboard takes them off (see useNewMembers).
export const getMembers = (from: string, to: string, signal?: AbortSignal) =>
  get<{ totalMembers: number }>("/v1/members/summary", from, to, signal).then(
    (r) => r.totalMembers,
  );

// Check-ins in the range.
export const getPresent = (from: string, to: string, signal?: AbortSignal) =>
  get<{ totalPresent: number }>(
    "/v1/attendance/summary",
    from,
    to,
    signal,
  ).then((r) => r.totalPresent);

export const getTithes = (from: string, to: string, signal?: AbortSignal) =>
  get<{ totalCollected: number }>("/v1/tithes/summary", from, to, signal).then(
    (r) => r.totalCollected,
  );

// What the Tithes card shows for one stretch.
export type TitheFigures = { total: number; tithers: number; average: number };

export const getTitheFigures = (
  from: string,
  to: string,
  signal?: AbortSignal,
) =>
  get<{
    totalCollected: number;
    uniqueTithers: number;
    averagePerMember: number;
  }>("/v1/tithes/summary", from, to, signal).then((r): TitheFigures => ({
    total: r.totalCollected,
    tithers: r.uniqueTithers,
    average: r.averagePerMember,
  }));

// Income minus expenses. Tithes and project gifts are not in it.
export const getNet = (from: string, to: string, signal?: AbortSignal) =>
  get<{ net: number }>("/v1/transactions/summary", from, to, signal).then(
    (r) => r.net,
  );

// One row per calendar month in the range, oldest first, with zeros for a month with nothing in it.
export type MoneyMonth = {
  year: number;
  month: number;
  totalIncome: number;
  totalExpenses: number;
};

export const getMonths = (from: string, to: string, signal?: AbortSignal) =>
  get<MoneyMonth[]>("/v1/transactions/summary/monthly", from, to, signal);

// What one category added up to over the range.
export type CategoryTotal = {
  categoryId: number;
  categoryName: string;
  type: "Income" | "Expense";
  total: number;
};

export const getCategories = (from: string, to: string, signal?: AbortSignal) =>
  get<{ byCategory: CategoryTotal[] }>(
    "/v1/transactions/summary",
    from,
    to,
    signal,
  ).then((r) => r.byCategory);

// An event as a dashboard card needs it. Days are plain "2026-10-24"; endDate is the last day, the same as
// startDate for a single day.
export type DashEvent = {
  id: string;
  name: string;
  startDate: string;
  endDate: string;
  startTime?: string | null;
  registrationCount: number;
};

// Events that start in the range (by the day they start), in date order. The server answers at most 50.
export const getEvents = (
  from: string,
  to: string,
  order: "Asc" | "Desc",
  pageSize: number,
  signal?: AbortSignal,
) =>
  api
    .get<{ data: DashEvent[] }>("/v1/events", {
      query: { from, to, order, pageSize },
      signal,
    })
    .then((r) =>
      r.data.map((e) => ({
        ...e,
        startDate: e.startDate.slice(0, 10),
        endDate: e.endDate.slice(0, 10),
      })),
    );

// A project and what it has raised. All time, and the first 50 projects the server sends: a church with more
// than that would see only those.
export type DashProject = { id: string; name: string; raised: number };

export const getProjects = (signal?: AbortSignal) =>
  api
    .get<{ data: { id: string; name: string; raisedAmount: number }[] }>(
      "/v1/projects",
      {
        query: { from: ALL_TIME.from, to: ALL_TIME.to, pageSize: 50 },
        signal,
      },
    )
    .then((r) =>
      r.data.map((p): DashProject => ({
        id: p.id,
        name: p.name,
        raised: p.raisedAmount,
      })),
    );

// What assets are grouped under, with how many each holds.
export type AssetCategory = { id: number; name: string; assetCount: number };

export const getAssetCategories = (signal?: AbortSignal) =>
  api.get<AssetCategory[]>("/v1/asset-categories", { signal });

// The assets' numbers, for all of them or one category. Liquidated assets are not counted. `lost` is cost
// minus value, below zero when they are worth more than they cost.
export type AssetFigures = {
  count: number;
  value: number;
  cost: number;
  lost: number;
};

export const getAssetFigures = (
  categoryId: number | undefined,
  signal?: AbortSignal,
) =>
  api
    .get<{
      totalAssets: number;
      totalCurrentValue: number;
      totalPurchaseCost: number;
      totalDepreciation: number;
    }>("/v1/assets/summary", {
      query: { from: ALL_TIME.from, to: ALL_TIME.to, categoryId },
      signal,
    })
    .then((r): AssetFigures => ({
      count: r.totalAssets,
      value: r.totalCurrentValue,
      cost: r.totalPurchaseCost,
      lost: r.totalDepreciation,
    }));

// Visitors added in the range, and how many visitors became members in it.
export type VisitorFigures = { added: number; joined: number };

export const getVisitorFigures = (
  from: string,
  to: string,
  signal?: AbortSignal,
) =>
  get<{ newVisitors: number; convertedVisitors: number }>(
    "/v1/visitors/summary",
    from,
    to,
    signal,
  ).then((r): VisitorFigures => ({
    added: r.newVisitors,
    joined: r.convertedVisitors,
  }));

// Every member, and how many are men and women. Not the period.
export type GenderFigures = { total: number; male: number; female: number };

export const getGender = (signal?: AbortSignal) =>
  get<{ totalMembers: number; maleCount: number; femaleCount: number }>(
    "/v1/members/summary",
    ALL_TIME.from,
    ALL_TIME.to,
    signal,
  ).then((r): GenderFigures => ({
    total: r.totalMembers,
    male: r.maleCount,
    female: r.femaleCount,
  }));
