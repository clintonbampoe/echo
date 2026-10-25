import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import type { ApiError } from "@/lib/api";
import { handleSubmitError } from "@/lib/forms";
import { FormSheet } from "@/ui";
import type { Session } from "../../api/sessions";
import { shortDay } from "../../model/day";
import { useUpdateSession } from "../../hooks/mutations";
import {
  fromSession,
  lockedTimes,
  sessionFieldMap,
  sessionSchema,
  toSessionUpdate,
  type SessionValues,
} from "../../forms/session";
import { SessionFields } from "./SessionFields";

type Props = { session: Session; onClose: () => void };

// The "Edit session" sheet: the same form, filled with what was saved. The type and the day cannot change.
export function EditSessionSheet({ session, onClose }: Props) {
  const update = useUpdateSession();
  const [problem, setProblem] = useState<ApiError | null>(null);
  const form = useForm<SessionValues>({
    resolver: zodResolver(sessionSchema(lockedTimes(session))),
    defaultValues: fromSession(session),
  });

  async function save(values: SessionValues) {
    setProblem(null);
    try {
      await update.mutateAsync({ id: session.id, ...toSessionUpdate(values) });
      onClose();
    } catch (e) {
      handleSubmitError(e, form.setError, sessionFieldMap, setProblem);
    }
  }

  const where = `${session.sessionLabelName}, ${shortDay(session.date)} ${session.date.slice(0, 4)}`;
  return (
    <FormSheet
      title="Edit session"
      subtitle={where}
      onClose={onClose}
      formId="session-edit-form"
      onSubmit={form.handleSubmit(save)}
      submitLabel="Save changes"
      pending={update.isPending}
      failures={Object.keys(form.formState.errors).length}
      problem={
        problem && {
          title: "We could not save this session",
          message: problem.message,
        }
      }
    >
      <SessionFields form={form} types={[]} fixed={where} />
    </FormSheet>
  );
}
