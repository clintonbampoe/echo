import { useState } from "react";
import { isApiError } from "@/lib/api";
import { Button, useCooldown } from "@/ui";
import styles from "./ResendButton.module.css";

const COOLDOWN_SECONDS = 30;

type Props = {
  // Does the sending. It is a prop so the same button serves "verify email" and "reset password".
  send: () => Promise<unknown>;
  // The one filled button on a screen is the most likely next step. Usually that is elsewhere.
  variant?: "primary" | "secondary";
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
