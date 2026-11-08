// Words for the pricing page, as drawn on the boards.

export const BILL = [
  { label: "Per member", value: "GHS 0" },
  { label: "Per feature", value: "GHS 0" },
  { label: "Setup fee", value: "GHS 0" },
  { label: "Hidden fees", value: "We looked. None." },
] as const;

export const SANCTUARY = [
  {
    title: "Your own server",
    summary: "Runs on your servers, in the country you choose.",
    body: [
      "Nothing leaves them. Your data stays on hardware you control, behind your own network if you wish.",
    ],
  },
  {
    title: "Guided setup",
    summary: "We install it and set it up with you.",
    body: [
      "A named person walks your team through install, access and settings.",
    ],
  },
  {
    title: "Your data, moved across",
    summary: "We bring your records over.",
    body: [
      "From Excel or your current system. We tidy it on the way, so you start with clean records.",
    ],
  },
  {
    title: "Staff training",
    summary: "Live sessions for your admins and team.",
    body: ["Sessions are recorded, so new volunteers can catch up later."],
  },
  {
    title: "Updates and backups",
    summary: "We keep it current, with tested backups.",
    body: [
      "Updates land when it suits you, and we test that a restore really works.",
    ],
  },
  {
    title: "Direct support",
    summary: "Talk to the people who build Echo.",
    body: ["A real person replies, in your time zone where we can."],
  },
  {
    title: "Your name on it",
    summary: "Your church’s name, logo and web address.",
    body: ["Your logo and colours too, so it feels like your church’s own."],
  },
  {
    title: "A say in what’s next",
    summary: "Your requests go to the front of the line.",
    body: ["Echo Sanctuary churches help shape the roadmap with us."],
  },
] as const;

export const QUESTIONS = [
  {
    title: "What’s the catch?",
    body: [
      "There isn’t one. Echo is built in the open and kept going by gifts and by churches that run Echo Sanctuary.",
    ],
  },
  {
    title: "Will it stay free?",
    body: [
      "Yes. Day-to-day use of hosted Echo stays free. It is not moving behind a paywall.",
    ],
  },
  {
    title: "Can I pay you anyway?",
    body: [
      "Please do. Support Echo, or talk to us about Echo Sanctuary. Gifts go into the servers and the time behind each improvement.",
    ],
  },
] as const;
