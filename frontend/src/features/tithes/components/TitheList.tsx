import { PaymentChip } from "@/features/transactions";
import { formatDate, formatMoney } from "@/lib/format";
import { PersonList, type PersonColumn } from "@/ui";
import type { Tithe } from "../api";

type Props = {
  tithes: Tithe[];
  onEdit: (t: Tithe) => void;
  onDelete: (t: Tithe) => void;
};

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
    chip: true,
    cell: (t) => <PaymentChip method={t.paymentMethod} />,
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
    cell: (t) => t.description || "–",
  },
];

// The rows: who gave and what. A row opens the edit sheet (rule S6), and the ⋯ menu has the same two actions as elsewhere.
export function TitheList({ tithes, onEdit, onDelete }: Props) {
  return (
    <PersonList
      label="Tithes"
      rows={tithes}
      rowKey={(t) => t.id}
      person={(t) => ({
        name: t.memberName,
        plain: true,
        onOpen: () => onEdit(t),
      })}
      columns={columns}
      actions={(t) => [
        { label: "Edit tithe", onSelect: () => onEdit(t) },
        { label: "Delete tithe", danger: true, onSelect: () => onDelete(t) },
      ]}
    />
  );
}
