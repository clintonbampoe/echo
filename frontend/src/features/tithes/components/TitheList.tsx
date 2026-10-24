import { paymentLabel } from "@/features/transactions";
import type { ReactNode } from "react";
import { formatDayMonth, formatNumber } from "@/lib/format";
import { PersonList, type PersonColumn } from "@/ui";
import type { Tithe } from "../api";
import type { TitheStart } from "./TitheDialogs";

type Props = {
  tithes: Tithe[];
  onOpen: (t: Tithe, start: TitheStart) => void;
  footer?: ReactNode;
};

const columns: PersonColumn<Tithe>[] = [
  {
    header: "Amount",
    width: "minmax(80px, 0.5fr)",
    end: true,
    cell: (t) => <strong>{formatNumber(t.amount)}</strong>,
  },
];

// The rows: who gave, how and when, and the amount. A row opens the tithe to read, where it can be edited or deleted.
export function TitheList({ tithes, onOpen, footer }: Props) {
  return (
    <PersonList
      label="Tithes"
      rows={tithes}
      rowKey={(t) => t.id}
      person={(t) => ({
        name: t.memberName,
        detail: `${paymentLabel[t.paymentMethod]} · ${formatDayMonth(t.collectionDate)}`,
        onOpen: () => onOpen(t, "view"),
      })}
      columns={columns}
      compact
      footer={footer}
    />
  );
}
