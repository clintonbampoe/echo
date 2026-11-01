import { useState } from "react";
import { isApiError } from "@/lib/api";
import { Button, useCooldown } from "@/ui";
import styles from "./ResendButton.module.css";

const COOLDOWN_SECONDS = 30;

const clock = (seconds: number) =>
  `${Math.floor(seconds / 60)}:${String(seconds % 60).padStart(2, "0")}`;

type Props = {
  // Does the sending. It is a prop so the same button serves "verify email" and "reset password".
  send: () => Promise<unknown>;
  // The one filled button on a screen is the most likely next step. Usually that is elsewhere.
  variant?: "primary" | "secondary" | "link";
  // Called after an email went out, so the screen around the button can acknowledge it.
  onSent?: () => void;
};

// Sends an email, then waits before it can be used again. The API is rate limited, and a
// nervous double-click should not use up the allowance.
export function ResendButton({ send, variant = "secondary", onSent }: Props) {
  const [pending, setPending] = useState(false);
  const [wait, startWait] = useCooldown(COOLDOWN_SECONDS);
  const [problem, setProblem] = useState("");

  async function onClick() {
    setPending(true);
    setProblem("");
    try {
      await send();
      startWait();
      onSent?.();
    } catch (e) {
      if (isApiError(e) && e.status === 429) startWait();
      else
        setProblem(
          isApiError(e) ? e.message : "Could not send the email. Try again.",
        );
    } finally {
      setPending(false);
    }
  }

  if (variant === "link")
    return (
      <div>
        <p className={styles.line} aria-live="polite">
          {wait > 0 ? (
            `Send another link in ${clock(wait)}`
          ) : (
            <>
              Did not get it?{" "}
              <button
                type="button"
                className={styles.link}
                disabled={pending}
                onClick={onClick}
              >
                Send another link
              </button>
            </>
          )}
        </p>
        {problem && (
          <p role="alert" className={styles.problem}>
            {problem}
          </p>
        )}
      </div>
    );

  return (
    <div>
      <Button
        variant={variant}
        size="lg"
        pill
        block
        loading={pending}
        disabled={wait > 0}
        onClick={onClick}
      >
        {wait > 0 ? `Email sent. Resend in ${wait}s` : "Resend email"}
      </Button>
      {problem && (
        <p role="alert" className={styles.problem}>
          {problem}
        </p>
      )}
    </div>
  );
}
