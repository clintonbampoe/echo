import { useEffect } from "react";
import { Navigate, useLocation } from "react-router-dom";
import { Icon } from "@/ui";
import { AuthLinks, BackLink } from "../components/AuthLinks";
import { AuthScreen } from "../components/AuthScreen";
import { useIsSignedIn, useVerifyFlow } from "../hooks";
import { mailProviderFor } from "../model/mailProvider";
import { rememberOrigin, safePath } from "../model/origin";
import { VerifyDone } from "./VerifyDone";
import { VerifyMark } from "./VerifyMark";
import { VerifyWaiting } from "./VerifyWaiting";
import styles from "./VerifySentPage.module.css";

type Arrival = { email?: string; send?: boolean; from?: string } | null;

// "Check your email", then "Account verified". Two things bring people here: signing up (the link already
// went out) and the reminder in the app (this page sends it). Either way the page waits, notices by itself
// when the link has been opened (here, in another tab or on another device) and says so. The address is
// the account's own, shown but never editable. Opened directly there is nothing to say, so go on.
export function VerifySentPage() {
  const arrival = useLocation().state as Arrival;
  const signedIn = useIsSignedIn();
  const from = safePath(arrival?.from);
  // Back to the page they asked from, so verifying never loses their place.
  const leave = signedIn ? (from ?? "/dashboard") : "/signin";
  if (!arrival?.email) return <Navigate to={leave} replace />;
  return (
    <CheckEmail
      email={arrival.email}
      autoSend={arrival.send === true}
      signedIn={signedIn}
      leave={leave}
      from={from}
    />
  );
}

type Props = {
  from: string | undefined;
  email: string;
  autoSend: boolean;
  signedIn: boolean;
  leave: string;
};

function CheckEmail({ email, autoSend, signedIn, leave, from }: Props) {
  // The email link opens its own tab, which needs to know where to take them.
  useEffect(() => rememberOrigin(from), [from]);
  const { phase, wait, send } = useVerifyFlow(email, autoSend, signedIn);
  const verified = phase === "verified";
  return (
    <AuthScreen
      title={verified ? "Account verified" : "Check your email"}
      text={
        verified
          ? "Thank you. Your account is verified and the reminder is gone."
          : phase === "sending"
            ? "We are sending a link to verify your account."
            : "We sent a link to verify your account. Open it on any device and this page updates by itself."
      }
      links={
        signedIn ? (
          <BackLink to={leave} heading="Not now?">
            Back to Echo
          </BackLink>
        ) : (
          <AuthLinks
            heading="Wrong address?"
            items={[
              { to: "/signup", label: "Use a different email" },
              { to: "/signin", label: "Back to sign in" },
            ]}
          />
        )
      }
    >
      <VerifyMark phase={phase} />
      <div className={styles.field}>
        <p className={styles.caption}>Sent to the email you signed up with</p>
        <p className={styles.address}>
          <Icon name="lock" size={14} className={styles.lock} />
          <span>{email}</span>
          {verified && (
            <span className={styles.badge}>
              <Icon name="check" size={13} /> Verified
            </span>
          )}
        </p>
      </div>
      <p className={styles.sr} role="status">
        {verified ? "Account verified" : ""}
      </p>
      {verified ? (
        <VerifyDone signedIn={signedIn} leave={leave} />
      ) : (
        <VerifyWaiting
          phase={phase}
          provider={mailProviderFor(email) ?? undefined}
          wait={wait}
          signedIn={signedIn}
          leave={leave}
          onSend={() => void send()}
        />
      )}
    </AuthScreen>
  );
}
