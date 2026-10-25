import type { IconName } from "@/ui";

export type QuickAction = {
  key: string;
  label: string;
  icon: IconName;
  // The page that owns the record. "?new=1" makes that page open its New sheet.
  to: string;
  // The main action, drawn dark. Only one.
  primary?: true;
};

// In the order they appear.
export const quickActions: QuickAction[] = [
  {
    key: "attendance",
    label: "Take attendance",
    icon: "clipboard-check",
    to: "/attendance",
    primary: true,
  },
  {
    key: "tithe",
    label: "Record tithe",
    icon: "coin",
    to: "/tithes?new=1",
  },
  {
    key: "member",
    label: "New member",
    icon: "user-plus",
    to: "/members?new=1",
  },
  {
    key: "visitor",
    label: "Add visitor",
    icon: "door-in",
    to: "/visitors?new=1",
  },
  {
    key: "transaction",
    label: "Record transaction",
    icon: "swap",
    to: "/finance?new=1",
  },
  {
    key: "event",
    label: "New event",
    icon: "calendar-plus",
    to: "/events?new=1",
  },
];
