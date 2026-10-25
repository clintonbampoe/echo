import { formatDate, formatMoney } from "@/lib/format";
import type { Paged } from "@/lib/paged";
import { paymentLabel } from "@/features/transactions";
import { DateTile, RecordTable } from "@/ui";
import type { Contribution } from "../../api/contributions";
import type { ContributionStart } from "./ContributionDialogs";

type Props = {
  list: Paged<Contribution>;
  onAction: (contribution: Contribution, start: ContributionStart) => void;
};

// Every gift in view, newest first. A table on a wide screen, a card for each on a phone. A note too long for its cell
// wraps. A row opens the gift to read; the ⋯ menu goes straight to editing or deleting.
export function ContributionList({ list, onAction }: Props) {
  return (
    <RecordTable
      label="Contributions"
      title="Contributions"
      list={list}
      rowKey={(c) => c.id}
      onOpen={(c) => onAction(c, "view")}
      rowName={(c) =>
        `${formatMoney(c.amount)} on ${formatDate(c.dateContributed)}`
      }
      actions={(c) => [
        { label: "Edit contribution", onSelect: () => onAction(c, "edit") },
        {
          label: "Delete contribution",
          danger: true,
          onSelect: () => onAction(c, "delete"),
        },
      ]}
      line={(c) => ({
        lead: <DateTile date={c.dateContributed} />,
        title: paymentLabel[c.paymentMethod],
        sub: c.description,
        end: <strong>{formatMoney(c.amount)}</strong>,
      })}
      noun={["contribution", "contributions"]}
      summary=". Newest first."
      empty={{
        icon: "search",
        title: "No contributions found",
        text: "Try another method, or a wider period.",
      }}
      columns={[
        { header: "Date", cell: (c) => formatDate(c.dateContributed) },
        { header: "Method", cell: (c) => paymentLabel[c.paymentMethod] },
        {
          header: "Note",
          cell: (c) => c.description || "No note",
          full: true,
          last: true,
        },
        {
          header: "Amount",
          end: true,
          cell: (c) => <strong>{formatMoney(c.amount)}</strong>,
        },
      ]}
    />
  );
}
