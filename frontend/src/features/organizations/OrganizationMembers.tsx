import { useState } from "react";
import { formatMonth } from "@/lib/format";
import { Alert, Chip, ListBody, PersonList, type PersonColumn } from "@/ui";
import type { Organization, OrganizationMember, OrganizationRole } from "./api";
import { useOrganizationMembers, useUpdateOrganizationMember } from "./hooks";
import { roleLabel, roleTone } from "./labels";
import { MembersFilterBar } from "./MembersFilterBar";
import styles from "./OrganizationMembers.module.css";
import { useMemberFilters } from "./useMemberFilters";
import { roles } from "./view";
import { useMembershipPanels } from "../attendance/useMembershipPanels";

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

// The people in one organization, with their role and when they joined. They can be searched by name and
// narrowed by role; Clear filters empties both. A row opens their membership panel (about this group only);
// its ⋯ menu changes their role or takes them out of the group without opening anything.
export function OrganizationMembers({
  organization,
}: {
  organization: Organization;
}) {
  const f = useMemberFilters();
  const [failed, setFailed] = useState(false);
  const list = useOrganizationMembers(organization.id, f.api);
  const update = useUpdateOrganizationMember();
  const { open, remove, panels } = useMembershipPanels(organization);

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
      <MembersFilterBar filters={f} />
      {failed && (
        <div className={styles.failed}>
          <Alert title="We could not change that role">
            Check your connection and try again.
          </Alert>
        </div>
      )}
      <ListBody
        list={list}
        icon="user"
        noun={["member", "members"]}
        emptyText="People you add to this group will appear here."
        filtered={f.filtered}
        noMatchText="No one in this group matches that."
        onClear={f.clear}
      >
        {(members) => (
          <PersonList
            label="Members"
            rows={members}
            rowKey={(m) => m.id}
            person={(m) => ({
              name: m.memberName,
              onOpen: () => open(m),
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
                onSelect: () => remove(m),
              },
            ]}
          />
        )}
      </ListBody>
      {panels}
    </>
  );
}
