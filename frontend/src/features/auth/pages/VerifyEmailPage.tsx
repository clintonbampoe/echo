import { useSearchParams } from "react-router-dom";
import { LinkButton } from "@/ui";
import { BackLink } from "../components/AuthLinks";
import { LinkExpired } from "../components/LinkExpired";
import { AuthScreen } from "../components/AuthScreen";
import { useVerifyEmailLink } from "../hooks/queries";
import styles from "./form.module.css";

// Where the link in the verification email lands: /verify-email?token=...
export function VerifyEmailPage() {
  const token = useSearchParams()[0].get("token") ?? "";
  const check = useVerifyEmailLink(token);

  if (token === "" || check.isError) {
    return (
      <LinkExpired
        title="This link has expired"
        text="Verification links expire after 24 hours. We can send you a fresh one."
        links={<BackLink />}
      />
    );
  }
  if (check.isSuccess) {
    return (
      <AuthScreen
        title="Email verified"
        text="Your account is active. Sign in to set up your church."
      >
        <LinkButton to="/signin" size="lg" pill block>
          Continue to sign in
        </LinkButton>
      </AuthScreen>
    );
  }
  return (
    <AuthScreen title="Verifying your email" text="This only takes a moment.">
      <p className={styles.note}>Checking your link…</p>
    </AuthScreen>
  );
}
