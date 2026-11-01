import type { IconName } from "@/ui";

export type ReminderPhase = "idle" | "sent" | "failed" | "confirmed";

type Facts = {
  loaded: boolean;
  // The address is not verified, as far as we know right now.
  unverified: boolean;
  // It was unverified at some point while this page was open.
  sawUnverified: boolean;
  // "Account verified" has been shown for long enough.
  gone: boolean;
  sent: boolean;
  failed: boolean;
};

// Which of the reminder's faces to show. "hidden" means draw nothing.
export function reminderPhase(f: Facts): ReminderPhase | "hidden" {
  if (!f.loaded) return "hidden";
  if (f.unverified) return f.failed ? "failed" : f.sent ? "sent" : "idle";
  return f.sawUnverified && !f.gone ? "confirmed" : "hidden";
}

// What the reminder says. Each line comes in two lengths: the long one for tablets and wider, the short one
// for phones, which show less.
type Text = { long: string; short: string };

export type ReminderView = {
  icon: IconName;
  on?: boolean;
  tone?: "neg";
  title: Text;
  text: Text;
  // No action once the address is confirmed.
  action?: Text & { disabled?: boolean };
};

const clock = (seconds: number) =>
  `${Math.floor(seconds / 60)}:${String(seconds % 60).padStart(2, "0")}`;

export function reminderView(
  phase: ReminderPhase,
  email: string,
  wait: number,
): ReminderView {
  switch (phase) {
    case "idle":
      return {
        icon: "mail",
        title: {
          long: "Verify your account",
          short: "Verify your account",
        },
        text: {
          long: `We will send a link to ${email}.`,
          short: "Send a link to your inbox",
        },
        action: { long: "Send link", short: "Send link" },
      };
    case "sent":
      return {
        icon: "check",
        on: true,
        title: { long: "Link on its way", short: "Check your email" },
        // The same sentence whatever the server knows: it never says whether the address exists.
        text: {
          long: "If that email is registered and unverified, a verification link is on its way.",
          short: wait > 0 ? `Resend in ${clock(wait)}` : "Check your email",
        },
        action: {
          long: wait > 0 ? `Resend in ${clock(wait)}` : "Resend",
          short: "Resend",
          disabled: wait > 0,
        },
      };
    case "failed":
      return {
        icon: "alert",
        tone: "neg",
        title: { long: "We could not send the link", short: "Could not send" },
        text: {
          long: "Check your connection and try again.",
          short: "Check your connection",
        },
        action: { long: "Try again", short: "Retry" },
      };
    case "confirmed":
      return {
        icon: "check",
        on: true,
        title: { long: "Account verified", short: "Account verified" },
        text: { long: "Thank you. This reminder is gone.", short: "Thank you" },
      };
  }
}
