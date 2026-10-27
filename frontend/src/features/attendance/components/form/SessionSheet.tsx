import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import type { ApiError } from "@/lib/api";
import { handleSubmitError } from "@/lib/forms";
import { FormSheet } from "@/ui";
import type { Session } from "../../api/sessions";
import { useCreateSession } from "../../hooks/mutations";
import { useSessionLabels } from "../../hooks/queries";
import {
  emptySession,
  sessionFieldMap,
  sessionSchema,
  toSessionInput,
  typeOptions,
  type SessionValues,
} from "../../forms/session";
import { SessionFields } from "./SessionFields";

type Props = {
  open: boolean;
  day: string;
  onClose: () => void;
  onCreated: (session: Session) => void;
};

// The "New session" sheet. The form only exists while the sheet is open, so it starts fresh every time.
export function SessionSheet({ open, ...rest }: Props) {
  return open ? <NewSession {...rest} /> : null;
}

function NewSession({ day, onClose, onCreated }: Omit<Props, "open">) {
  const create = useCreateSession();
  const labels = useSessionLabels();
  const [problem, setProblem] = useState<ApiError | null>(null);
  const form = useForm<SessionValues>({
    resolver: zodResolver(sessionSchema()),
    defaultValues: emptySession(day),
  });

  async function save(values: SessionValues) {
    setProblem(null);
    try {
      onCreated(await create.mutateAsync(toSessionInput(values)));
      onClose();
    } catch (e) {
      handleSubmitError(e, form.setError, sessionFieldMap, setProblem);
    }
  }

  return (
    <FormSheet
      title="New session"
      subtitle="One gathering you want to take attendance for."
      onClose={onClose}
      formId="session-form"
      onSubmit={form.handleSubmit(save)}
      submitLabel="Create session"
      pending={create.isPending}
      failures={Object.keys(form.formState.errors).length}
      problem={
        problem && {
          title: "We could not create this session",
          message: problem.message,
        }
      }
    >
      <SessionFields form={form} types={typeOptions(labels.data ?? [])} />
    </FormSheet>
  );
}
