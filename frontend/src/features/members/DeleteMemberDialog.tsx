import { DeleteDialog } from "@/ui";
import type { Member } from "./api";
import { useDeleteMember } from "./hooks";

// onDeleted runs after a successful delete (the profile page uses it to go back to the list).
type Props = { member: Member; onClose: () => void; onDeleted?: () => void };

// "Delete" removes the member from the list and search; the server keeps their history.
export function DeleteMemberDialog({ member, onClose, onDeleted }: Props) {
  const remove = useDeleteMember();
  return (
    <DeleteDialog
      title={`Delete ${member.name}?`}
      noun="member"
      onDelete={() => remove.mutateAsync(member.id)}
      onClose={onClose}
      onDeleted={onDeleted}
    >
      <p>
        {member.firstName} will be removed from your member list and search.
        Attendance, tithe and event history is kept.
      </p>
      <p>
        To keep them on record, set their status to Transferred or Archived
        instead.
      </p>
    </DeleteDialog>
  );
}
