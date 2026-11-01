import { useState } from "react";
import { Navigate, useLocation } from "react-router-dom";
import { buttonClass, Icon, LinkButton } from "@/ui";
import { ResendButton } from "../components/ResendButton";
import { AuthScreen } from "../components/AuthScreen";
import { sendVerificationLink } from "../api/auth";
import { mailProviderFor } from "../model/mailProvider";
import form from "./form.module.css";
import styles from "./VerifySentPage.module.css";

// "Check your email." Registering makes a working account, so this is a pause rather than a gate: open
// the mail, ask for another link, or skip to sign-in. The address arrives from the page that registered,
// passed along in the navigation state. Opened directly there is nothing to say, so go to sign-in.
export function VerifySentPage() {
  const email = (useLocation().state as { email?: string } | null)?.email;
  if (!email) return <Navigate to="/signin" replace />;
  return <CheckEmail email={email} />;
}

function CheckEmail({ email }: { email: string }) {
  const [sent, setSent] = useState(false);
  const provider = mailProviderFor(email);

  return (
    <AuthScreen
      title="Check your email"
      text="We sent a verification link to this address. Open it to confirm it is yours."
    >
      <div className={styles.tile} data-sent={sent} aria-hidden="true">
        {/* Keyed, so the icon springs in again when the mail becomes a tick. */}
        <Icon
          key={sent ? "sent" : "mail"}
          name={sent ? "check" : "mail"}
          size={24}
        />
      </div>
      <p className={styles.address}>{email}</p>
      {provider && (
        <a
          href={provider.url}
          target="_blank"
          rel="noopener noreferrer"
          className={buttonClass({ size: "lg", block: true })}
        >
          Open {provider.name}
          <Icon name="arrow-up" size={16} className={styles.out} />
        </a>
      )}
      <ResendButton
        send={() => sendVerificationLink(email)}
        variant={provider ? "secondary" : "primary"}
        onSent={() => setSent(true)}
      />
      <LinkButton to="/signin" variant="ghost" size="lg" pill block>
        Skip for now
        <Icon name="arrow-right" size={16} />
      </LinkButton>
      <p role="status" className={form.note}>
        {sent
          ? "If that email is registered and unverified, a verification link is on its way."
          : "You can sign in now. We keep a reminder at the top of the app until you confirm."}
      </p>
    </AuthScreen>
  );
}
