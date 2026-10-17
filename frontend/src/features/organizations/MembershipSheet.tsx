import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm, useWatch } from "react-hook-form";
import type { ApiError } from "@/lib/api";
import { todayText } from "@/lib/dates";
import { formatDate } from "@/lib/format";
import { handleSubmitError } from "@/lib/forms";
import { paths } from "@/lib/paths";
import {
  Avatar,
  Button,
  Chip,
  ControlledDate,
  FormSheet,
  LinkButton,
  SegmentedField,
} from "@/ui";
import type { Organization, OrganizationMember } from "./api";
import { useUpdateOrganizationMember } from "./hooks";
import { roleLabel, roleTone } from "./labels";
import {
  fromMembership,
  membershipSchema,
  toMembershipInput,
  type MembershipValues,
} from "./membership";
import styles from "./MembershipSheet.module.css";
import { roles } from "./view";

type Props = {
  organization: Organization;
  member: OrganizationMember;
  // Closes this sheet and asks "Remove from group?".
  onRemove: () => void;
  onClose: () => void;
};

const roleChoices = roles.map((r) => ({ value: r, label: roleLabel[r] }));

// One person's place in this group: their role, when they joined and when they were added. Everything here is
// about the group; the person's own details are on their member page, one link away.
export function MembershipSheet({
  organization,
  member,
  onRemove,
  onClose,
}: Props) {
  const update = useUpdateOrganizationMember();
  const [problem, setProblem] = useState<ApiError | null>(null);
  const form = useForm<MembershipValues>({
    resolver: zodResolver(membershipSchema),
    defaultValues: fromMembership(member),
  });
  const { errors, isDirty } = form.formState;
  const role = useWatch({ control: form.control, name: "role" });

  async function save(values: MembershipValues) {
    setProblem(null);
    try {
      await update.mutateAsync({ id: member.id, ...toMembershipInput(values) });
      onClose();
    } catch (e) {
      handleSubmitError(
        e,
        form.setError,
        { role: "role", joinedAt: "joinedAt" },
        setProblem,
      );
    }
  }

  return (
    <FormSheet
      title="Membership"
      subtitle={`In ${organization.name}`}
      onClose={onClose}
      formId="membership-form"
      onSubmit={form.handleSubmit(save)}
      submitLabel="Save changes"
      pending={update.isPending}
      failures={Object.keys(errors).length}
      note={isDirty ? "Unsaved changes" : undefined}
      problem={
        problem && {
          title: "We could not save these changes",
          message: problem.message,
        }
      }
    >
      <div className={styles.body}>
        <div className={styles.who}>
          <Avatar name={member.memberName} size={48} />
          <div className={styles.names}>
            <p className={styles.name}>{member.memberName}</p>
            <Chip tone={roleTone[member.role]}>{roleLabel[member.role]}</Chip>
          </div>
          <LinkButton
            to={paths.member(member.memberId, "organizations")}
            variant="secondary"
            size="sm"
          >
            View profile
          </LinkButton>
        </div>
        <div className={styles.fields}>
          <SegmentedField
            label="Role"
            options={roleChoices}
            value={role}
            onChange={(r) => form.setValue("role", r, { shouldDirty: true })}
          />
          <ControlledDate
            control={form.control}
            name="joinedAt"
            label="Joined"
            max={todayText()}
          />
          <p className={styles.added}>
            Added to this group on {formatDate(member.createdAt)}.
          </p>
        </div>
        <div className={styles.remove}>
          <p className={styles.removeText}>
            Taking them out of the group keeps them as a church member.
          </p>
          <Button variant="secondary" onClick={onRemove}>
            Remove from group
          </Button>
        </div>
      </div>
    </FormSheet>
  );
}
