import { useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { isApiError } from "@/lib/api";
import { todayText } from "@/lib/dates";
import { Alert, Avatar, Button, Chip, Icon, useMedia } from "@/ui";
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

// Below this width the details move from under the name into their own tab.
const narrowScreen = "(max-width: 1180px)";

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
        <Back />
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

function Back() {
  return (
    <Link to="/members" className={styles.back}>
      <Icon name="arrow-right" size={16} className={styles.flip} />
      Members
    </Link>
  );
}

function Profile({ member: m }: { member: Member }) {
  const navigate = useNavigate();
  const narrow = useMedia(narrowScreen);
  const [editing, setEditing] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const today = todayText();
  const details = <MemberDetails m={m} today={today} />;

  return (
    <div className="anim-rise">
      <Back />
      <div className={narrow ? styles.stack : styles.layout}>
        <div className={narrow ? undefined : styles.joined}>
          <Identity
            member={m}
            onEdit={() => setEditing(true)}
            onDelete={() => setDeleting(true)}
          />
          {!narrow && <div>{details}</div>}
        </div>
        <div className={styles.main}>
          <StatCards id={m.id} today={today} />
          <ProfileTabs
            id={m.id}
            today={today}
            details={narrow ? details : undefined}
          />
        </div>
      </div>

      {editing && (
        <EditMemberSheet member={m} onClose={() => setEditing(false)} />
      )}
      {deleting && (
        <DeleteMemberDialog
          member={m}
          onClose={() => setDeleting(false)}
          onDeleted={() => navigate("/members", { replace: true })}
        />
      )}
    </div>
  );
}

type IdentityProps = {
  member: Member;
  onEdit: () => void;
  onDelete: () => void;
};

// The name, status and the two things you can do to a member.
function Identity({ member: m, onEdit, onDelete }: IdentityProps) {
  const tenure = memberFor(m.joinedDate, todayText());
  return (
    <section className={styles.identity} aria-label="Member">
      <div className={styles.header}>
        <Avatar name={m.name} size={64} />
        <div className={styles.who}>
          <h1 className={styles.name}>{m.name}</h1>
          <p className={styles.meta}>
            <Chip tone={statusTone[m.status]}>{m.status}</Chip>
            {tenure && <span>{tenure}</span>}
          </p>
        </div>
      </div>
      <div className={styles.actions}>
        <Button variant="secondary" onClick={onEdit}>
          <Icon name="user" size={16} />
          Edit
        </Button>
        <Button
          variant="secondary"
          className={styles.danger}
          onClick={onDelete}
        >
          Delete
        </Button>
      </div>
    </section>
  );
}
