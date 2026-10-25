import { formatDate, formatMonth } from "@/lib/format";
import { paths } from "@/lib/paths";
import { Button, Chip, DetailGroup, PanelIdentity, Sheet } from "@/ui";
import styles from "./MembershipPanel.module.css";
import type { Organization } from "../../api/organizations";
import type { OrganizationMember } from "../../api/memberships";
import { roleLabel, roleTone } from "../../model/labels";

type Props = {
  organization: Organization;
  member: OrganizationMember;
  // Each one closes the panel and opens its own sheet or dialog on the page behind.
  onEdit: () => void;
  onRemove: () => void;
  onClose: () => void;
};

// One person's place in this group, read only: their role, when they joined and when they were added.
// Everything here is about the group; the person's own details are on their member page, one link away
// (design rule S6). Changing or removing the membership is a deliberate next step.
export function MembershipPanel({
  organization,
  member,
  onEdit,
  onRemove,
  onClose,
}: Props) {
  const footer = (
    <div className={styles.actions}>
      <Button variant="secondary" onClick={onRemove} className={styles.delete}>
        Remove from group
      </Button>
      <Button onClick={onEdit}>Edit membership</Button>
    </div>
  );
  return (
    <Sheet
      open
      onClose={onClose}
      title="Membership"
      subtitle={`In ${organization.name}`}
      footer={footer}
    >
      <div className={styles.body}>
        <PanelIdentity
          name={member.memberName}
          meta={
            <Chip tone={roleTone[member.role]}>{roleLabel[member.role]}</Chip>
          }
          profileHref={paths.member(member.memberId, "organizations")}
        />
        <DetailGroup
          title="Details"
          rows={[
            ["Role", roleLabel[member.role]],
            ["Joined", formatMonth(member.joinedAt)],
            ["Added to this group", formatDate(member.createdAt.slice(0, 10))],
          ]}
        />
      </div>
    </Sheet>
  );
}
