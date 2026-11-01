import { useEffect, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { LinkButton } from "@/ui";
import { BackLink } from "../components/AuthLinks";
import { LinkExpired } from "../components/LinkExpired";
import { AuthScreen } from "../components/AuthScreen";
import { useIsSignedIn } from "../hooks/session";
import { useOtherTab } from "../hooks/verified";
import { recallOrigin } from "../model/origin";
import { VerifyDone } from "./VerifyDone";
import { useVerifyEmailLink } from "../hooks/queries";
import styles from "./form.module.css";

// Where the link in the verification email lands: /verify-email?token=...
export function VerifyEmailPage() {
  const token = useSearchParams()[0].get("token") ?? "";
  const check = useVerifyEmailLink(token);
  const signedIn = useIsSignedIn();
  // Tell a verify page waiting in another tab, so it can celebrate and carry on. If one answers, this tab
  // has nothing left to do; if not, this tab takes the person back to where they started.
  const other = useOtherTab(check.isSuccess);
  const [origin] = useState(recallOrigin);
  useEffect(() => {
    if (other) window.close();
  }, [other]);

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
        title="Account verified"
        text={
          other
            ? "Your other tab already knows. You can close this one."
            : "Thank you. Your account is verified."
        }
      >
        {other === false && (
          <VerifyDone signedIn={signedIn} leave={origin ?? "/dashboard"} />
        )}
        {other && (
          <LinkButton
            to={origin ?? "/dashboard"}
            variant="ghost"
            size="lg"
            pill
            block
          >
            Continue here instead
          </LinkButton>
        )}
      </AuthScreen>
    );
  }
  return (
    <AuthScreen title="Verifying your account" text="This only takes a moment.">
      <p className={styles.note}>Checking your link…</p>
    </AuthScreen>
  );
}
