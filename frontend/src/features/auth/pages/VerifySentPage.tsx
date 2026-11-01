import { useState } from "react";
import { Navigate, useLocation } from "react-router-dom";
import { buttonClass, Icon, LinkButton, StatusTile } from "@/ui";
import { ResendButton } from "../components/ResendButton";
import { AuthScreen } from "../components/AuthScreen";
import { sendVerificationLink } from "../api/auth";
import { useIsSignedIn } from "../hooks";
import { mailProviderFor } from "../model/mailProvider";
import form from "./form.module.css";
import styles from "./VerifySentPage.module.css";

type Arrival = { email?: string; signedIn?: boolean } | null;

// "Check your email." Registering makes a working account and signs the person in, so this is a pause rather
// than a gate: open the mail, ask for another link, or skip into the app. The address (and whether sign-in
// worked) arrives from the page that registered, in the navigation state. Opened directly there is nothing to
// say, so go on to the app, or to sign-in.
export function VerifySentPage() {
  const arrival = useLocation().state as Arrival;
  const signedIn = useIsSignedIn();
  const leave = signedIn ? "/dashboard" : "/signin";
  if (!arrival?.email) return <Navigate to={leave} replace />;
  return <CheckEmail email={arrival.email} leave={leave} />;
}

function CheckEmail({ email, leave }: { email: string; leave: string }) {
  const [sent, setSent] = useState(false);
  const provider = mailProviderFor(email);

  return (
    <AuthScreen
      title="Check your email"
      text="We sent a verification link to this address. Open it to confirm it is yours."
    >
      <StatusTile icon={sent ? "check" : "mail"} on={sent} />
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
        variant={provider ? "link" : "primary"}
        onSent={() => setSent(true)}
      />
      <div className={styles.skip}>
        <LinkButton to={leave} variant="ghost" size="lg" pill>
          Skip for now
          <Icon name="arrow-right" size={16} />
        </LinkButton>
        <p role="status" className={form.note}>
          {sent
            ? "If that email is registered and unverified, a verification link is on its way."
            : leave === "/dashboard"
              ? "You are signed in. We keep a reminder at the top of the app until you confirm."
              : "You can sign in now. We keep a reminder at the top of the app until you confirm."}
        </p>
      </div>
    </AuthScreen>
  );
}
