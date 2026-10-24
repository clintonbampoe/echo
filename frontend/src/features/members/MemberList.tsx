import { formatDate } from "@/lib/format";
import { paths } from "@/lib/paths";
import { regionLabel } from "@/lib/regions";
import { Chip, PersonList, type PersonColumn } from "@/ui";
import type { Member } from "./api";
import { statusTone } from "./labels";

type Props = {
  members: Member[];
  onEdit: (m: Member) => void;
  onDelete: (m: Member) => void;
};

// Which columns a member row shows. The row itself (avatar, name, email, menu, card layout) is shared.
const columns: PersonColumn<Member>[] = [
  {
    header: "Phone",
    width: "minmax(110px, 1.1fr)",
    low: true,
    cell: (m) => m.phoneNumber,
  },
  {
    header: "Location",
    width: "minmax(150px, 1.6fr)",
    low: true,
    cell: (m) => `${m.city}, ${regionLabel(m.region)}`,
  },
  { header: "Gender", width: "0.7fr", low: true, cell: (m) => m.gender },
  {
    header: "Status",
    width: "minmax(90px, 0.8fr)",
    chip: true,
    cell: (m) => <Chip tone={statusTone[m.status]}>{m.status}</Chip>,
  },
  {
    header: "Joined",
    width: "minmax(100px, 0.9fr)",
    cell: (m) => (m.joinedDate ? formatDate(m.joinedDate) : "–"),
  },
];

export function MemberList({ members, onEdit, onDelete }: Props) {
  return (
    <PersonList
      label="Members"
      rows={members}
      rowKey={(m) => m.id}
      person={(m) => ({
        name: m.name,
        detail: m.emailAddress,
        href: paths.member(m.id),
      })}
      columns={columns}
      actions={(m) => [
        { label: "Edit", onSelect: () => onEdit(m) },
        { label: "Delete", danger: true, onSelect: () => onDelete(m) },
      ]}
    />
  );
}
