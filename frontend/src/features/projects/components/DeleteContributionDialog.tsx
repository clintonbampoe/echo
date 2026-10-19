import { paymentLabel } from "@/features/transactions";
import { formatDate, formatMoney } from "@/lib/format";
import { DeleteDialog } from "@/ui";
import type { Contribution } from "../api";
import { useDeleteContribution } from "../hooks";

type Props = { contribution: Contribution; onClose: () => void };

// "Delete" takes the contribution out of the list and off the amount raised. Most mistakes are better fixed by editing.
export function DeleteContributionDialog({ contribution: c, onClose }: Props) {
  const remove = useDeleteContribution();
  return (
    <DeleteDialog
      title="Delete this contribution?"
      noun="contribution"
      onDelete={() => remove.mutateAsync(c.id)}
      onClose={onClose}
    >
      <p>
        {formatMoney(c.amount)} by {paymentLabel[c.paymentMethod].toLowerCase()}
        , dated {formatDate(c.dateContributed.slice(0, 10))}, will be removed
        from the list and taken off the amount raised.
      </p>
      <p>
        If the amount or date is wrong, edit the contribution instead. It keeps
        the record in place.
      </p>
    </DeleteDialog>
  );
}
