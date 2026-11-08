import { useEffect, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { Button, LinkButton } from "@/ui";
import { BackLink } from "../components/AuthLinks";
import { LinkExpired } from "../components/LinkExpired";
import { AuthScreen } from "../components/AuthScreen";
import { useIsSignedIn } from "../hooks/session";
import { useOtherTab } from "../hooks/verified";
import { recallOrigin } from "../model/origin";
import { VerifyDone } from "./VerifyDone";
import { useVerifyEmailLink } from "../hooks/queries";
import styles from "./form.module.css";

// Where the link in the verification email lands: /verify-email?token=... Landing here changes nothing; the
// person taps to confirm, because a scanner that opens the link would otherwise spend the single-use token.
export function VerifyEmailPage() {
  const token = useSearchParams()[0].get("token") ?? "";
  const [confirmed, setConfirmed] = useState(false);
  const check = useVerifyEmailLink(token, confirmed);
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
      <Verified
        other={other}
        signedIn={signedIn}
        leave={origin ?? "/dashboard"}
      />
    );
  }
  if (!confirmed) {
    return (
      <AuthScreen
        title="Confirm your email"
        text="One tap and your account is verified."
      >
        <Button size="lg" pill block onClick={() => setConfirmed(true)}>
          Confirm my email
        </Button>
      </AuthScreen>
    );
  }
  return (
    <AuthScreen title="Verifying your account" text="This only takes a moment.">
      <p className={styles.note}>Checking your link…</p>
    </AuthScreen>
  );
}

function Verified({
  other,
  signedIn,
  leave,
}: {
  other: boolean | undefined;
  signedIn: boolean;
  leave: string;
}) {
  return (
    <AuthScreen
      title="Account verified"
      text={
        other
          ? "Your other tab already knows. You can close this one."
          : "Thank you. Your account is verified."
      }
    >
      {other === false && <VerifyDone signedIn={signedIn} leave={leave} />}
      {other && (
        <LinkButton to={leave} variant="ghost" size="lg" pill block>
          Continue here instead
        </LinkButton>
      )}
    </AuthScreen>
  );
}
