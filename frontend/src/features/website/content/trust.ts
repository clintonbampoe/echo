// What the site says about handling people's data: the terms, the privacy policy and the security page.

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

export const SECURITY = {
  title: "How we handle your data",
  lead: "In plain words, so you do not have to read the small print to know where you stand.",
  rules: [
    {
      title: "What we keep",
      summary: "Only what your church puts into Echo.",
      body: [
        "Members, attendance, giving and events. We add no trackers and buy no data about your members.",
      ],
    },
    {
      title: "What we never do",
      summary: "Sell it, share it, or use it for advertising.",
      body: ["Your members’ details are only used to run your church’s Echo."],
    },
    {
      title: "Who can see it",
      summary: "Only the people your church gives access to.",
      body: [
        "Your admins choose who sees what. Our team looks at your data only when you ask us to help.",
      ],
    },
    {
      title: "Where it lives",
      summary: "On servers we run, or on your own.",
      body: ["With Echo Sanctuary you choose the country and the hardware."],
    },
    {
      title: "Leaving",
      summary: "Export or delete everything, any time.",
      body: [
        "Exports open in any spreadsheet. Deleting removes your data from our servers.",
      ],
    },
    {
      title: "Checking our work",
      summary: "The code is open source.",
      body: [
        "Anyone can see how Echo works. Developers and security researchers are welcome to read it and tell us what they find.",
      ],
    },
  ],
} as const;
