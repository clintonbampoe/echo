import { DeleteDialog, useToast } from "@/ui";
import type { Member } from "../../api/members";
import { useDeleteMember } from "../../hooks";
import { pronouns } from "../../model/display";

// onDeleted runs after a successful delete (the profile page uses it to go back to the list).
type Props = { member: Member; onClose: () => void; onDeleted?: () => void };

// "Delete" takes the member out of lists, totals and search; the server keeps their history.
export function DeleteMemberDialog({ member, onClose, onDeleted }: Props) {
  const remove = useDeleteMember();
  const toast = useToast();
  const they = pronouns(member.gender);
  return (
    <DeleteDialog
      title={`Delete ${member.name}?`}
      noun="member"
      confirmLabel="Delete"
      cancelLabel="Cancel"
      onDelete={() => remove.mutateAsync(member.id)}
      onClose={onClose}
      onDeleted={() => {
        toast({ message: "Member deleted" });
        onDeleted?.();
      }}
    >
      <p>
        {they.subject} {they.leaves} your lists and totals. {they.possessive}{" "}
        tithes and attendance are kept for your records.
      </p>
      <p>
        To keep {member.firstName} on your lists, set the status to Transferred
        or Archived instead.
      </p>
    </DeleteDialog>
  );
}
