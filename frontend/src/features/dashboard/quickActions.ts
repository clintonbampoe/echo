import type { IconName } from "@/ui";

export type QuickAction = {
  key: string;
  label: string;
  hint: string;
  icon: IconName;
  // The page that owns the record. "?new=1" makes that page open its New sheet.
  to: string;
  // The main action, drawn dark. Only one.
  primary?: true;
};

// In the order they appear.
export const quickActions: QuickAction[] = [
  {
    key: "tithe",
    label: "Record tithe",
    hint: "A member's gift",
    icon: "coin",
    to: "/tithes?new=1",
    primary: true,
  },
  {
    key: "attendance",
    label: "Take attendance",
    hint: "Mark who came",
    icon: "clipboard-check",
    to: "/attendance",
  },
  {
    key: "member",
    label: "New member",
    hint: "Add a person",
    icon: "user-plus",
    to: "/members?new=1",
  },
  {
    key: "visitor",
    label: "Add visitor",
    hint: "A first-time guest",
    icon: "door-in",
    to: "/visitors?new=1",
  },
  {
    key: "transaction",
    label: "Record transaction",
    hint: "Income or expense",
    icon: "swap",
    to: "/finance?new=1",
  },
  {
    key: "event",
    label: "New event",
    hint: "Service or rally",
    icon: "calendar-plus",
    to: "/events?new=1",
  },
];
