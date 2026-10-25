import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import type { ApiError } from "@/lib/api";
import { handleSubmitError } from "@/lib/forms";
import { FormSheet, useToast } from "@/ui";
import type { Member } from "../../api/members";
import { fieldMap, fromMember, toInput } from "../../forms/mapping";
import { memberSchema, type MemberValues } from "../../forms/schema";
import { useUpdateMember } from "../../hooks";
import { MemberFields } from "./MemberFields";

type Props = { member: Member; onClose: () => void };

// The "Edit member" sheet: every detail we keep, filled with what the member has now.
// Note: the server treats an empty optional field as "leave unchanged", so it cannot clear one.
export function EditMemberSheet({ member, onClose }: Props) {
  const update = useUpdateMember(member.id);
  const toast = useToast();
  const [problem, setProblem] = useState<ApiError | null>(null);
  const form = useForm<MemberValues>({
    resolver: zodResolver(memberSchema),
    defaultValues: fromMember(member),
  });

  async function save(values: MemberValues) {
    setProblem(null);
    try {
      await update.mutateAsync(toInput(values));
      toast({ message: "Changes saved" });
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
