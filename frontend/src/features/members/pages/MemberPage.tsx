import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { isApiError } from "@/lib/api";
import { todayText } from "@/lib/dates";
import { paths } from "@/lib/paths";
import { Alert, BackLink, Button } from "@/ui";
import type { Member } from "../api/members";
import {
  MemberActionHost,
  type MemberAction,
} from "../components/dialogs/MemberActionHost";
import { MemberDetails } from "../components/profile/MemberDetails";
import { MemberHeader } from "../components/profile/MemberHeader";
import { ProfileFigures } from "../components/profile/ProfileFigures";
import { ProfileTabs } from "../components/profile/ProfileTabs";
import { useMember } from "../hooks";
import styles from "./MemberPage.module.css";

// One member, all their details. Edit and Delete reuse the sheet and dialog from the list.
export function MemberPage() {
  const { id = "" } = useParams();
  const member = useMember(id);

  if (member.isPending)
    return (
      <p role="status" className={styles.note}>
        Loading member…
      </p>
    );
  if (member.isError) {
    const missing = isApiError(member.error) && member.error.status === 404;
    return (
      <div className={styles.note}>
        <BackLink to={paths.members}>Members</BackLink>
        <Alert
          title={
            missing
              ? "We could not find this member"
              : "We could not load this member"
          }
        >
          {missing
            ? "They may have been deleted."
            : "Check your connection and try again."}
        </Alert>
        {!missing && (
          <Button variant="secondary" onClick={() => void member.refetch()}>
            Try again
          </Button>
        )}
      </div>
    );
  }
  return <Profile member={member.data} />;
}

function Profile({ member: m }: { member: Member }) {
  const navigate = useNavigate();
  const [action, setAction] = useState<MemberAction | null>(null);
  const today = todayText();

  return (
    <div className="anim-rise">
      <BackLink to={paths.members}>Members</BackLink>
      <MemberHeader
        member={m}
        onEdit={() => setAction({ kind: "edit", member: m })}
        onDelete={() => setAction({ kind: "delete", member: m })}
      />
      <ProfileFigures id={m.id} today={today} />
      <ProfileTabs
        id={m.id}
        today={today}
        details={<MemberDetails m={m} today={today} />}
      />
      <MemberActionHost
        action={action}
        onClose={() => setAction(null)}
        onDeleted={() => navigate(paths.members, { replace: true })}
      />
    </div>
  );
}
