import { useState } from "react";
import { ConfirmDialog, DeleteDialog } from "@/ui";
import type { Session } from "../../api/sessions";
import { dayMonth, sessionTitle } from "../../model/day";
import { useDeleteSession, useUpdateSession } from "../../hooks/mutations";

type Props = { session: Session; onClose: () => void };

const people = (n: number) =>
  `${n.toLocaleString("en-GB")} ${n === 1 ? "person" : "people"}`;

// Cancelling or completing a session changes only its status, so the one dialog serves both.
export function StatusDialog({
  session,
  status,
  onClose,
}: Props & { status: "Completed" | "Cancelled" }) {
  const update = useUpdateSession();
  const [failed, setFailed] = useState(false);
  const title = sessionTitle(session);
  const cancelling = status === "Cancelled";

  async function confirm() {
    setFailed(false);
    try {
      await update.mutateAsync({ id: session.id, status });
      onClose();
    } catch {
      setFailed(true);
    }
  }

  return (
    <ConfirmDialog
      title={
        cancelling ? "Cancel this session?" : `Mark ${title} as completed?`
      }
      confirmLabel={cancelling ? "Cancel session" : "Mark completed"}
      cancelLabel={cancelling ? "Keep session" : "Cancel"}
      busy={update.isPending}
      error={failed ? "We could not save this change. Try again." : undefined}
      onConfirm={() => void confirm()}
      onCancel={onClose}
    >
      {cancelling ? (
        <>
          <p>
            {title} on {dayMonth(session.date)} will be marked Cancelled. Use
            this when the gathering did not take place.
          </p>
          <p>
            Cancelled sessions stay in your history and are left out of
            attendance averages.
          </p>
        </>
      ) : (
        <p>
          {people(session.attendanceCount)} marked present will count toward
          your attendance averages. You can still add or remove people
          afterwards.
        </p>
      )}
    </ConfirmDialog>
  );
}

// "Delete" takes the session out of the lists. A gathering that did not happen is better marked Cancelled.
export function DeleteSessionDialog({
  session,
  onClose,
  onDeleted,
}: Props & { onDeleted: () => void }) {
  const remove = useDeleteSession();
  return (
    <DeleteDialog
      title={`Delete ${sessionTitle(session)}, ${dayMonth(session.date)}?`}
      noun="session"
      onDelete={() => remove.mutateAsync(session.id)}
      onClose={onClose}
      onDeleted={onDeleted}
    >
      <p>
        The session will be removed from your lists and its{" "}
        {session.attendanceCount.toLocaleString("en-GB")} attendance{" "}
        {session.attendanceCount === 1 ? "record" : "records"} will no longer
        show.
      </p>
      <p>
        If the service did not happen, mark it as cancelled instead. That keeps
        it in your history.
      </p>
    </DeleteDialog>
  );
}
