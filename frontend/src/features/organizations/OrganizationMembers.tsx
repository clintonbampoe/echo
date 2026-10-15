import { useState } from "react";
import { formatMonth } from "@/lib/format";
import { paths } from "@/lib/paths";
import {
  Alert,
  Chip,
  FilterSelect,
  ListBody,
  PersonList,
  type PersonColumn,
} from "@/ui";
import type { Organization, OrganizationMember, OrganizationRole } from "./api";
import { useOrganizationMembers, useUpdateOrganizationMember } from "./hooks";
import { roleLabel, roleTone } from "./labels";
import styles from "./OrganizationMembers.module.css";
import { roleOptions } from "./options";
import { RemoveMemberDialog } from "./RemoveMemberDialog";
import { roles, toMemberFilters } from "./view";

const columns: PersonColumn<OrganizationMember>[] = [
  {
    header: "Role",
    width: "minmax(120px, 1fr)",
    chip: true,
    cell: (m) => <Chip tone={roleTone[m.role]}>{roleLabel[m.role]}</Chip>,
  },
  {
    header: "Joined",
    width: "minmax(100px, 1.4fr)",
    cell: (m) => formatMonth(m.joinedAt),
  },
];

// The people in one organization, with their role and when they joined. A row opens the member; its ⋯ menu
// changes their role or takes them out of the group.
export function OrganizationMembers({
  organization,
}: {
  organization: Organization;
}) {
  const [role, setRole] = useState("");
  const [removing, setRemoving] = useState<OrganizationMember | null>(null);
  const [failed, setFailed] = useState(false);
  const list = useOrganizationMembers(organization.id, toMemberFilters(role));
  const update = useUpdateOrganizationMember();

  async function changeRole(m: OrganizationMember, next: OrganizationRole) {
    if (next === m.role) return;
    setFailed(false);
    try {
      await update.mutateAsync({ id: m.id, role: next });
    } catch {
      setFailed(true);
    }
  }

  return (
    <>
      <div className={styles.filters}>
        <FilterSelect
          label="Role"
          options={roleOptions}
          value={role}
          onChange={setRole}
        />
      </div>
      {failed && (
        <div className={styles.failed}>
          <Alert title="We could not change that role">
            Check your connection and try again.
          </Alert>
        </div>
      )}
      <ListBody
        list={list}
        noun={["member", "members"]}
        emptyText="People you add to this group will appear here."
        filtered={role !== ""}
        noMatchText="No one in this group has that role."
        onClear={() => setRole("")}
      >
        {(members) => (
          <PersonList
            label="Members"
            rows={members}
            rowKey={(m) => m.id}
            person={(m) => ({
              name: m.memberName,
              href: paths.member(m.memberId),
              plain: true,
            })}
            columns={columns}
            actions={(m) => [
              ...roles.map((r) => ({
                label: roleLabel[r],
                group: "Change role",
                checked: m.role === r,
                onSelect: () => void changeRole(m, r),
              })),
              {
                label: "Remove from group",
                danger: true,
                separated: true,
                onSelect: () => setRemoving(m),
              },
            ]}
          />
        )}
      </ListBody>
      {removing && (
        <RemoveMemberDialog
          organization={organization}
          member={removing}
          onClose={() => setRemoving(null)}
        />
      )}
    </>
  );
}
