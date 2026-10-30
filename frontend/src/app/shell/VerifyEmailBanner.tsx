import { useState } from "react";
import { useMe } from "@/features/account";
import { useSendVerification } from "@/features/auth";
import { Alert } from "@/ui";
import styles from "./VerifyEmailBanner.module.css";

// Signing in does not need a verified email, so this is where the reminder lives. It reads the
// profile the shell already loads, so it costs no request of its own.
export function VerifyEmailBanner() {
  const me = useMe().data;
  const send = useSendVerification();
  const [sent, setSent] = useState(false);
  if (!me || me.emailVerifiedAt !== null) return null;

  async function onSend() {
    await send.mutateAsync(me!.emailAddress);
    setSent(true);
  }

  return (
    <div className={styles.banner}>
      <Alert
        title="Verify your email address"
        action={
          sent ? undefined : (
            <button
              type="button"
              className={styles.link}
              onClick={() => void onSend().catch(() => undefined)}
              disabled={send.isPending}
            >
              Send me a verification link
            </button>
          )
        }
      >
        {sent
          ? `We sent a link to ${me.emailAddress}. Open it to finish.`
          : `We have not confirmed ${me.emailAddress} yet.`}
      </Alert>
    </div>
  );
}
