import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useLastSeen, PersonVisits } from "@/features/attendance";
import { isApiError } from "@/lib/api";
import { todayText } from "@/lib/dates";
import { paths } from "@/lib/paths";
import {
  Alert,
  BackLink,
  Button,
  Chip,
  Icon,
  LinkButton,
  ProfileIdentity,
  ProfileLayout,
  StatStrip,
  useNarrowProfile,
} from "@/ui";
import type { Visitor } from "./api";
import { ConvertedBanner } from "./ConvertedBanner";
import { ConvertSheet } from "./ConvertSheet";
import { DeleteVisitorDialog } from "./DeleteVisitorDialog";
import { EditVisitorSheet } from "./EditVisitorSheet";
import { useVisitor } from "./hooks";
import { kindTone } from "./labels";
import { addedStat, becameStat, daysSinceStat, visitsStat } from "./stat";
import { memberIdOf, visitorKind } from "./view";
import { VisitorDetails } from "./VisitorDetails";
import styles from "./VisitorPage.module.css";

// One visitor: who they are, how often they have come, and every session they were marked present at.
export function VisitorPage() {
  const { id = "" } = useParams();
  const visitor = useVisitor(id);

  if (visitor.isPending)
    return (
      <p role="status" className={styles.note}>
        Loading visitor…
      </p>
    );
  if (visitor.isError) {
    const missing = isApiError(visitor.error) && visitor.error.status === 404;
    return (
      <div className={styles.note}>
        <BackLink to={paths.visitors}>Visitors</BackLink>
        <Alert
          title={
            missing
              ? "We could not find this visitor"
              : "We could not load this visitor"
          }
        >
          {missing
            ? "They may have been deleted."
            : "Check your connection and try again."}
        </Alert>
        {!missing && (
          <Button variant="secondary" onClick={() => void visitor.refetch()}>
            Try again
          </Button>
        )}
      </div>
    );
  }
  return <Profile visitor={visitor.data} />;
}

function Profile({ visitor: v }: { visitor: Visitor }) {
  const navigate = useNavigate();
  const narrow = useNarrowProfile();
  const [editing, setEditing] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [converting, setConverting] = useState(false);
  const today = todayText();
  const lastVisit = useLastSeen(v.id, "Last visit");
  // Someone who has become a member is edited and removed under Members, not here.
  const memberId = memberIdOf(v);
  const details = <VisitorDetails v={v} today={today} />;

  return (
    <>
      <ProfileLayout
        back={{ to: paths.visitors, label: "Visitors" }}
        narrow={narrow}
        aside={details}
        notice={<ConvertedBanner v={v} />}
        identity={
          <ProfileIdentity
            label="Visitor"
            name={v.name}
            meta={
              memberId ? (
                <Chip tone="positive">Member now</Chip>
              ) : (
                <Chip tone={kindTone[visitorKind(v)]}>{visitorKind(v)}</Chip>
              )
            }
          >
            {memberId ? (
              <LinkButton to={paths.member(memberId)}>View member</LinkButton>
            ) : (
              <>
                <Button
                  className={styles.make}
                  onClick={() => setConverting(true)}
                >
                  Make member
                </Button>
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
              </>
            )}
          </ProfileIdentity>
        }
      >
        <StatStrip
          label="Visitor numbers"
          stats={[
            visitsStat(v),
            addedStat(v),
            lastVisit,
            memberId ? becameStat(v) : daysSinceStat(v, today),
          ]}
        />
        {narrow && details}
        <PersonVisits personId={v.id} name={v.name} />
      </ProfileLayout>

      {converting && (
        <ConvertSheet visitor={v} onClose={() => setConverting(false)} />
      )}
      {editing && (
        <EditVisitorSheet visitor={v} onClose={() => setEditing(false)} />
      )}
      {deleting && (
        <DeleteVisitorDialog
          visitor={v}
          onClose={() => setDeleting(false)}
          onDeleted={() => navigate(paths.visitors, { replace: true })}
        />
      )}
    </>
  );
}
