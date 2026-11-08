export const ABOUT = {
  lead: "Echo began as a way to stop losing track of people. It became a way to see a whole congregation clearly, and to hear it back in plain numbers.",
  values: [
    {
      title: "Data you can use",
      text: "Storing records is the start. Getting answers back easily is the point.",
    },
    {
      title: "Calm by design",
      text: "Fewer things on screen, each one useful and pleasant to look at.",
    },
    {
      title: "Built with churches",
      text: "Every screen starts from how a congregation actually works.",
    },
  ],
} as const;

export const SECURITY = [
  {
    title: "Your data stays yours",
    text: "Each congregation is a private space. Choose a private deployment and nothing leaves your server.",
  },
  {
    title: "Roles you control",
    text: "Choose who can see giving, who can edit members and who can only view.",
  },
  {
    title: "Protected by default",
    text: "Data is encrypted in transit and backed up automatically.",
  },
  {
    title: "Open to inspection",
    text: "The code is public. Anyone can audit it, and security reports are welcome.",
  },
] as const;

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
  private: [
    {
      title: "Your server, your rules",
      text: "Echo runs on infrastructure you control.",
    },
    {
      title: "Your own domain",
      text: "Use your church's address, such as app.gracechapel.org.",
    },
    {
      title: "Everything included",
      text: "Setup, backups and updates, with a named person to call.",
    },
    { title: "Priority support", text: "Help in hours, not days." },
  ],
} as const;

const PLACEHOLDER =
  "Placeholder text for design purposes. The final wording will be reviewed before launch.";

// One rule: a title, one line that says it plainly, and the full wording under it.
type Rule = { title: string; summary?: string; body: readonly string[] };

export const LEGAL = {
  terms: {
    kicker: "The rules, in plain words",
    title: "Terms of use",
    rules: [
      {
        title: "Using Echo",
        summary: "Echo is for running and caring for your church.",
        body: [
          "You may use Echo to manage your church: its members, attendance, giving and events. You must have the authority to act for the church you register.",
          "You are responsible for your account, for keeping your sign-in details safe, and for the people you give access to. If you think someone else has used your account, change your password and tell us straight away.",
        ],
      },
      {
        title: "Your data",
        summary: "What you put into Echo belongs to your church.",
        body: [
          "Everything your church adds to Echo, including member records, attendance, giving and events, stays yours. We store and process it only to run Echo for you. We do not sell it, share it for advertising, or use it to build profiles of your members.",
          "You can export your data at any time, and you can delete it. Deleting removes it from our servers; copies held in backups are removed as those backups expire.",
          "Your church is responsible for having the right to keep its members’ information, and for using it as the data protection law where you are requires, such as the Data Protection Act, 2012 (Act 843) in Ghana.",
        ],
      },
      {
        title: "What not to do",
        summary: "Do not use Echo to harm people or the service.",
        body: [
          "Do not use Echo to harass, threaten or deceive anyone, or to send messages people have not asked for. Do not try to reach data that is not yours, and do not overload or break the hosted service.",
          "If you find a security problem, please report it to us instead of using it. We may suspend an account that puts other churches or the service at risk, and we will tell you why.",
        ],
      },
      {
        title: "Free, and Echo Sanctuary",
        summary: "Hosted Echo is free. Sanctuary is agreed in writing.",
        body: [
          "Hosted Echo is free for every church, with no per-member fees. If that ever changes, we will tell you first, as described under Changes.",
          "Echo Sanctuary runs on your own servers. Its scope, price and support are agreed with you in writing before we start, and that agreement applies in place of anything here that conflicts with it.",
          "Gifts to support Echo are voluntary. They are not payment for features and do not buy any promise from us.",
        ],
      },
      {
        title: "Open source",
        summary: "The code is open. The name and logo are ours.",
        body: [
          "Echo’s source code is published under an open-source licence, and the licence file in the repository says what you may do with it.",
          "The Echo name and logo are not covered by that licence. Please do not use them in a way that suggests we made, endorse or support something we did not.",
        ],
      },
      {
        title: "When things go wrong",
        summary:
          "We work to keep Echo running, but cannot promise it never fails.",
        body: [
          "We work hard to keep Echo available and your data safe, and we keep backups. But Echo is provided as it is, and we cannot promise it will always be available or free of errors.",
          "Because hosted Echo is free, our responsibility for losses is limited as far as the law allows. Keep your own export of anything you cannot afford to lose.",
        ],
      },
      {
        title: "Changes",
        summary: "We will tell you before changes that matter.",
        body: [
          "If we change these terms in a way that matters, we will tell you before the change takes effect, by email to the account owner and with a notice in Echo.",
          "If you do not agree, you can export your data and stop using Echo. If you keep using Echo after the change takes effect, you accept the new terms.",
        ],
      },
      {
        title: "Law and questions",
        summary: "These terms follow the laws of Ghana.",
        body: [
          "These terms are governed by the laws of Ghana. If you have a question about them, write to us through the contact page and a real person will answer.",
        ],
      },
    ],
  },
  // The words are placeholders until they have been reviewed.
  privacy: {
    kicker: "How we handle your information",
    title: "Privacy policy",
    rules: [
      "What we collect",
      "How we use it",
      "Who can see it",
      "Your choices",
      "Contact",
    ].map((title): Rule => ({ title, body: [PLACEHOLDER] })),
  },
  lead: "The short version comes first. The full wording sits under each point.",
  updated: "Last updated 7 November 2026",
} as const;
