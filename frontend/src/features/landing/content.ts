// All the words on the landing page, in one place. Components only lay them out.
import type { IconName } from "@/ui";
import attendance1x from "./images/attendance-1x.webp";
import attendance2x from "./images/attendance-2x.webp";
import dashboard1x from "./images/dashboard-1x.webp";
import dashboard2x from "./images/dashboard-2x.webp";
import finance1x from "./images/finance-1x.webp";
import finance2x from "./images/finance-2x.webp";
import projects1x from "./images/projects-1x.webp";
import projects2x from "./images/projects-2x.webp";
import tithes1x from "./images/tithes-1x.webp";
import tithes2x from "./images/tithes-2x.webp";

// Every screenshot is 1440x980 (and 2880x1960 for sharp screens).
export const SHOT_WIDTH = 1440;
export const SHOT_HEIGHT = 980;

export type Shot = { x1: string; x2: string; alt: string };

export const heroShot: Shot = {
  x1: dashboard1x,
  x2: dashboard2x,
  alt: "The Echo dashboard for Grace Chapel: members, last Sunday attendance, tithes this month, net income, attendance and income charts, upcoming events and recent visitors.",
};

export type ProductTab = {
  id: string;
  label: string;
  title: string;
  text: string;
  shot: Shot;
};

export const productTabs: readonly ProductTab[] = [
  {
    id: "attendance",
    label: "Attendance",
    title: "Take attendance in a few taps.",
    text: "Open the day’s session, search for a name and tap. Members and visitors are counted separately, and first-time guests are flagged.",
    shot: {
      x1: attendance1x,
      x2: attendance2x,
      alt: "The session page for Sunday Service: 214 present, 189 members, 25 visitors, 6 first-time visitors, with the list of people checked in.",
    },
  },
  {
    id: "tithes",
    label: "Tithes",
    title: "Every tithe, against the right member.",
    text: "Record what was given and the month it is for. Totals, averages and a twelve-month chart update as you go.",
    shot: {
      x1: tithes1x,
      x2: tithes2x,
      alt: "The Tithes page: GH₵ 18,450 collected, 62 members who gave, a monthly chart and a list of tithes.",
    },
  },
  {
    id: "finance",
    label: "Finance",
    title: "Know where the money comes from and goes.",
    text: "Log income and expenses under your own categories. The month’s net is always at the top, kept apart from tithes.",
    shot: {
      x1: finance1x,
      x2: finance2x,
      alt: "The Finance page: income, expenses and net, a 12-month chart, and a list of transactions.",
    },
  },
  {
    id: "projects",
    label: "Projects",
    title: "Raise money for what comes next.",
    text: "Set a target for the roof, the bus or the borehole. Record each contribution and watch the progress bar fill.",
    shot: {
      x1: projects1x,
      x2: projects2x,
      alt: "The Projects page: six projects with progress bars showing how much each has raised against its target.",
    },
  },
];

export type Feature = { icon: IconName; title: string; text: string };

export const features: readonly Feature[] = [
  {
    icon: "user",
    title: "Members",
    text: "Complete profiles with contact details, address, region and emergency contacts. Find anyone in a second.",
  },
  {
    icon: "calendar",
    title: "Events",
    text: "Plan conferences, retreats and outreaches. Members register, and you check them in on the day.",
  },
  {
    icon: "organizations",
    title: "Ministries and groups",
    text: "Organize choirs, youth, ushers and cell groups, each with their own members and leaders.",
  },
  {
    icon: "visitor",
    title: "Visitors",
    text: "Remember every first-time guest, follow up, and turn them into members when they decide to stay.",
  },
  {
    icon: "assets",
    title: "Assets",
    text: "Keep a register of everything the church owns, what it cost and what it is worth now.",
  },
  {
    icon: "link",
    title: "Invitations",
    text: "Invite your leaders with a link or a code. You choose each person’s role before they join.",
  },
];

export const steps = [
  {
    title: "Register your church",
    text: "Create your account and add your church details. You become its first administrator.",
  },
  {
    title: "Invite your people",
    text: "Send an invitation link or code. Each person joins your church directly, with the role you chose.",
  },
  {
    title: "Run the week",
    text: "Record attendance, giving and events as they happen, and see the whole picture on one dashboard.",
  },
] as const;
