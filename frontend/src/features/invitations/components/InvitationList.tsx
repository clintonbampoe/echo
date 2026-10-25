import { Chip, Row, RowCard } from "@/ui";
import type { MenuItem } from "@/ui";
import type { Invitation } from "../api/invitations";
import { subLine, titleOf } from "../model/display";

type Props = {
  invitations: Invitation[];
  onEdit: (i: Invitation) => void;
  onRevoke: (i: Invitation) => void;
  onDelete: (i: Invitation) => void;
};

// An invitation that is not Active carries a quiet chip saying so; an active one has nothing to flag.
const chipOf = (i: Invitation) =>
  i.status === "Active" ? null : (
    <Chip>{i.status === "Revoked" ? "Off" : "Expired"}</Chip>
  );

export function InvitationList({
  invitations,
  onEdit,
  onRevoke,
  onDelete,
}: Props) {
  const items = (i: Invitation): MenuItem[] => [
    { label: "Edit invitation", onSelect: () => onEdit(i) },
    ...(i.status === "Revoked"
      ? []
      : [{ label: "Switch off", onSelect: () => onRevoke(i) }]),
    {
      label: "Delete",
      danger: true,
      separated: true,
      onSelect: () => onDelete(i),
    },
  ];
  return (
    <RowCard label="Invitations">
      {invitations.map((i) => (
        <Row
          key={i.id}
          icon="inbox"
          title={titleOf(i)}
          sub={subLine(i)}
          muted={i.status !== "Active"}
          end={chipOf(i)}
          actions={{
            label: `Actions for ${titleOf(i)}`,
            title: titleOf(i),
            items: items(i),
          }}
        />
      ))}
    </RowCard>
  );
}
