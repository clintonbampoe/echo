import { useState } from "react";
import { useNavigate, useParams, useSearchParams } from "react-router-dom";
import { useLastSeen, PersonVisits, RecentVisits } from "@/features/attendance";
import { isApiError } from "@/lib/api";
import { todayText } from "@/lib/dates";
import { paths } from "@/lib/paths";
import {
  ActionMenu,
  Alert,
  BackLink,
  Button,
  Chip,
  LinkButton,
  ProfileHeader,
  StatStrip,
  Tabs,
} from "@/ui";
import type { Visitor } from "./api";
import { ConvertedBanner } from "./ConvertedBanner";
import { ConvertSheet } from "./ConvertSheet";
import { DeleteVisitorDialog } from "./DeleteVisitorDialog";
import { EditVisitorSheet } from "./EditVisitorSheet";
import { useVisitor } from "./hooks";
import { becameStat, daysSinceStat, firstVisitStat, visitsStat } from "./stat";
import { memberIdOf } from "./view";
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
  const [params, setParams] = useSearchParams();
  const tab = params.get("tab") === "visits" ? "visits" : "details";
  const [editing, setEditing] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [converting, setConverting] = useState(false);
  const today = todayText();
  const lastVisit = useLastSeen(v.id, "Last visit");
  // Someone who has become a member is edited and removed under Members, not here.
  const memberId = memberIdOf(v);

  return (
    <div className="anim-rise">
      <BackLink to={paths.visitors}>Visitors</BackLink>
      <ConvertedBanner v={v} />
      <ProfileHeader
        label="Visitor"
        name={v.name}
        line={[v.phoneNumber, v.emailAddress].filter(Boolean).join(" · ")}
        badges={memberId ? <Chip tone="positive">Member now</Chip> : undefined}
      >
        {memberId ? (
          <LinkButton to={paths.member(memberId)}>View member</LinkButton>
        ) : (
          <>
            <Button onClick={() => setConverting(true)}>Make a member</Button>
            <ActionMenu
              label={`More actions for ${v.name}`}
              title={v.name}
              items={[
                { label: "Edit", onSelect: () => setEditing(true) },
                {
                  label: "Delete",
                  danger: true,
                  separated: true,
                  onSelect: () => setDeleting(true),
                },
              ]}
            />
          </>
        )}
      </ProfileHeader>
      <StatStrip
        label="Visitor numbers"
        stats={[
          visitsStat(v),
          firstVisitStat(v),
          lastVisit,
          memberId ? becameStat(v) : daysSinceStat(v, today),
        ]}
      />
      <Tabs
        label="Visitor history"
        tabs={[
          { id: "details", label: "Details" },
          { id: "visits", label: "Visits" },
        ]}
        value={tab}
        onChange={(t) => setParams({ tab: t }, { replace: true })}
        idPrefix="visitor"
      />
      <div
        key={tab}
        className="anim-rise"
        role="tabpanel"
        id={`visitor-panel-${tab}`}
        aria-labelledby={`visitor-tab-${tab}`}
      >
        {tab === "details" ? (
          <div className={styles.two}>
            <VisitorDetails v={v} today={today} />
            <RecentVisits personId={v.id} />
          </div>
        ) : (
          <PersonVisits personId={v.id} name={v.name} />
        )}
      </div>

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
    </div>
  );
}
