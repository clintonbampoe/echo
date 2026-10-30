import { Navigate, useLocation } from "react-router-dom";
import { LinkButton } from "@/ui";
import { ResendButton } from "../components/ResendButton";
import { AuthScreen } from "../components/AuthScreen";
import { sendVerificationLink } from "../api/auth";
import styles from "./form.module.css";

// "Your account is ready." Registering makes a working account, so this points to sign-in and
// mentions the verification link as a follow-up. The address arrives from the page that registered,
// passed along in the navigation state. Opened directly there is nothing to say, so go to sign-in.
export function VerifySentPage() {
  const email = (useLocation().state as { email?: string } | null)?.email;
  if (!email) return <Navigate to="/signin" replace />;

  return (
    <AuthScreen
      title="Your account is ready"
      text={
        <>
          You can sign in now. We also sent a link to <strong>{email}</strong>{" "}
          so we can confirm it is yours.
        </>
      }
    >
      <LinkButton to="/signin" size="lg" pill block>
        Continue to sign in
      </LinkButton>
      <ResendButton send={() => sendVerificationLink(email)} />
      <p className={styles.note}>
        No email? Check spam. You can ask for another link from inside the app
        at any time.
      </p>
    </AuthScreen>
  );
}
