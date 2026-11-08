// Words for the home page, as drawn on the boards.

export const HERO_QUESTION = "How many came last Sunday?";

export const HERO_STATS = [
  { label: "Giving", value: "GHS 10,400" },
  { label: "New members", value: "6 this month" },
  { label: "Next event", value: "Sat 14 Dec" },
] as const;

// What each card in the stack draws beside its figure.
export type Mark =
  | { kind: "line" }
  | { kind: "bar"; percent: number }
  | { kind: "people" }
  | { kind: "date"; month: string; day: string };

export const ANSWERS = [
  {
    q: "How many came on Sunday?",
    a: "342",
    note: "in the room, 12 of them first-time visitors",
    mark: { kind: "line" },
  },
  {
    q: "What did we give this month?",
    a: "GHS 10,400",
    note: "from 132 of 248 members",
    mark: { kind: "bar", percent: 53 },
  },
  {
    q: "Which project is closest?",
    a: "The borehole",
    note: "82% of GHS 40,000",
    mark: { kind: "bar", percent: 82 },
  },
  {
    q: "Who is new?",
    a: "6 new",
    note: "members this month, 248 in total",
    mark: { kind: "people" },
  },
  {
    q: "What is coming up?",
    a: "Youth retreat",
    note: "Sat 14 Dec, 86 going",
    mark: { kind: "date", month: "DEC", day: "14" },
  },
] as const satisfies readonly {
  q: string;
  a: string;
  note: string;
  mark: Mark;
}[];

export const FREE = [
  {
    title: "No plans",
    summary: "One Echo, the same for everyone.",
    body: ["There are no tiers and nothing sits behind a paywall."],
  },
  {
    title: "No per-member fees",
    summary: "Grow without the bill growing.",
    body: [
      "Your church pays the same whether it has fifty members or five thousand.",
    ],
  },
  {
    title: "No lock-in",
    summary: "Your data exports whenever you like.",
    body: ["Exports are files that open in any spreadsheet."],
  },
  {
    title: "Built in the open",
    summary: "Echo is open source.",
    body: [
      "Fix a bug, pick up an idea, shape what comes next. Anyone can read the code.",
    ],
  },
] as const;

export type Step = {
  title: string;
  line: string;
  text: string;
  state: "now" | "next" | "open";
};

export const ROADMAP: readonly Step[] = [
  {
    title: "Today",
    line: "Members, attendance, giving and events.",
    text: "Record members and visitors, take attendance, track giving and plan events, with answers you can read at a glance. This is what Echo does now.",
    state: "now",
  },
  {
    title: "Integrated payments",
    line: "Give by mobile money or card, recorded for you.",
    text: "Members give from their phone and the gift is recorded against the right person and fund, with no manual entry.",
    state: "next",
  },
  {
    title: "Native mobile apps",
    line: "Echo on iPhone and Android, made for the phone.",
    text: "Apps built for the phone, so taking attendance or checking an answer is quick, even one-handed.",
    state: "next",
  },
  {
    title: "Offline availability",
    line: "Keep taking attendance when the network drops.",
    text: "Attendance is saved on the device and syncs when the network returns.",
    state: "next",
  },
  {
    title: "What you ask for",
    line: "Tell us what your church needs.",
    text: "The roadmap starts with what churches tell us. Write to us and your request joins the list.",
    state: "open",
  },
];

export const WAYS = [
  {
    name: "Hosted by us",
    tag: "Free, ready in minutes",
    points: [
      "Ready in minutes, nothing to install",
      "Open source, so anyone can read the code",
      "Export or delete your data any time",
    ],
    action: { label: "Get started free", to: "/signup" },
  },
  {
    name: "Echo Sanctuary",
    tag: "On your own servers",
    points: [
      "Runs on your infrastructure, nothing leaves it",
      "We set it up with you",
      "Direct support from the people who build Echo",
    ],
    action: { label: "Talk to us", to: "/contact?topic=private" },
  },
] as const;
