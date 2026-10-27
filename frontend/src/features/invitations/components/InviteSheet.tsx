import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm, useWatch } from "react-hook-form";
import type { ApiError } from "@/lib/api";
import { handleSubmitError } from "@/lib/forms";
import { ControlledSelect, Field, FormSheet } from "@/ui";
import type { CreatedInvitation } from "../api/invitations";
import { useCreateInvitation } from "../hooks";
import {
  emptyInvitation,
  fieldMap,
  invitationSchema,
  toInput,
  type InvitationValues,
} from "../model/form";
import { expiryOptions, roleOptions } from "../model/options";
import { InviteCreated } from "./InviteCreated";
import styles from "./InviteSheet.module.css";

type Props = { onClose: () => void };

// The "Invite people" sheet. It asks what the invitation lets people be and for how long; once created
// it shows the code, which is the only time the code can be seen.
export function InviteSheet({ onClose }: Props) {
  const create = useCreateInvitation();
  const [created, setCreated] = useState<CreatedInvitation | null>(null);
  const [problem, setProblem] = useState<ApiError | null>(null);
  const form = useForm<InvitationValues>({
    resolver: zodResolver(invitationSchema),
    defaultValues: emptyInvitation,
  });
  const role = useWatch({ control: form.control, name: "allowedRole" });

  async function save(values: InvitationValues) {
    setProblem(null);
    try {
      setCreated(await create.mutateAsync(toInput(values)));
    } catch (e) {
      handleSubmitError(e, form.setError, fieldMap, setProblem);
    }
  }

  if (created) return <InviteCreated invitation={created} onClose={onClose} />;
  return (
    <FormSheet
      title="Invite people"
      onClose={onClose}
      formId="invitation-form"
      onSubmit={form.handleSubmit(save)}
      submitLabel="Create invitation"
      pending={create.isPending}
      failures={Object.keys(form.formState.errors).length}
      problem={
        problem && {
          title: "We could not create this invitation",
          message: problem.message,
        }
      }
    >
      <div className={styles.fields}>
        <Field
          label="Name"
          optional
          placeholder="Youth team"
          hint="Helps you tell invitations apart in the list."
          error={form.formState.errors.label?.message}
          {...form.register("label")}
        />
        <div className={styles.group}>
          <ControlledSelect
            control={form.control}
            name="allowedRole"
            label="Role"
            options={roleOptions}
          />
          {role === "Clerk" && (
            <p className={styles.hint}>
              Clerks can record tithes and attendance. They cannot see finance.
            </p>
          )}
        </div>
        <div className={styles.group}>
          <ControlledSelect
            control={form.control}
            name="expiryDays"
            label="Expires"
            options={expiryOptions}
          />
          <p className={styles.hint}>
            Anyone with the code can join until then.
          </p>
        </div>
      </div>
    </FormSheet>
  );
}
