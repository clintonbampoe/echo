import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { useNavigate } from "react-router-dom";
import {
  MemberFields,
  memberFieldMap,
  memberSchema,
  toMemberInput,
  type MemberValues,
} from "@/features/members";
import { isApiError } from "@/lib/api";
import { handleSubmitError } from "@/lib/forms";
import { paths } from "@/lib/paths";
import { FormSheet } from "@/ui";
import type { Visitor } from "./api";
import { toMemberValues } from "./convert";
import { useConvertVisitor } from "./hooks";
import styles from "./ConvertSheet.module.css";

type Problem = { title: string; message: string };

// "Make member": the full member form, started with what the visitor record knows. When it saves, the
// visitor is a member (same id, same history) and we stay on their visitor page, which now says so and links on.
export function ConvertSheet({
  visitor,
  onClose,
}: {
  visitor: Visitor;
  onClose: () => void;
}) {
  const navigate = useNavigate();
  const convert = useConvertVisitor(visitor.id);
  const [problem, setProblem] = useState<Problem | null>(null);
  const form = useForm<MemberValues>({
    resolver: zodResolver(memberSchema),
    defaultValues: toMemberValues(visitor),
  });

  async function save(values: MemberValues) {
    setProblem(null);
    try {
      await convert.mutateAsync(toMemberInput(values));
      navigate(paths.visitor(visitor.id));
    } catch (e) {
      // 409: they were already made a member, probably by someone else. Nothing to fix in the form.
      if (isApiError(e) && e.status === 409) {
        return setProblem({
          title: `${visitor.firstName} is already a member`,
          message:
            "A visitor can only be made a member once. Close this and open their member profile instead.",
        });
      }
      handleSubmitError(e, form.setError, memberFieldMap, (error) =>
        setProblem({
          title: "We could not make this person a member",
          message: error.message,
        }),
      );
    }
  }

  return (
    <FormSheet
      title={`Make ${visitor.name} a member`}
      subtitle={`${visitor.firstName} will keep their visit history.`}
      onClose={onClose}
      formId="visitor-convert-form"
      onSubmit={form.handleSubmit(save)}
      submitLabel="Make member"
      pending={convert.isPending}
      failures={Object.keys(form.formState.errors).length}
      problem={problem}
    >
      <p className={styles.intro}>
        {visitor.firstName}’s visits and notes stay with their profile. Fill in
        the details a visitor record does not have.
      </p>
      <MemberFields form={form} />
    </FormSheet>
  );
}
