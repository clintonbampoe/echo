import { useNavigate } from "react-router-dom";
import type { ApiError } from "@/lib/api";
import { Alert } from "@/ui";
import { useSendVerification } from "../hooks";
import styles from "./SignInAlert.module.css";

type Props = { problem: ApiError; email: string };

// Says what went wrong with a sign-in in words a person can act on. For a wrong password it
// never says which of the two was wrong: the API deliberately does not tell us.
export function SignInAlert({ problem, email }: Props) {
  const navigate = useNavigate();
  const resend = useSendVerification();

  if (problem.errorCode === "EMAIL_NOT_VERIFIED") {
    const onResend = async () => {
      await resend.mutateAsync(email).catch(() => undefined);
      navigate("/verify", { state: { email } });
    };
    return (
      <Alert
        title="Verify your email to sign in"
        action={
          <button
            type="button"
            className={styles.link}
            onClick={onResend}
            disabled={resend.isPending}
          >
            Resend verification email
          </button>
        }
      >
        We sent a link to {email} when you registered.
      </Alert>
    );
  }
  if (problem.errorCode === "INVALID_CREDENTIALS") {
    return (
      <Alert title="Email or password is incorrect">
        Check them and try again.
      </Alert>
    );
  }
  if (problem.status === 429) {
    return (
      <Alert title="Too many attempts">Wait a minute, then try again.</Alert>
    );
  }
  return <Alert title="We could not sign you in">{problem.message}</Alert>;
}
