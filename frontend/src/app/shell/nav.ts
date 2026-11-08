import type { IconName } from "@/ui";

export type NavItem = { label: string; to: string; icon: IconName };

// The whole menu, in one place. The sidebar draws it as one list; the menu page on tablets and phones takes
// the same pages. The pages only administrators can open come after, and are drawn for them alone.
export const mainNav: NavItem[] = [
  { label: "Dashboard", to: "/dashboard", icon: "dashboard" },
  { label: "Members", to: "/members", icon: "user" },
  { label: "Visitors", to: "/visitors", icon: "door-in" },
  { label: "Organizations", to: "/organizations", icon: "organizations" },
  { label: "Attendance", to: "/attendance", icon: "calendar-check" },
  { label: "Events", to: "/events", icon: "calendar" },
  { label: "Tithes", to: "/tithes", icon: "tithes" },
  { label: "Finance", to: "/finance", icon: "finance" },
  { label: "Projects", to: "/projects", icon: "projects" },
  { label: "Assets", to: "/assets", icon: "assets" },
];

export const adminNav: NavItem[] = [
  { label: "Staff", to: "/staff", icon: "team" },
  { label: "Invitations", to: "/invitations", icon: "inbox" },
];
