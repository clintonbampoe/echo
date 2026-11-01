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

// Registration creates the church, so the first step is always done.
export const setupSteps = (
  facts: SetupFacts,
  churchName: string,
): SetupStep[] => [
  {
    key: "church",
    title: "Set up your church",
    text: `${churchName} has its default categories and session types.`,
    icon: "settings",
    action: "Review settings",
    to: "/settings",
    done: true,
  },
  {
    key: "member",
    title: "Add your first member",
    text: "Start with yourself, or someone from last Sunday.",
    icon: "user-plus",
    action: "Add member",
    to: "/members?new=1",
    done: facts.members === true,
  },
  {
    key: "attendance",
    title: "Take your first attendance",
    text: "Open a Sunday session and mark who came.",
    icon: "clipboard-check",
    action: "Take attendance",
    to: "/attendance",
    done: facts.attendance === true,
  },
  {
    key: "tithe",
    title: "Record a tithe",
    text: "Log what a member gave this month.",
    icon: "coin",
    action: "Record tithe",
    to: "/tithes?new=1",
    done: facts.tithes === true,
  },
  {
    key: "team",
    title: "Invite your team",
    text: "Share a code so others can help run things.",
    icon: "inbox",
    action: "Invite team",
    to: "/invitations",
    done: facts.invitations === true,
  },
];

// The first step still to do, which is the one filled button of the card.
export const nextStep = (steps: readonly SetupStep[]) =>
  steps.find((s) => !s.done);

export const stepsDone = (steps: readonly SetupStep[]) =>
  steps.filter((s) => s.done).length;

// Until every answer is in we cannot say, so the card waits rather than flash.
export const factsKnown = (facts: SetupFacts) =>
  Object.values(facts).every((v) => v !== undefined);

// A church with no member, no attendance and no tithe has nothing to chart yet.
export const nothingYet = (facts: SetupFacts) =>
  facts.members === false &&
  facts.attendance === false &&
  facts.tithes === false;

const WORDS = ["No", "One", "Two", "Three", "Four", "Five"];

// "Four more steps", for the welcome line.
export const stepsLeft = (n: number) =>
  `${WORDS[n] ?? n} more ${n === 1 ? "step" : "steps"}`;

export type SetupState = "off" | "loading" | "on";

// Off for anyone but an admin, and once hidden; loading until every answer is in.
export const setupState = (
  admin: boolean,
  dismissed: boolean,
  ready: boolean,
): SetupState => {
  if (!admin || dismissed) return "off";
  return ready ? "on" : "loading";
};

// The line under a step's title. The slim rows of the phone's panel say where each stands; the full rows say
// what the step is for, except a done one, which only keeps its line while it is the one thing done.
export function stepLine(
  state: "done" | "next" | "todo",
  text: string,
  dense: boolean,
  note: boolean,
): string | undefined {
  if (dense) {
    if (state === "next") return "Next step";
    return state === "done" ? "Done" : undefined;
  }
  return state !== "done" || note ? text : undefined;
}
