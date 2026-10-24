export const ABOUT = {
  lead: "Echo began as a way to stop losing track of people. It became a way to see a whole congregation clearly, and to hear it back in plain numbers.",
  values: [
    { title: "Data you can use", text: "Storing records is the start. Getting answers back easily is the point." },
    { title: "Calm by design", text: "Fewer things on screen, each one useful and pleasant to look at." },
    { title: "Built with churches", text: "Every screen starts from how a congregation actually works." },
  ],
} as const;

export const SECURITY = [
  { title: "Your data stays yours", text: "Each congregation is a private space. Choose a private deployment and nothing leaves your server." },
  { title: "Roles you control", text: "Choose who can see giving, who can edit members and who can only view." },
  { title: "Protected by default", text: "Data is encrypted in transit and backed up automatically." },
  { title: "Open to inspection", text: "The code is public. Anyone can audit it, and security reports are welcome." },
] as const;

export const ROADMAP = [
  { when: "Now", note: "Being built", items: ["Redesigned app", "Clickable charts", "Invitations that can be reused"] },
  { when: "Next", note: "Up next", items: ["Reports", "Offline attendance", "Fine-grained roles"] },
  { when: "Later", note: "On the horizon", items: ["Giving statements", "SMS reminders", "Native mobile app"] },
] as const;

export const CHANGELOG = [
  { date: "Oct 2026", title: "A new look", text: "A calmer layout, one typeface, a phone-first menu and a dashboard that groups what belongs together." },
  { date: "Sep 2026", title: "Clickable charts", text: "Select a month or week and see the records behind it." },
  { date: "Aug 2026", title: "Day-by-day attendance", text: "The day dial makes Sundays quick to record." },
  { date: "Jul 2026", title: "Events, past and upcoming", text: "One list, with registration and check-in." },
] as const;

export const DOCS = {
  topics: [
    { title: "Getting started", text: "Register your church and add your first members." },
    { title: "People", text: "Members, visitors, groups and invitations." },
    { title: "Attendance and events", text: "Record a Sunday, run an event, check people in." },
    { title: "Giving and finance", text: "Record giving and read your charts." },
    { title: "Projects and assets", text: "Fund what you build and track what you own." },
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
  ways: [
    { title: "Read the code", text: "Everything is public. See how Echo works and what it does with your data." },
    { title: "Suggest a feature", text: "Tell us what your church needs. Ideas are voted on and land on the roadmap." },
    { title: "Send a fix", text: "Found a bug or want to improve a screen? Open a pull request." },
  ],
  private: [
    { title: "Your server, your rules", text: "Echo runs on infrastructure you control." },
    { title: "Your own domain", text: "Use your church's address, such as app.gracechapel.org." },
    { title: "Everything included", text: "Setup, backups and updates, with a named person to call." },
    { title: "Priority support", text: "Help in hours, not days." },
  ],
} as const;

const PLACEHOLDER =
  "Placeholder text for design purposes. The final wording will be reviewed before launch.";

export const LEGAL = {
  privacy: {
    title: "Privacy policy",
    sections: ["What we collect", "How we use it", "Who can see it", "Your choices", "Contact"],
  },
  terms: {
    title: "Terms of service",
    sections: ["Using Echo", "Your content", "Open source licence", "Ending your account", "Changes"],
  },
  updated: "Updated 24 October 2026",
  placeholder: PLACEHOLDER,
} as const;
