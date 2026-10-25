import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { isApiError } from "@/lib/api";
import { paths } from "@/lib/paths";
import { Alert, BackLink, Button, StatStrip } from "@/ui";
import type { Session } from "../api/sessions";
import { presenceStats } from "../model/day";
import {
  useAttendanceSummary,
  useCheckIns,
  useSession,
} from "../hooks/queries";
import { CheckInPanel } from "../components/checkin/CheckInPanel";
import { EditSessionSheet } from "../components/form/EditSessionSheet";
import {
  DeleteSessionDialog,
  StatusDialog,
} from "../components/dialogs/SessionDialogs";
import { MarkSheet } from "../components/checkin/MarkSheet";
import { SessionCheckIns } from "../components/session/SessionCheckIns";
import { SessionHeader } from "../components/session/SessionHeader";
import styles from "./SessionPage.module.css";

// One session: what it is, the numbers, who is marked present, and the panel for marking people as they arrive.
export function SessionPage() {
  const { id = "" } = useParams();
  const session = useSession(id);

  if (session.isPending)
    return (
      <p role="status" className={styles.note}>
        Loading session…
      </p>
    );
  if (session.isError) {
    const missing = isApiError(session.error) && session.error.status === 404;
    return (
      <div className={styles.note}>
        <BackLink to={paths.attendance}>Attendance</BackLink>
        <Alert
          title={
            missing
              ? "We could not find this session"
              : "We could not load this session"
          }
        >
          {missing
            ? "It may have been deleted."
            : "Check your connection and try again."}
        </Alert>
        {!missing && (
          <Button variant="secondary" onClick={() => void session.refetch()}>
            Try again
          </Button>
        )}
      </div>
    );
  }
  return <Loaded session={session.data} />;
}

type Dialog = "edit" | "delete" | "complete" | "cancel" | "mark" | null;

function Loaded({ session }: { session: Session }) {
  const navigate = useNavigate();
  const [dialog, setDialog] = useState<Dialog>(null);
  const summary = useAttendanceSummary({ sessionId: session.id });
  // Unfiltered, so the panel knows everyone already marked whatever the list below is searched for.
  const everyone = useCheckIns(session.id);
  const close = () => setDialog(null);

  return (
    <div className="anim-rise">
      <BackLink to={paths.attendanceDay(session.date)}>Attendance</BackLink>
      <SessionHeader
        session={session}
        onMark={() => setDialog("mark")}
        onEdit={() => setDialog("edit")}
        onDelete={() => setDialog("delete")}
        onComplete={() => setDialog("complete")}
        onCancel={() => setDialog("cancel")}
      />
      <div className={styles.layout}>
        <div className={styles.stats}>
          <StatStrip
            label="Attendance for this session"
            stats={presenceStats(
              summary.data,
              "Marked for this session",
              "Not started",
            )}
            busy={summary.isPending}
          />
        </div>
        <div className={styles.panel}>
          <CheckInPanel session={session} rows={everyone.rows} />
        </div>
        <div className={styles.list}>
          <SessionCheckIns session={session} />
        </div>
      </div>
      {dialog === "edit" && (
        <EditSessionSheet session={session} onClose={close} />
      )}
      {dialog === "mark" && (
        <MarkSheet session={session} rows={everyone.rows} onClose={close} />
      )}
      {dialog === "complete" && (
        <StatusDialog session={session} status="Completed" onClose={close} />
      )}
      {dialog === "cancel" && (
        <StatusDialog session={session} status="Cancelled" onClose={close} />
      )}
      {dialog === "delete" && (
        <DeleteSessionDialog
          session={session}
          onClose={close}
          onDeleted={() =>
            navigate(paths.attendanceDay(session.date), { replace: true })
          }
        />
      )}
    </div>
  );
}
