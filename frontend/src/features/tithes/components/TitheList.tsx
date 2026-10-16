import { paymentLabel } from "@/features/transactions";
import { formatDate, formatMoney } from "@/lib/format";
import { PersonList, type PersonColumn } from "@/ui";
import type { Tithe } from "../api";
import type { TitheStart } from "./TitheDialogs";

type Props = { tithes: Tithe[]; onOpen: (t: Tithe, start: TitheStart) => void };

const columns: PersonColumn<Tithe>[] = [
  {
    header: "Tithe for",
    width: "minmax(120px, 1fr)",
    cell: (t) => `${t.forMonth} ${t.forYear}`,
  },
  {
    header: "Amount",
    width: "minmax(100px, 0.8fr)",
    end: true,
    cell: (t) => <strong>{formatMoney(t.amount)}</strong>,
  },
  {
    header: "Method",
    width: "minmax(110px, 0.9fr)",
    cell: (t) => paymentLabel[t.paymentMethod],
  },
  {
    header: "Collected",
    width: "minmax(100px, 0.9fr)",
    cell: (t) => formatDate(t.collectionDate),
  },
  {
    header: "Note",
    width: "minmax(120px, 1.2fr)",
    low: true,
    cell: (t) => t.description || "No note",
  },
];

// The rows: who gave and what. A row opens the tithe to read (rule S6); the ⋯ menu goes straight to editing or deleting.
export function TitheList({ tithes, onOpen }: Props) {
  return (
    <PersonList
      label="Tithes"
      nameHeader="Member"
      rows={tithes}
      rowKey={(t) => t.id}
      person={(t) => ({
        name: t.memberName,
        plain: true,
        onOpen: () => onOpen(t, "view"),
      })}
      columns={columns}
      actions={(t) => [
        { label: "Edit tithe", onSelect: () => onOpen(t, "edit") },
        {
          label: "Delete tithe",
          danger: true,
          onSelect: () => onOpen(t, "delete"),
        },
      ]}
    />
  );
}
