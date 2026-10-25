import { PersonList, DashCard } from "@/ui";
import type { Organization } from "../../api/organizations";
import type { OrganizationMember } from "../../api/memberships";
import { roleLabel } from "../../model/labels";
import { useMembershipPanels } from "../members/useMembershipPanels";
import styles from "./OrganizationDetails.module.css";

type Props = { organization: Organization; leaders: OrganizationMember[] };

// The Details tab: what the group is for, and the people who lead it. The numbers above already say how many
// members it has and when it began, so they are not said again here. A leader opens their membership panel (their
// part in this group), not their member page; everyone else is on the Members tab.
export function OrganizationDetails({ organization: o, leaders }: Props) {
  const { open, panels } = useMembershipPanels(o);
  return (
    <div className={styles.grid}>
      <DashCard title="About">
        <p className={o.description ? styles.text : styles.none}>
          {o.description || "No description yet."}
        </p>
      </DashCard>
      <DashCard title="Leadership">
        {leaders.length > 0 ? (
          <PersonList
            label="Leadership"
            title="Leadership"
            rows={leaders}
            rowKey={(m) => m.id}
            person={(m) => ({
              name: m.memberName,
              detail: roleLabel[m.role],
              onOpen: () => open(m),
            })}
            columns={[]}
            compact
            flat
          />
        ) : (
          <p className={styles.none}>No one leads this group yet.</p>
        )}
      </DashCard>
      {panels}
    </div>
  );
}
