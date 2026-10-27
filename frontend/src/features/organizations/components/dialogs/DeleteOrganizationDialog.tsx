import { formatNumber } from "@/lib/format";
import { DeleteDialog } from "@/ui";
import type { Organization } from "../../api/organizations";
import { useDeleteOrganization } from "../../hooks/mutations";

// onDeleted runs after a successful delete (the page uses it to go back to the list).
type Props = {
  organization: Organization;
  onClose: () => void;
  onDeleted?: () => void;
};

// "Delete" removes the group from the list and search. The people in it stay in the church, and the
// events it hosted are kept.
export function DeleteOrganizationDialog({
  organization,
  onClose,
  onDeleted,
}: Props) {
  const remove = useDeleteOrganization();
  const people =
    organization.memberCount === 1
      ? "Its 1 member stays"
      : `Its ${formatNumber(organization.memberCount)} members stay`;
  return (
    <DeleteDialog
      title={`Delete ${organization.name}?`}
      noun="organization"
      onDelete={() => remove.mutateAsync(organization.id)}
      onClose={onClose}
      onDeleted={onDeleted}
    >
      <p>
        The group will be removed from your organizations list and from search.{" "}
        {people} in the church.
      </p>
      <p>Events hosted by this group are kept and still show its name.</p>
    </DeleteDialog>
  );
}
