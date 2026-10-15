import { formatMoney } from "@/lib/format";
import { DeleteDialog } from "@/ui";
import type { Tithe } from "../api";
import { useDeleteTithe } from "../hooks";

type Props = { tithe: Tithe; onClose: () => void };

// "Delete" takes the tithe out of the lists and the totals. Most mistakes are better fixed by editing.
export function DeleteTitheDialog({ tithe, onClose }: Props) {
  const remove = useDeleteTithe();
  return (
    <DeleteDialog
      title="Delete this tithe?"
      noun="tithe"
      onDelete={() => remove.mutateAsync(tithe.id)}
      onClose={onClose}
    >
      <p>
        {formatMoney(tithe.amount)} from {tithe.memberName}, for{" "}
        {tithe.forMonth} {tithe.forYear}, will be removed from lists and totals.
      </p>
      <p>
        If the amount, month or method is wrong, edit the tithe instead. It
        keeps the record in place.
      </p>
    </DeleteDialog>
  );
}
