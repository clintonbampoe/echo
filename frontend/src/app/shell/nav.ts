import type { IconName } from "@/ui";

export type NavItem = {
  label: string;
  to: string;
  icon: IconName;
  // false until the page exists. Such an item shows "Soon" and cannot be clicked.
  ready: boolean;
};

export type NavGroup = { title?: string; items: NavItem[] };

// The whole menu, in one place. Turn `ready` on as each page is built.
export const navGroups: NavGroup[] = [
  {
    items: [
      { label: "Dashboard", to: "/dashboard", icon: "dashboard", ready: true },
    ],
  },
  {
    title: "People",
    items: [
      { label: "Members", to: "/members", icon: "user", ready: false },
      { label: "Visitors", to: "/visitors", icon: "visitor", ready: false },
      {
        label: "Organizations",
        to: "/organizations",
        icon: "organizations",
        ready: false,
      },
    ],
  },
  {
    title: "Worship",
    items: [
      {
        label: "Attendance",
        to: "/attendance",
        icon: "calendar-check",
        ready: false,
      },
      { label: "Events", to: "/events", icon: "calendar", ready: false },
    ],
  },
  {
    title: "Money",
    items: [
      { label: "Tithes", to: "/tithes", icon: "tithes", ready: false },
      { label: "Finance", to: "/finance", icon: "finance", ready: false },
      { label: "Projects", to: "/projects", icon: "projects", ready: false },
    ],
  },
  {
    title: "Church",
    items: [{ label: "Assets", to: "/assets", icon: "assets", ready: false }],
  },
  {
    title: "Insights",
    items: [
      { label: "Reports", to: "/reports", icon: "reports", ready: false },
    ],
  },
];
