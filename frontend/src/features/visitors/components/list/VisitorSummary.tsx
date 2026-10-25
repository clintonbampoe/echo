import { formatNumber } from "@/lib/format";
import { StatStrip, type Stat } from "@/ui";
import type { VisitorSummary } from "../../api/visitors";

type Props = { summary?: VisitorSummary; windowLabel: string };

// The four numbers above the list. They follow the time range, not the status or the search.
export function VisitorSummary({ summary: s, windowLabel }: Props) {
  const n = (value: number | undefined) =>
    s && value !== undefined ? formatNumber(value) : "–";
  const conversion =
    s && s.newVisitors > 0
      ? String(Math.round((s.convertedVisitors / s.newVisitors) * 100))
      : "–";
  const stats: Stat[] = [
    {
      label: windowLabel,
      value: n(s?.newVisitors),
      note: "Added, including those who became members",
    },
    {
      label: "Conversion",
      value: conversion,
      unit: conversion === "–" ? undefined : "%",
      note: "Share who became members",
    },
    {
      label: "Returning",
      value: n(s?.recurringVisitors),
      note: "Came more than once",
    },
    {
      label: "Not yet members",
      value: n(s?.totalVisitors),
      note: "Still visitors",
    },
  ];
  return <StatStrip label="Visitor summary" stats={stats} busy={!s} />;
}
