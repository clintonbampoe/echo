import { useEffect, useState } from "react";
import { isApiError } from "@/lib/api";
import { Button } from "@/ui";
import styles from "./ResendButton.module.css";

const COOLDOWN_SECONDS = 30;

type Props = {
  // Does the sending. It is a prop so the same button serves "verify email" and "reset password".
  send: () => Promise<unknown>;
};

// Sends an email, then waits before it can be used again. The API is rate limited, and a
// nervous double-click should not use up the allowance.
export function ResendButton({ send }: Props) {
  const [pending, setPending] = useState(false);
  const [wait, setWait] = useState(0);
  const [problem, setProblem] = useState("");

  useEffect(() => {
    if (wait <= 0) return;
    const timer = setTimeout(() => setWait(wait - 1), 1000);
    return () => clearTimeout(timer);
  }, [wait]);

  async function onClick() {
    setPending(true);
    setProblem("");
    try {
      await send();
      setWait(COOLDOWN_SECONDS);
    } catch (e) {
      if (isApiError(e) && e.status === 429) setWait(COOLDOWN_SECONDS);
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
        variant="secondary"
        size="lg"
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
