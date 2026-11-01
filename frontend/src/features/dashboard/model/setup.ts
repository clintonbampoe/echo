import type { IconName } from "@/ui";

// What the church has done so far. `undefined` means the answer has not come yet.
export type SetupFacts = {
  members: boolean | undefined;
  attendance: boolean | undefined;
  tithes: boolean | undefined;
  invitations: boolean | undefined;
};

export type SetupStep = {
  key: string;
  title: string;
  text: string;
  icon: IconName;
  action: string;
  // The page that owns the record. "?new=1" opens its New sheet.
  to: string;
  done: boolean;
};

// Registration creates the congregation, so the first step is always done.
export const setupSteps = (facts: SetupFacts): SetupStep[] => [
  {
    key: "church",
    title: "Set up your church",
    text: "Your church and its settings are ready.",
    icon: "settings",
    action: "Review settings",
    to: "/settings",
    done: true,
  },
  {
    key: "member",
    title: "Add your first member",
    text: "Start your register with one person. You can import the rest later.",
    icon: "user-plus",
    action: "Add a member",
    to: "/members?new=1",
    done: facts.members === true,
  },
  {
    key: "attendance",
    title: "Take attendance",
    text: "Record who came to your last gathering.",
    icon: "clipboard-check",
    action: "Take attendance",
    to: "/attendance",
    done: facts.attendance === true,
  },
  {
    key: "tithe",
    title: "Record a tithe",
    text: "Log a first tithe so the money pages come alive.",
    icon: "coin",
    action: "Record a tithe",
    to: "/tithes?new=1",
    done: facts.tithes === true,
  },
  {
    key: "team",
    title: "Invite your team",
    text: "Give a clerk or an accountant their own sign-in.",
    icon: "inbox",
    action: "Invite someone",
    to: "/invitations",
    done: facts.invitations === true,
  },
];

// The first step still to do, which is the one filled button of the card.
export const nextStep = (steps: readonly SetupStep[]) =>
  steps.find((s) => !s.done);

export const stepsDone = (steps: readonly SetupStep[]) =>
  steps.filter((s) => s.done).length;

// A new church has nothing yet. Until every answer is in we cannot say, so the card waits rather than flash.
export const factsKnown = (facts: SetupFacts) =>
  Object.values(facts).every((v) => v !== undefined);
