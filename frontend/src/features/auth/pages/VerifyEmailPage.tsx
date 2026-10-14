import { useSearchParams } from "react-router-dom";
import { LinkButton } from "@/ui";
import { BackLink } from "../components/AuthLinks";
import { LinkExpired } from "../components/LinkExpired";
import { StatusScreen } from "../components/StatusScreen";
import { useVerifyEmailLink } from "../hooks";

// Where the link in the verification email lands: /verify-email?token=...
export function VerifyEmailPage() {
  const token = useSearchParams()[0].get("token") ?? "";
  const check = useVerifyEmailLink(token);

  if (token === "" || check.isError) {
    return (
      <LinkExpired
        title="This link has expired"
        text="Verification links expire after 24 hours. We can send you a fresh one."
      >
        <BackLink />
      </LinkExpired>
    );
  }
  if (check.isSuccess) {
    return (
      <StatusScreen
        icon="check"
        title="Email verified"
        text="Your account is active. Sign in to set up your church."
      >
        <LinkButton to="/signin" size="lg" block>
          Continue to sign in
        </LinkButton>
      </StatusScreen>
    );
  }
  return (
    <StatusScreen
      icon="spinner"
      title="Verifying your email"
      text="This only takes a moment."
    />
  );
}
