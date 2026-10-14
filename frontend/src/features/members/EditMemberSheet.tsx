import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import type { ApiError } from "@/lib/api";
import { handleSubmitError } from "@/lib/forms";
import { FormSheet } from "@/ui";
import type { Member } from "./api";
import { useUpdateMember } from "./hooks";
import { MemberFields } from "./MemberFields";
import {
  fieldMap,
  fromMember,
  memberSchema,
  toInput,
  type MemberValues,
} from "./schema";

type Props = { member: Member; onClose: () => void };

// The "Edit" sheet: the same form as New member, filled with what the member has now.
// Note: the server treats an empty optional field as "leave unchanged", so it cannot clear one.
export function EditMemberSheet({ member, onClose }: Props) {
  const update = useUpdateMember(member.id);
  const [problem, setProblem] = useState<ApiError | null>(null);
  const form = useForm<MemberValues>({
    resolver: zodResolver(memberSchema),
    defaultValues: fromMember(member),
  });

  async function save(values: MemberValues) {
    setProblem(null);
    try {
      await update.mutateAsync(toInput(values));
      onClose();
    } catch (e) {
      handleSubmitError(e, form.setError, fieldMap, setProblem);
    }
  }

  return (
    <FormSheet
      title={`Edit ${member.name}`}
      subtitle="Changes save to this member only."
      onClose={onClose}
      formId="member-edit-form"
      onSubmit={form.handleSubmit(save)}
      submitLabel="Save changes"
      pending={update.isPending}
      failures={Object.keys(form.formState.errors).length}
      note={form.formState.isDirty ? "Unsaved changes" : undefined}
      problem={
        problem && {
          title: "We could not save these changes",
          message: problem.message,
        }
      }
    >
      <MemberFields form={form} />
    </FormSheet>
  );
}
