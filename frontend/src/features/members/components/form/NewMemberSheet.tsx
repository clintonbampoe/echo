import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { useCongregation } from "@/features/account";
import type { ApiError } from "@/lib/api";
import { handleSubmitError } from "@/lib/forms";
import { FormSheet, useToast } from "@/ui";
import { emptyMember, fieldMap, toInput } from "../../forms/mapping";
import { memberSchema, type MemberValues } from "../../forms/schema";
import { useCreateMember } from "../../hooks";
import { NewMemberFields } from "./NewMemberFields";

type Props = { onClose: () => void };

// The "Add member" sheet. The form only exists while the sheet does, so it is blank every time it opens.
export function NewMemberSheet({ onClose }: Props) {
  const church = useCongregation().data?.name;
  const create = useCreateMember();
  const toast = useToast();
  const [problem, setProblem] = useState<ApiError | null>(null);
  const form = useForm<MemberValues>({
    resolver: zodResolver(memberSchema),
    defaultValues: emptyMember,
  });

  // Saves the member. "Add another" keeps the sheet open on a blank form; otherwise it closes.
  async function save(values: MemberValues, another: boolean) {
    setProblem(null);
    try {
      const member = await create.mutateAsync(toInput(values));
      toast({ message: `${member.name} added` });
      if (!another) return onClose();
      form.reset(emptyMember);
      form.setFocus("firstName");
    } catch (e) {
      handleSubmitError(e, form.setError, fieldMap, setProblem);
    }
  }

  return (
    <FormSheet
      title="Add member"
      subtitle={
        church ? `Add someone to ${church}.` : "Add someone to your church."
      }
      onClose={onClose}
      formId="member-form"
      onSubmit={form.handleSubmit((v) => save(v, false))}
      submitLabel="Add member"
      pending={create.isPending}
      failures={Object.keys(form.formState.errors).length}
      problem={
        problem && {
          title: "We could not save this member",
          message: problem.message,
        }
      }
      another={{
        label: "Save and add another",
        onClick: () => void form.handleSubmit((v) => save(v, true))(),
      }}
    >
      <NewMemberFields form={form} />
    </FormSheet>
  );
}
