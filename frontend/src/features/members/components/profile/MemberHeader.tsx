import { Button, Chip, ActionMenu, ProfileHeader } from "@/ui";
import type { Member } from "../../api/members";
import { profileLine } from "../../model/display";
import { statusTone } from "../../model/labels";

type Props = { member: Member; onEdit: () => void; onDelete: () => void };

// The top of a member's page: who they are, Edit, and a ⋯ menu for the things you rarely want (Delete).
export function MemberHeader({ member: m, onEdit, onDelete }: Props) {
  return (
    <ProfileHeader
      label="Member"
      name={m.name}
      line={profileLine(m)}
      badges={
        m.status === "Active" ? null : (
          <Chip tone={statusTone[m.status]}>{m.status}</Chip>
        )
      }
    >
      <Button onClick={onEdit}>Edit</Button>
      <ActionMenu
        label={`More actions for ${m.name}`}
        title={m.name}
        items={[{ label: "Delete", danger: true, onSelect: onDelete }]}
      />
    </ProfileHeader>
  );
}
