import type { ReactNode } from "react";
import { formatDayMonth } from "@/lib/format";
import { paths } from "@/lib/paths";
import { Chip, PersonList, type MenuItem, type PersonColumn } from "@/ui";
import type { Visitor } from "./api";

export type VisitorAction = "edit" | "convert" | "delete";

type Props = {
  visitors: Visitor[];
  footer?: ReactNode;
  onAct: (visitor: Visitor, action: VisitorAction) => void;
};

// Which columns a visitor row shows: "Joined" for someone who became a member, and when they last came.
// The row itself (avatar, name, the line under it, card layout) is shared.
const columns: PersonColumn<Visitor>[] = [
  {
    header: "Status",
    width: "minmax(80px, 0.6fr)",
    chip: true,
    cell: (v) => (v.convertedAt ? <Chip tone="positive">Joined</Chip> : null),
  },
  {
    header: "Last visit",
    width: "minmax(90px, 0.7fr)",
    end: true,
    cell: (v) => (v.lastVisitedAt ? formatDayMonth(v.lastVisitedAt) : "–"),
  },
];

// The ⋯ menu of a row: what can be done without opening the visitor. Someone who has joined cannot be made a member again.
const menu = (v: Visitor, onAct: Props["onAct"]): MenuItem[] => [
  { label: "Edit visitor", onSelect: () => onAct(v, "edit") },
  ...(v.convertedAt
    ? []
    : [{ label: "Make a member", onSelect: () => onAct(v, "convert") }]),
  {
    label: "Delete visitor",
    danger: true,
    separated: true,
    onSelect: () => onAct(v, "delete"),
  },
];

export function VisitorList({ visitors, footer, onAct }: Props) {
  return (
    <PersonList
      label="Visitors"
      rows={visitors}
      rowKey={(v) => v.id}
      person={(v) => ({
        name: v.name,
        detail: [v.phoneNumber, v.emailAddress].filter(Boolean).join(" · "),
        href: paths.visitor(v.id),
      })}
      columns={columns}
      actions={(v) => menu(v, onAct)}
      compact
      footer={footer}
    />
  );
}
