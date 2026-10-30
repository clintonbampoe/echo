import { Chip, Row, RowCard, useToast } from "@/ui";
import type { MenuItem } from "@/ui";
import type { Invitation } from "../api/invitations";
import { subLine, titleOf } from "../model/display";

type Props = {
  invitations: Invitation[];
  onView: (i: Invitation) => void;
  onEdit: (i: Invitation) => void;
  onRevoke: (i: Invitation) => void;
  onDelete: (i: Invitation) => void;
};

const chipLabel = {
  Revoked: "Off",
  Expired: "Expired",
  Exhausted: "Full",
} as const;

// An invitation that is not Active carries a quiet chip saying so; an active one has nothing to flag.
const chipOf = (i: Invitation) =>
  i.status === "Active" ? null : <Chip>{chipLabel[i.status]}</Chip>;

export function InvitationList({
  invitations,
  onView,
  onEdit,
  onRevoke,
  onDelete,
}: Props) {
  const toast = useToast();
  async function copy(code: string) {
    await navigator.clipboard?.writeText(code);
    toast({ message: "Code copied" });
  }
  const items = (i: Invitation): MenuItem[] => [
    ...(i.code
      ? [{ label: "Copy code", onSelect: () => void copy(i.code!) }]
      : []),
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
          onOpen={() => onView(i)}
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
