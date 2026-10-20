// Turns the numbers the server sent into what the four cards show. Pure, so the tests need no screen.
import { formatMoney, formatNumber } from "@/lib/format";
import type { Period } from "./periods";

// One number on its way: not here yet, here, or failed. `retry` asks for it again.
export type Amount = {
  value: number | undefined;
  failed: boolean;
  retry: () => void;
};

// The numbers the cards need. "Before" is the stretch of the same length before the period.
export type Numbers = {
  members: Amount;
  newMembers: Amount;
  present: Amount;
  presentBefore: Amount;
  tithes: Amount;
  tithesBefore: Amount;
  net: Amount;
  netBefore: Amount;
};

export type Tone = "positive" | "negative" | "neutral";
export type CardChip = { text: string; tone: Tone; arrow?: "up" | "down" };

export type CardView = {
  key: string;
  label: string;
  state: "loading" | "failed" | "ready";
  value: string;
  chip?: CardChip;
  note: string;
  retry: () => void;
};

// How far a number moved, in percent to one decimal. Nothing to say when it started from zero.
export function change(now: number, before: number): CardChip | undefined {
  if (before === 0) return undefined;
  const percent = Math.round(((now - before) / Math.abs(before)) * 1000) / 10;
  if (percent === 0) return { text: "0%", tone: "neutral" };
  const up = percent > 0;
  return {
    text: `${formatNumber(Math.abs(percent))}%`,
    tone: up ? "positive" : "negative",
    arrow: up ? "up" : "down",
  };
}

const stateOf = (a: Amount): CardView["state"] =>
  a.failed ? "failed" : a.value === undefined ? "loading" : "ready";

const versus = (now: Amount, before: Amount) =>
  now.value !== undefined && before.value !== undefined
    ? change(now.value, before.value)
    : undefined;

function card(
  key: string,
  label: string,
  now: Amount,
  format: (n: number) => string,
  note: string,
  chip: CardChip | undefined,
): CardView {
  const state = stateOf(now);
  return {
    key,
    label,
    state,
    value:
      state === "ready" && now.value !== undefined ? format(now.value) : "–",
    chip: state === "ready" ? chip : undefined,
    note: state === "failed" ? "Could not load" : note,
    retry: now.retry,
  };
}

function newMembersChip(count: number | undefined): CardChip | undefined {
  if (count === undefined) return undefined;
  return {
    text: `${formatNumber(count)} new`,
    tone: count > 0 ? "positive" : "neutral",
  };
}

export function buildCards(n: Numbers, period: Period): CardView[] {
  return [
    card(
      "members",
      "Members",
      n.members,
      formatNumber,
      period.label.toLowerCase(),
      newMembersChip(n.newMembers.value),
    ),
    card(
      "present",
      "Present",
      n.present,
      formatNumber,
      period.before,
      versus(n.present, n.presentBefore),
    ),
    card(
      "tithes",
      "Tithes",
      n.tithes,
      formatMoney,
      period.before,
      versus(n.tithes, n.tithesBefore),
    ),
    card(
      "net",
      "Net income",
      n.net,
      formatMoney,
      period.before,
      versus(n.net, n.netBefore),
    ),
  ];
}
