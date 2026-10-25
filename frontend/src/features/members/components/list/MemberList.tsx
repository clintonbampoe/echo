import type { ReactNode } from "react";
import { formatDate } from "@/lib/format";
import { paths } from "@/lib/paths";
import { Chip, PersonList, type PersonColumn } from "@/ui";
import type { Member } from "../../api/members";
import type { MemberAction } from "../dialogs/MemberActionHost";
import { contactLine } from "../../model/display";
import { statusTone } from "../../model/labels";

type Props = {
  members: Member[];
  footer?: ReactNode;
  onAct: (action: MemberAction) => void;
};

// Which columns a member row shows. The row itself (avatar, name, line under it, menu, card layout) is shared.
// A member who is Active has nothing to flag, so only the others carry a chip.
const columns: PersonColumn<Member>[] = [
  {
    header: "Status",
    width: "minmax(100px, 0.8fr)",
    chip: true,
    cell: (m) =>
      m.status === "Active" ? null : (
        <Chip tone={statusTone[m.status]}>{m.status}</Chip>
      ),
  },
  {
    header: "Joined",
    width: "minmax(110px, 0.9fr)",
    end: true,
    cell: (m) => (m.joinedDate ? formatDate(m.joinedDate) : "–"),
  },
];

export function MemberList({ members, footer, onAct }: Props) {
  return (
    <PersonList
      label="Members"
      title="All members"
      rows={members}
      rowKey={(m) => m.id}
      person={(m) => ({
        name: m.name,
        detail: contactLine(m),
        href: paths.member(m.id),
      })}
      columns={columns}
      actions={(m) => [
        { label: "Edit", onSelect: () => onAct({ kind: "edit", member: m }) },
        {
          label: "Delete",
          danger: true,
          onSelect: () => onAct({ kind: "delete", member: m }),
        },
      ]}
      compact
      footer={footer}
    />
  );
}
