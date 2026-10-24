import { Presence, useHeld } from "@/ui";
import type { Member } from "../../api/members";
import { EditMemberSheet } from "../form/EditMemberSheet";
import { DeleteMemberDialog } from "./DeleteMemberDialog";

// What someone has asked to do to a member from a list row or a profile.
export type MemberAction = { kind: "edit" | "delete"; member: Member };

type Props = {
  action: MemberAction | null;
  onClose: () => void;
  // After a delete has gone through (a profile page goes back to the list).
  onDeleted?: () => void;
};

// Shows the sheet or dialog for the action, and lets it play its exit before it goes. The member stays held a
// moment after `action` clears, so the closing sheet still has its content.
export function MemberActionHost({ action, onClose, onDeleted }: Props) {
  const held = useHeld(action);
  return (
    <>
      <Presence open={action?.kind === "edit"}>
        {held && <EditMemberSheet member={held.member} onClose={onClose} />}
      </Presence>
      <Presence open={action?.kind === "delete"} ms={160}>
        {held && (
          <DeleteMemberDialog
            member={held.member}
            onClose={onClose}
            onDeleted={onDeleted}
          />
        )}
      </Presence>
    </>
  );
}
