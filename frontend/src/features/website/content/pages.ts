import { GITHUB_URL } from "./site";

export const ABOUT = {
  title: "Built so your church can spend its time on people",
  lead: "Echo is free, open source, and made in the open for churches everywhere.",
  promises: [
    {
      title: "Free",
      summary:
        "No plans, no per-member fees, no lock-in. The free part stays free.",
      body: [
        "Gifts and Echo Sanctuary churches keep Echo going. Nothing a church needs day to day sits behind a paywall.",
      ],
    },
    {
      title: "Open",
      summary:
        "The code is public, and we are plain about how your data is handled.",
      body: [
        "Anyone can read the code, check how your data is handled and suggest a fix. Developers are welcome to help build it.",
      ],
    },
    {
      title: "Yours",
      summary: "Export it any time, or run it on your own servers.",
      body: [
        "Everything you put in comes back out in files you can open anywhere. Or take Echo to your own servers with Echo Sanctuary.",
      ],
    },
  ],
  why: [
    "Most of what keeps a church going is volunteers, time and care. The software meant to help often costs more than a church can spare, and asks it to hand over its members’ data.",
    "Echo is our answer: the same tools, free, open, and yours. It is built in Accra, and shaped by the churches that use it. Every idea on the roadmap began with someone who runs a church.",
  ],
} as const;

export const ROADMAP = [
  {
    when: "Now",
    note: "Being built",
    items: [
      "Redesigned app",
      "Clickable charts",
      "Invitations that can be reused",
    ],
  },
  {
    when: "Next",
    note: "Up next",
    items: ["Reports", "Offline attendance", "Fine-grained roles"],
  },
  {
    when: "Later",
    note: "On the horizon",
    items: ["Giving statements", "SMS reminders", "Native mobile app"],
  },
] as const;

export const CHANGELOG = [
  {
    date: "Oct 2026",
    title: "A new look",
    text: "A calmer layout, one typeface, a phone-first menu and a dashboard that groups what belongs together.",
  },
  {
    date: "Sep 2026",
    title: "Clickable charts",
    text: "Select a month or week and see the records behind it.",
  },
  {
    date: "Aug 2026",
    title: "Day-by-day attendance",
    text: "The day dial makes Sundays quick to record.",
  },
  {
    date: "Jul 2026",
    title: "Events, past and upcoming",
    text: "One list, with registration and check-in.",
  },
] as const;

export const DOCS = {
  topics: [
    {
      title: "Getting started",
      text: "Register your church and add your first members.",
    },
    { title: "People", text: "Members, visitors, groups and invitations." },
    {
      title: "Attendance and events",
      text: "Record a Sunday, run an event, check people in.",
    },
    {
      title: "Giving and finance",
      text: "Record giving and read your charts.",
    },
    {
      title: "Projects and assets",
      text: "Fund what you build and track what you own.",
    },
    { title: "Self-hosting", text: "Run Echo on your own server." },
  ],
  popular: [
    "How do I take attendance from my phone?",
    "How do I invite another admin?",
    "How do I record a tithe?",
    "How do I move a transaction to another category?",
    "How do I self-host Echo?",
  ],
} as const;

export const COMMUNITY = {
  // How a change gets from someone's idea to every church, one step at a time.
  steps: [
    {
      title: "Find a bug or an idea.",
      text: "Spot something broken, think of a feature churches need, or read the code to see how Echo works.",
    },
    {
      title: "Make your own copy.",
      text: "Fork Echo on GitHub. The copy is yours to change, and nothing you do there can break the real one.",
    },
    {
      title: "Fix it or build it.",
      text: "Change the code in your copy. Fix the bug, or add the feature.",
    },
    {
      title: "Send it for review.",
      text: "Open a pull request. That is how you offer your change back to Echo.",
    },
    {
      title: "We check it, then merge it.",
      text: "A maintainer reads your change, asks questions if there are any, and merges it into Echo.",
    },
    {
      title: "Every church gets it.",
      text: "Your change ships to everyone using Echo. You helped shape what comes next.",
    },
  ],
} as const;

export const CONTACT = {
  lead: "Tell us about your church and what you need. A real person on the team replies, and nothing is sent until you press the button.",
  steps: [
    {
      title: "We talk",
      summary: "A short conversation about your church.",
      body: [
        "No forms to fill in first. Tell us about your church and what you need, and we listen.",
      ],
    },
    {
      title: "We plan it",
      summary: "Servers, data and people, agreed together.",
      body: [
        "We agree where it runs, what moves across and who needs training, with no surprises.",
      ],
    },
    {
      title: "We set it up",
      summary: "And look after it afterwards.",
      body: [
        "We install, move your data and train your team, then stay on for updates and backups.",
      ],
    },
  ],
} as const;

// Where the community page sends people.
export const COMMUNITY_ROWS = [
  {
    title: "Help build it",
    text: "Open source. Fix a bug, shape what comes next.",
    to: GITHUB_URL,
    external: true,
  },
  {
    title: "Support Echo",
    text: "Gifts go into the servers and the time behind each improvement.",
    to: "/contact?topic=support",
  },
  {
    title: "Tell us what you need",
    text: "The roadmap starts with what churches ask for.",
    to: "/contact",
  },
] as const;
