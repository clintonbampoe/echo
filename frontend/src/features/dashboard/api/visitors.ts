import { api } from "@/lib/api";
import { monthsBefore } from "@/lib/dates";
import { get } from "./client";

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

// A visitor as the recent visitors card needs it, with the day it was added. Plain "2026-10-19".
export type RecentVisitor = { id: string; name: string; added: string };

// The newest visitors, taken from a page the server sends in name order. The server cannot order by
// when it added them, so this asks for a wide window and the card sorts the page it gets.
const VISITOR_WINDOW_MONTHS = 6;

const VISITOR_PAGE = 50;

export const getRecentVisitors = (today: string, signal?: AbortSignal) =>
  api
    .get<{ data: { id: string; name: string; createdAt: string }[] }>(
      "/v1/visitors",
      {
        query: {
          from: monthsBefore(today, VISITOR_WINDOW_MONTHS),
          to: today,
          pageSize: VISITOR_PAGE,
        },
        signal,
      },
    )
    .then((r) =>
      r.data.map((v): RecentVisitor => ({
        id: v.id,
        name: v.name,
        added: v.createdAt.slice(0, 10),
      })),
    );
