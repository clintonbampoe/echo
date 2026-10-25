import { useState, type ReactNode } from "react";
import { EditMembershipSheet } from "./EditMembershipSheet";
import { MembershipPanel } from "./MembershipPanel";
import type { Organization } from "../organizations/api/organizations";
import type { OrganizationMember } from "../organizations/api/memberships";
import { RemoveMemberDialog } from "../organizations/components/members/RemoveMemberDialog";

type Panels = {
  open: (member: OrganizationMember) => void;
  remove: (member: OrganizationMember) => void;
  panels: ReactNode;
};

// Opening a membership is the same three steps: the panel that reads it, the sheet that changes it, and the
// dialog that takes the person out of the group. The list keeps `panels` on the page and calls `open` on a row.
export function useMembershipPanels(organization: Organization): Panels {
  const [viewing, setViewing] = useState<OrganizationMember | null>(null);
  const [editing, setEditing] = useState<OrganizationMember | null>(null);
  const [removing, setRemoving] = useState<OrganizationMember | null>(null);

  const panels = (
    <>
      {viewing && (
        <MembershipPanel
          organization={organization}
          member={viewing}
          onClose={() => setViewing(null)}
          onEdit={() => {
            setEditing(viewing);
            setViewing(null);
          }}
          onRemove={() => {
            setRemoving(viewing);
            setViewing(null);
          }}
        />
      )}
      {editing && (
        <EditMembershipSheet
          organization={organization}
          member={editing}
          onClose={() => setEditing(null)}
        />
      )}
      {removing && (
        <RemoveMemberDialog
          organization={organization}
          member={removing}
          onClose={() => setRemoving(null)}
        />
      )}
    </>
  );

  return { open: setViewing, remove: setRemoving, panels };
}
