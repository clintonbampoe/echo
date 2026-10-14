import { formatDate } from "@/lib/format";
import { paths } from "@/lib/paths";
import { Chip, PersonList, type PersonColumn } from "@/ui";
import type { Visitor } from "./api";
import { kindTone } from "./labels";
import { visitorKind } from "./view";

type Props = {
  visitors: Visitor[];
  // True for the list of visitors who became members: they have no menu here, and their page links on to the member.
  converted: boolean;
  onConvert: (v: Visitor) => void;
  onEdit: (v: Visitor) => void;
  onDelete: (v: Visitor) => void;
};

const phone: PersonColumn<Visitor> = {
  header: "Phone",
  width: "minmax(110px, 1.1fr)",
  low: true,
  cell: (v) => v.phoneNumber || "–",
};
const visits: PersonColumn<Visitor> = {
  header: "Visits",
  width: "0.6fr",
  low: true,
  cell: (v) => String(v.visitCount),
};

// Which columns a visitor row shows. The row itself (avatar, name, email, card layout) is shared.
const current: PersonColumn<Visitor>[] = [
  phone,
  visits,
  {
    header: "Last visit",
    width: "minmax(100px, 1fr)",
    cell: (v) => (v.lastVisitedAt ? formatDate(v.lastVisitedAt) : "–"),
  },
  {
    header: "Status",
    width: "minmax(90px, 0.8fr)",
    chip: true,
    cell: (v) => <Chip tone={kindTone[visitorKind(v)]}>{visitorKind(v)}</Chip>,
  },
];

const converted: PersonColumn<Visitor>[] = [
  phone,
  visits,
  {
    header: "Became member",
    width: "minmax(110px, 1fr)",
    cell: (v) => (v.convertedAt ? formatDate(v.convertedAt) : "–"),
  },
];

export function VisitorList({
  visitors,
  converted: joined,
  onConvert,
  onEdit,
  onDelete,
}: Props) {
  return (
    <PersonList
      label="Visitors"
      rows={visitors}
      rowKey={(v) => v.id}
      person={(v) => ({
        name: v.name,
        email: v.emailAddress,
        href: paths.visitor(v.id),
      })}
      columns={joined ? converted : current}
      // Someone who has become a member is edited on their member page, so they have no menu here.
      actions={
        joined
          ? undefined
          : (v) => [
              { label: "Make member", onSelect: () => onConvert(v) },
              { label: "Edit", onSelect: () => onEdit(v) },
              { label: "Delete", danger: true, onSelect: () => onDelete(v) },
            ]
      }
    />
  );
}
