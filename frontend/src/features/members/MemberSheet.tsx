import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { useCongregation } from "@/features/account";
import type { ApiError } from "@/lib/api";
import { handleSubmitError } from "@/lib/forms";
import { FormSheet } from "@/ui";
import { useCreateMember } from "./hooks";
import { MemberFields } from "./MemberFields";
import {
  emptyMember,
  fieldMap,
  memberSchema,
  toInput,
  type MemberValues,
} from "./schema";

type Props = { open: boolean; onClose: () => void };

// The "New member" sheet. The form only exists while the sheet is open, so it is blank every time.
export function MemberSheet({ open, onClose }: Props) {
  return open ? <NewMember onClose={onClose} /> : null;
}

function NewMember({ onClose }: { onClose: () => void }) {
  const church = useCongregation().data?.name;
  const create = useCreateMember();
  const [problem, setProblem] = useState<ApiError | null>(null);
  const [added, setAdded] = useState("");
  const form = useForm<MemberValues>({
    resolver: zodResolver(memberSchema),
    defaultValues: emptyMember,
  });

  // Saves the member. "Add another" keeps the sheet open on a blank form; otherwise it closes.
  async function save(values: MemberValues, another: boolean) {
    setProblem(null);
    try {
      const member = await create.mutateAsync(toInput(values));
      if (!another) return onClose();
      form.reset(emptyMember);
      form.setFocus("firstName");
      setAdded(member.name);
    } catch (e) {
      handleSubmitError(e, form.setError, fieldMap, setProblem);
    }
  }

  return (
    <FormSheet
      title="New member"
      subtitle={
        church ? `Add someone to ${church}.` : "Add someone to your church."
      }
      onClose={onClose}
      formId="member-form"
      onSubmit={form.handleSubmit((v) => save(v, false))}
      submitLabel="Create member"
      pending={create.isPending}
      failures={Object.keys(form.formState.errors).length}
      note={added ? `${added} was added.` : undefined}
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
      <MemberFields form={form} />
    </FormSheet>
  );
}
