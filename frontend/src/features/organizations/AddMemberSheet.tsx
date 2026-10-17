import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm, useWatch } from "react-hook-form";
import { MemberPicker, type MemberMatch } from "@/features/members";
import { isApiError } from "@/lib/api";
import { todayText } from "@/lib/dates";
import { handleSubmitError } from "@/lib/forms";
import { ControlledDate, FormSheet, SegmentedField } from "@/ui";
import {
  addMemberFieldMap,
  addMemberSchema,
  emptyAddMember,
  toMemberInput,
  type AddMemberValues,
} from "./addMember";
import type { Organization } from "./api";
import { useAddOrganizationMember } from "./hooks";
import styles from "./AddMemberSheet.module.css";
import { roleLabel } from "./labels";
import { roles } from "./view";

type Props = {
  organization: Organization;
  // Members already in the group among those loaded, so the search can mark them.
  taken: ReadonlySet<string>;
  onClose: () => void;
};

type Problem = { title: string; message: string };

const roleChoices = roles.map((r) => ({ value: r, label: roleLabel[r] }));

// "Add member": find a church member, choose their role and the day they joined. People are created on the
// Members page; this only puts an existing member in the group. The form exists only while the sheet is open.
export function AddMemberSheet({ organization, taken, onClose }: Props) {
  const add = useAddOrganizationMember();
  const [picked, setPicked] = useState<MemberMatch | null>(null);
  const [problem, setProblem] = useState<Problem | null>(null);
  const form = useForm<AddMemberValues>({
    resolver: zodResolver(addMemberSchema),
    defaultValues: emptyAddMember(todayText()),
  });
  const { errors } = form.formState;
  const role = useWatch({ control: form.control, name: "role" });

  function pick(member: MemberMatch | null) {
    setPicked(member);
    setProblem(null);
    form.setValue("memberId", member?.id ?? "", {
      shouldValidate: form.formState.isSubmitted,
    });
  }

  async function save(values: AddMemberValues) {
    setProblem(null);
    try {
      await add.mutateAsync(toMemberInput(organization.id, values));
      onClose();
    } catch (e) {
      // 409: they are already in the group, probably added by someone else a moment ago.
      if (isApiError(e) && e.status === 409) {
        return setProblem({
          title: `${picked?.name ?? "This person"} is already in ${organization.name}`,
          message:
            "Close this and look for them in the members list, or choose someone else.",
        });
      }
      handleSubmitError(e, form.setError, addMemberFieldMap, (error) =>
        setProblem({
          title: "We could not add this member",
          message: error.message,
        }),
      );
    }
  }

  return (
    <FormSheet
      title="Add member"
      subtitle={`Put a church member in ${organization.name}.`}
      onClose={onClose}
      formId="add-organization-member-form"
      onSubmit={form.handleSubmit(save)}
      submitLabel="Add member"
      pending={add.isPending}
      failures={Object.keys(errors).length}
      problem={problem}
    >
      <div className={styles.fields}>
        <MemberPicker
          value={picked}
          onChange={pick}
          taken={taken}
          takenText="Already in this group"
          error={errors.memberId?.message}
        />
        <SegmentedField
          label="Role"
          options={roleChoices}
          value={role}
          onChange={(r) => form.setValue("role", r)}
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
