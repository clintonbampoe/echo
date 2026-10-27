export const ANSWERS = [
  {
    q: "How many came on Sunday?",
    a: "342",
    note: "in the room, 12 of them first-timers",
  },
  {
    q: "What did we give this month?",
    a: "GHS 10,400",
    note: "from 132 of 248 members",
  },
  {
    q: "Which project is closest to its goal?",
    a: "The borehole",
    note: "82% of GHS 40,000 raised",
  },
  {
    q: "Who joined this year?",
    a: "60 new members",
    note: "up 30% on last year",
  },
] as const;

export const STORIES = [
  {
    number: "01",
    label: "People and Sundays",
    title: "Know who is there, and who is new.",
    text: "Take attendance in a few taps. Members and visitors are counted apart, first-time guests are flagged, and every person has one profile that follows them.",
    points: [
      "Search a name or phone, tap, done",
      "First-time visitors flagged for follow-up",
      "One profile for contacts, groups and giving",
    ],
  },
  {
    number: "02",
    label: "Giving and money",
    title: "Know where the money comes from and goes.",
    text: "Record tithes and offerings against the right member, log expenses under your own categories, and read the month on one honest chart.",
    points: [
      "Income above the line, expenses below",
      "Tap a month to open the records behind it",
      "Mobile money, cash and transfers kept apart",
    ],
  },
  {
    number: "03",
    label: "Announcements",
    title: "Sunday's announcement, written for you.",
    text: "One tap turns the week's giving into a message you can read from the pulpit or send to the group chat.",
    points: [
      "This week, last week or this month",
      "Copy it or share it straight away",
      "Every figure ties back to a record",
    ],
  },
] as const;

export const EVERYTHING = [
  { title: "Events", text: "Plan, register and check people in on the day." },
  { title: "Projects", text: "Set a target and watch the progress bar fill." },
  {
    title: "Ministries and groups",
    text: "Choirs, youth, ushers and cell groups, each with its own members.",
  },
  {
    title: "Visitors",
    text: "Follow up every first-time guest, then welcome them as members.",
  },
  { title: "Assets", text: "What the church owns and what it is worth now." },
  {
    title: "Invitations and roles",
    text: "Bring leaders in with a reusable link or code.",
  },
] as const;
