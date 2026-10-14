import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { isApiError } from "@/lib/api";
import { todayText } from "@/lib/dates";
import { paths } from "@/lib/paths";
import {
  Alert,
  BackLink,
  Button,
  Chip,
  Icon,
  ProfileIdentity,
  ProfileLayout,
  useNarrowProfile,
} from "@/ui";
import type { Member } from "./api";
import { memberFor } from "./dates";
import { DeleteMemberDialog } from "./DeleteMemberDialog";
import { EditMemberSheet } from "./EditMemberSheet";
import { useMember } from "./hooks";
import { statusTone } from "./labels";
import { MemberDetails } from "./MemberDetails";
import { ProfileTabs } from "./profile/ProfileTabs";
import { StatCards } from "./profile/StatCards";
import styles from "./MemberPage.module.css";

// One member, all their details. The Edit and Delete buttons reuse the sheet and dialog from the list.
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
  const narrow = useNarrowProfile();
  const [editing, setEditing] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const today = todayText();
  const details = <MemberDetails m={m} today={today} />;
  const tenure = memberFor(m.joinedDate, today);

  return (
    <>
      <ProfileLayout
        back={{ to: paths.members, label: "Members" }}
        narrow={narrow}
        aside={details}
        identity={
          <ProfileIdentity
            label="Member"
            name={m.name}
            meta={
              <>
                <Chip tone={statusTone[m.status]}>{m.status}</Chip>
                {tenure && <span>{tenure}</span>}
              </>
            }
          >
            <Button variant="secondary" onClick={() => setEditing(true)}>
              <Icon name="user" size={16} />
              Edit
            </Button>
            <Button
              variant="secondary"
              className={styles.danger}
              onClick={() => setDeleting(true)}
            >
              Delete
            </Button>
          </ProfileIdentity>
        }
      >
        <StatCards id={m.id} today={today} />
        <ProfileTabs
          id={m.id}
          today={today}
          details={narrow ? details : undefined}
        />
      </ProfileLayout>

      {editing && (
        <EditMemberSheet member={m} onClose={() => setEditing(false)} />
      )}
      {deleting && (
        <DeleteMemberDialog
          member={m}
          onClose={() => setDeleting(false)}
          onDeleted={() => navigate(paths.members, { replace: true })}
        />
      )}
    </>
  );
}
