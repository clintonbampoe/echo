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
import { expiryInput, maxUsesField } from "../model/form";
import { expiryOptions } from "../model/options";
import styles from "./InviteSheet.module.css";

const schema = z.object({
  label: z.string().trim().max(100, "Keep the name under 100 letters"),
  // "keep" leaves the date alone; the others count from now.
  expiryDays: z.enum(["keep", "1", "7", "14", "30", "90", "never"]),
  maxUses: maxUsesField,
});
type Values = z.infer<typeof schema>;

type Props = { invitation: Invitation; onClose: () => void };

const options = [{ value: "keep", label: "Keep as it is" }, ...expiryOptions];

// Rename an invitation, give it a new expiry or raise its limit. A switched-off invitation cannot be extended.
export function EditInvitationSheet({ invitation, onClose }: Props) {
  const update = useUpdateInvitation(invitation.id);
  const toast = useToast();
  const [problem, setProblem] = useState<ApiError | null>(null);
  const form = useForm<Values>({
    resolver: zodResolver(schema),
    defaultValues: {
      label: invitation.label ?? "",
      expiryDays: "keep",
      maxUses: invitation.maxUses ? String(invitation.maxUses) : "",
    },
  });
  const revoked = invitation.status === "Revoked";
  const previousLimit = invitation.maxUses ? String(invitation.maxUses) : "";

  async function save(values: Values) {
    setProblem(null);
    try {
      await update.mutateAsync({
        label: values.label,
        ...(values.expiryDays === "keep" ? {} : expiryInput(values.expiryDays)),
        // A limit can be raised but not removed, so an empty box leaves it alone.
        ...(values.maxUses && values.maxUses !== previousLimit
          ? { maxUses: Number(values.maxUses) }
          : {}),
      });
      toast({ message: "Changes saved" });
      onClose();
    } catch (e) {
      handleSubmitError(
        e,
        form.setError,
        { label: "label", maxUses: "maxUses" },
        setProblem,
      );
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
          <>
            <ControlledSelect
              control={form.control}
              name="expiryDays"
              label="Expires"
              options={options}
            />
            <Field
              label="Limit"
              inputMode="numeric"
              placeholder="No limit"
              hint={`How many people can join. It can go up, not down below the ${invitation.useCount} who joined, and a limit cannot be removed.`}
              error={form.formState.errors.maxUses?.message}
              {...form.register("maxUses")}
            />
          </>
        )}
      </div>
    </FormSheet>
  );
}
