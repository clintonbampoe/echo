import type { IconName } from "@/ui";

export type NavItem = { label: string; to: string; icon: IconName };

// The whole menu, in one place: the app's pages in the order people reach for them, and the pages only
// administrators can open, which sit under their own small heading.
export const mainNav: NavItem[] = [
  { label: "Dashboard", to: "/dashboard", icon: "dashboard" },
  { label: "Members", to: "/members", icon: "user" },
  { label: "Visitors", to: "/visitors", icon: "door-in" },
  { label: "Attendance", to: "/attendance", icon: "calendar-check" },
  { label: "Events", to: "/events", icon: "calendar" },
  { label: "Finance", to: "/finance", icon: "finance" },
  { label: "Tithes", to: "/tithes", icon: "tithes" },
  { label: "Projects", to: "/projects", icon: "projects" },
  { label: "Assets", to: "/assets", icon: "assets" },
  { label: "Organizations", to: "/organizations", icon: "organizations" },
];

export const adminNav: NavItem[] = [
  { label: "Invitations", to: "/invitations", icon: "inbox" },
];
