import { DeleteDialog } from "@/ui";
import type { Visitor } from "./api";
import { useDeleteVisitor } from "./hooks";

// onDeleted runs after a successful delete (the page uses it to go back to the list).
type Props = { visitor: Visitor; onClose: () => void; onDeleted?: () => void };

// "Delete" removes the visitor from the list and search; the server keeps their visit history.
export function DeleteVisitorDialog({ visitor, onClose, onDeleted }: Props) {
  const remove = useDeleteVisitor();
  return (
    <DeleteDialog
      title={`Delete ${visitor.name}?`}
      noun="visitor"
      onDelete={() => remove.mutateAsync(visitor.id)}
      onClose={onClose}
      onDeleted={onDeleted}
    >
      <p>
        {visitor.firstName} will be removed from your visitors list and search.
        Visit history is kept.
      </p>
      <p>If they have joined the church, make them a member instead.</p>
    </DeleteDialog>
  );
}
