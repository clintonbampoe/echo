import type { IconName } from "@/ui";

export type NavItem = { label: string; to: string; icon: IconName };
export type NavGroup = { heading?: string; items: NavItem[] };

// The whole menu, in one place. The sidebar draws the groups; the menu page on tablets and phones takes
// the same pages as one flat list. The pages only administrators can open sit under their own heading.
export const navGroups: NavGroup[] = [
  {
    items: [{ label: "Dashboard", to: "/dashboard", icon: "dashboard" }],
  },
  {
    heading: "People",
    items: [
      { label: "Members", to: "/members", icon: "user" },
      { label: "Visitors", to: "/visitors", icon: "door-in" },
      { label: "Organizations", to: "/organizations", icon: "organizations" },
    ],
  },
  {
    heading: "Gatherings",
    items: [
      { label: "Attendance", to: "/attendance", icon: "calendar-check" },
      { label: "Events", to: "/events", icon: "calendar" },
    ],
  },
  {
    heading: "Money",
    items: [
      { label: "Tithes", to: "/tithes", icon: "tithes" },
      { label: "Finance", to: "/finance", icon: "finance" },
      { label: "Projects", to: "/projects", icon: "projects" },
      { label: "Assets", to: "/assets", icon: "assets" },
    ],
  },
];

export const mainNav: NavItem[] = navGroups.flatMap((g) => g.items);

export const adminNav: NavItem[] = [
  { label: "Invitations", to: "/invitations", icon: "inbox" },
];
