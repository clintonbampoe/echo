import { DeleteDialog, Presence, useHeld, useToast } from "@/ui";
import type { Invitation } from "../api/invitations";
import { useDeleteInvitation, useRevokeInvitation } from "../hooks";
import { titleOf } from "../model/display";
import { EditInvitationSheet } from "./EditInvitationSheet";

export type InvitationAction = {
  kind: "edit" | "revoke" | "delete";
  invitation: Invitation;
};

type Props = { action: InvitationAction | null; onClose: () => void };

// Shows the sheet or dialog for what someone asked to do to an invitation, and lets it play its exit.
export function InvitationActionHost({ action, onClose }: Props) {
  const held = useHeld(action);
  return (
    <>
      <Presence open={action?.kind === "edit"}>
        {held && (
          <EditInvitationSheet invitation={held.invitation} onClose={onClose} />
        )}
      </Presence>
      <Presence open={action?.kind === "revoke"} ms={160}>
        {held && (
          <RevokeDialog invitation={held.invitation} onClose={onClose} />
        )}
      </Presence>
      <Presence open={action?.kind === "delete"} ms={160}>
        {held && (
          <DeleteInvitation invitation={held.invitation} onClose={onClose} />
        )}
      </Presence>
    </>
  );
}

type DialogProps = { invitation: Invitation; onClose: () => void };

function RevokeDialog({ invitation, onClose }: DialogProps) {
  const revoke = useRevokeInvitation();
  const toast = useToast();
  return (
    <DeleteDialog
      title={`Switch off ${titleOf(invitation)}?`}
      noun="invitation"
      confirmLabel="Switch off"
      cancelLabel="Cancel"
      onDelete={() => revoke.mutateAsync(invitation.id)}
      onClose={onClose}
      onDeleted={() => toast({ message: "Invitation switched off" })}
    >
      <p>
        Nobody can join with it from now on. People who already joined keep
        their accounts, and the invitation stays in your list.
      </p>
    </DeleteDialog>
  );
}

function DeleteInvitation({ invitation, onClose }: DialogProps) {
  const remove = useDeleteInvitation();
  const toast = useToast();
  return (
    <DeleteDialog
      title={`Delete ${titleOf(invitation)}?`}
      noun="invitation"
      confirmLabel="Delete"
      cancelLabel="Cancel"
      onDelete={() => remove.mutateAsync(invitation.id)}
      onClose={onClose}
      onDeleted={() => toast({ message: "Invitation deleted" })}
    >
      <p>
        It disappears from your list and stops working. People who already
        joined keep their accounts.
      </p>
    </DeleteDialog>
  );
}
