import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm, useWatch } from "react-hook-form";
import type { ApiError } from "@/lib/api";
import { todayText } from "@/lib/dates";
import { handleSubmitError } from "@/lib/forms";
import { ControlledDate, FormSheet, SegmentedField } from "@/ui";
import styles from "./EditMembershipSheet.module.css";
import type { Organization } from "../../api/organizations";
import type { OrganizationMember } from "../../api/memberships";
import { useUpdateOrganizationMember } from "../../hooks/mutations";
import { roles } from "../../model/display";
import {
  fromMembership,
  membershipSchema,
  toMembershipInput,
  type MembershipValues,
} from "../../model/membership";
import { roleLabel } from "../../model/labels";

type Props = {
  organization: Organization;
  member: OrganizationMember;
  onClose: () => void;
};

const roleChoices = roles.map((r) => ({ value: r, label: roleLabel[r] }));

// Changes one person's role in this group and the day they joined it. Opened from their membership panel,
// which is where the membership is read; nothing here touches the person themselves (one-owner rule).
export function EditMembershipSheet({ organization, member, onClose }: Props) {
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
      title="Edit membership"
      subtitle={`${member.memberName}, in ${organization.name}`}
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
      </div>
    </FormSheet>
  );
}
