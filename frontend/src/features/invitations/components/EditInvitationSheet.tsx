import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";
import type { ApiError } from "@/lib/api";
import { handleSubmitError } from "@/lib/forms";
import { ControlledSelect, Field, FormSheet, useToast } from "@/ui";
import type { Invitation } from "../api/invitations";
import { useUpdateInvitation } from "../hooks";
import { titleOf } from "../model/display";
import { expiryOptions } from "../model/options";
import styles from "./InviteSheet.module.css";

const schema = z.object({
  label: z.string().trim().max(100, "Keep the name under 100 letters"),
  // "keep" leaves the date alone; the others count from now.
  expiryDays: z.enum(["keep", "1", "7", "14", "30"]),
});
type Values = z.infer<typeof schema>;

type Props = { invitation: Invitation; onClose: () => void };

const options = [{ value: "keep", label: "Keep as it is" }, ...expiryOptions];

// Rename an invitation or give it a new expiry. A switched-off invitation cannot be extended.
export function EditInvitationSheet({ invitation, onClose }: Props) {
  const update = useUpdateInvitation(invitation.id);
  const toast = useToast();
  const [problem, setProblem] = useState<ApiError | null>(null);
  const form = useForm<Values>({
    resolver: zodResolver(schema),
    defaultValues: { label: invitation.label ?? "", expiryDays: "keep" },
  });
  const revoked = invitation.status === "Revoked";

  async function save(values: Values) {
    setProblem(null);
    try {
      await update.mutateAsync({
        label: values.label,
        ...(values.expiryDays === "keep"
          ? {}
          : { expiryDays: Number(values.expiryDays) }),
      });
      toast({ message: "Changes saved" });
      onClose();
    } catch (e) {
      handleSubmitError(e, form.setError, { label: "label" }, setProblem);
    }
  }

  return (
    <FormSheet
      title="Edit invitation"
      subtitle={titleOf(invitation)}
      onClose={onClose}
      formId="invitation-edit-form"
      onSubmit={form.handleSubmit(save)}
      submitLabel="Save changes"
      pending={update.isPending}
      failures={Object.keys(form.formState.errors).length}
      problem={
        problem && {
          title: "We could not save this invitation",
          message: problem.message,
        }
      }
    >
      <div className={styles.fields}>
        <Field
          label="Name"
          optional
          error={form.formState.errors.label?.message}
          {...form.register("label")}
        />
        {!revoked && (
          <ControlledSelect
            control={form.control}
            name="expiryDays"
            label="Expires"
            options={options}
          />
        )}
      </div>
    </FormSheet>
  );
}
