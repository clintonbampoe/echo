import type { ReactNode } from "react";
import { roleLabel, useMe, type UserRole } from "@/features/account";
import { Chip, PersonList, type MenuItem } from "@/ui";
import type { StaffUser } from "../api/staff";
import { flag, isDisabled } from "../model/display";
import type { RowAction } from "../model/actions";

type Props = {
  role: UserRole;
  rows: StaffUser[];
  // The count and "Load more", or a link to the role's page. Whatever the card or page puts at the bottom.
  footer?: ReactNode;
  onAct: (action: RowAction, user: StaffUser) => void;
};

// The people of one role as rows, with their status and the ⋯ menu. The role's card and the role's own page draw the
// same rows, so what an admin can do to a person never depends on where they found them.
export function StaffRoleList({ role, rows, footer, onAct }: Props) {
  const me = useMe();

  const items = (u: StaffUser): MenuItem[] =>
    u.id === me.data?.id
      ? [{ label: "View details", onSelect: () => onAct("view", u) }]
      : [
          { label: "View details", onSelect: () => onAct("view", u) },
          { label: "Change role", onSelect: () => onAct("role", u) },
          isDisabled(u)
            ? { label: "Enable account", onSelect: () => onAct("enable", u) }
            : { label: "Disable account", onSelect: () => onAct("disable", u) },
          {
            label: "Remove account",
            danger: true,
            separated: true,
            onSelect: () => onAct("remove", u),
          },
        ];

  return (
    <PersonList
      label={roleLabel(role)}
      title={roleLabel(role)}
      // No count in the header: the lists load a page at a time, so only what is loaded is known.
      aside=""
      rows={rows}
      rowKey={(u) => u.id}
      person={(u) => ({
        name: u.name,
        detail: u.emailAddress,
        onOpen: () => onAct("view", u),
      })}
      columns={[
        {
          header: "Status",
          width: "96px",
          chip: true,
          cell: (u) => {
            const f = flag(u, u.id === me.data?.id);
            return f ? <Chip tone={f.tone}>{f.label}</Chip> : null;
          },
        },
      ]}
      actions={items}
      footer={footer}
    />
  );
}
