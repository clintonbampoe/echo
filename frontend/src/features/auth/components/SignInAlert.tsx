import type { ApiError } from "@/lib/api";
import { Alert } from "@/ui";

type Props = { problem: ApiError };

// Says what went wrong with a sign-in in words a person can act on. For a wrong password it
// never says which of the two was wrong: the API deliberately does not tell us.
export function SignInAlert({ problem }: Props) {
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
