import type { ReactNode } from "react";
import { LinkButton } from "@/ui";
import type { InvitationInfo } from "../api";
import { useInvitation } from "../hooks";
import { AuthScreen } from "./AuthScreen";
import styles from "../pages/form.module.css";

export function InvalidInvitation() {
  return (
    <AuthScreen
      title="This invitation is no longer valid"
      text="Invitation links expire, and each one works once. Ask your church administrator to send you a new one."
    >
      <LinkButton to="/signin" size="lg" pill block>
        Back to sign in
      </LinkButton>
    </AuthScreen>
  );
}

type Props = {
  code: string;
  children: (invitation: InvitationInfo) => ReactNode;
};

// Looks up the invitation for a code and shows the right thing while it loads or if the code
// is bad, so each join screen only deals with a good invitation.
export function InvitationGate({ code, children }: Props) {
  const invitation = useInvitation(code);
  if (invitation.isPending)
    return (
      <AuthScreen
        title="Checking your invitation"
        text="This only takes a moment."
      >
        <p className={styles.note}>Checking your invitation…</p>
      </AuthScreen>
    );
  if (invitation.isError) return <InvalidInvitation />;
  return <>{children(invitation.data)}</>;
}
