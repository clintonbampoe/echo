import { Navigate, useLocation } from "react-router-dom";
import { BackLink } from "../components/AuthLinks";
import { ResendButton } from "../components/ResendButton";
import { AuthScreen } from "../components/AuthScreen";
import { sendVerificationLink } from "../api/auth";
import styles from "./form.module.css";

// "We sent you a link." The address arrives from the page that registered (or signed in),
// passed along in the navigation state. Opened directly there is nothing to say, so go to sign-in.
export function VerifySentPage() {
  const email = (useLocation().state as { email?: string } | null)?.email;
  if (!email) return <Navigate to="/signin" replace />;

  return (
    <AuthScreen
      title="Verify your email"
      links={<BackLink />}
      text={
        <>
          We sent a link to <strong>{email}</strong>. Open it to activate your
          account.
        </>
      }
    >
      <ResendButton send={() => sendVerificationLink(email)} />
      <p className={styles.note}>
        Nothing in your inbox? Check spam, or contact your church administrator
        if the address is wrong.
      </p>
    </AuthScreen>
  );
}
