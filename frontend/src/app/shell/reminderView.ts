import type { IconName } from "@/ui";

export type ReminderPhase = "idle" | "confirmed";

type Facts = {
  loaded: boolean;
  // The account is not verified, as far as we know right now.
  unverified: boolean;
  // It was unverified at some point while this page was open.
  sawUnverified: boolean;
  // "Account verified" has been shown for long enough.
  gone: boolean;
};

// Which of the reminder's faces to show. "hidden" means draw nothing.
export function reminderPhase(f: Facts): ReminderPhase | "hidden" {
  if (!f.loaded) return "hidden";
  if (f.unverified) return "idle";
  return f.sawUnverified && !f.gone ? "confirmed" : "hidden";
}

// What the reminder says. Each line comes in two lengths: the long one for tablets and wider, the short one
// for phones, which show less.
type Text = { long: string; short: string };

export type ReminderView = {
  icon: IconName;
  on?: boolean;
  title: Text;
  text: Text;
  // No action once the account is verified.
  action?: Text;
};

// The reminder has one job: send the person to the verify page, which sends the link and waits.
export function reminderView(
  phase: ReminderPhase,
  email: string,
): ReminderView {
  if (phase === "confirmed")
    return {
      icon: "check",
      on: true,
      title: { long: "Account verified", short: "Account verified" },
      text: { long: "Thank you. This reminder is gone.", short: "Thank you" },
    };
  return {
    icon: "mail",
    title: { long: "Verify your account", short: "Verify your account" },
    text: {
      long: `We will send a link to ${email}.`,
      short: "We will send a link",
    },
    action: { long: "Verify account", short: "Verify" },
  };
}
