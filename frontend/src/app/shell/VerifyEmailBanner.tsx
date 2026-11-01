import { useState } from "react";
import { Button, StatusTile, useMedia, usePresence } from "@/ui";
import { reminderView, type ReminderPhase } from "./reminderView";
import { useVerifyReminder } from "./useVerifyReminder";
import styles from "./VerifyEmailBanner.module.css";

// Signing in does not need a verified email, so this is where the reminder lives: one quiet card at the top
// of every page until the address is confirmed. It reads the profile the shell already loads, so it costs no
// request of its own until a link has been sent.
export function VerifyEmailBanner() {
  const reminder = useVerifyReminder();
  const phone = useMedia("(max-width: 600px)");
  const { mounted, closing } = usePresence(reminder.phase !== "hidden", 200);

  // Keep the last thing it said, so it can finish saying it while it leaves.
  const [last, setLast] = useState<ReminderPhase>("idle");
  if (reminder.phase !== "hidden" && reminder.phase !== last)
    setLast(reminder.phase);

  if (!mounted) return null;
  const view = reminderView(last, reminder.email, reminder.wait);
  return (
    <section
      aria-label="Account verification"
      className={`${styles.banner} pop`}
      data-closing={closing}
    >
      <StatusTile
        icon={view.icon}
        on={view.on}
        tone={view.tone}
        size={phone ? 40 : 44}
      />
      <div className={styles.words} aria-live="polite">
        <p className={styles.title}>
          {phone ? view.title.short : view.title.long}
        </p>
        <p className={styles.text}>
          {phone ? view.text.short : view.text.long}
        </p>
      </div>
      {view.action && (
        <Button
          variant="secondary"
          pill
          className={styles.action}
          loading={reminder.sending}
          disabled={view.action.disabled}
          onClick={() => void reminder.onSend()}
        >
          {phone ? view.action.short : view.action.long}
        </Button>
      )}
    </section>
  );
}
