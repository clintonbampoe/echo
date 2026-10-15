import { DeleteDialog } from "@/ui";
import type { Organization, OrganizationMember } from "./api";
import { useRemoveOrganizationMember } from "./hooks";

type Props = {
  organization: Organization;
  member: OrganizationMember;
  onClose: () => void;
};

// Takes one person out of the group. They stay a church member and keep their other groups.
export function RemoveMemberDialog({ organization, member, onClose }: Props) {
  const remove = useRemoveOrganizationMember();
  return (
    <DeleteDialog
      title={`Remove ${member.memberName} from ${organization.name}?`}
      noun="member"
      confirmLabel="Remove from group"
      cancelLabel="Keep in group"
      onDelete={() => remove.mutateAsync(member.id)}
      onClose={onClose}
    >
      <p>
        They will no longer appear in this group’s members. They stay a member
        of the church and keep their other groups.
      </p>
      <p>You can add them back at any time.</p>
    </DeleteDialog>
  );
}
