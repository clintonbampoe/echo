import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import type { ApiError } from "@/lib/api";
import { handleSubmitError } from "@/lib/forms";
import { FormSheet } from "@/ui";
import { useCreateOrganization } from "./hooks";
import { OrganizationFields } from "./OrganizationFields";
import {
  emptyOrganization,
  fieldMap,
  organizationSchema,
  toInput,
  type OrganizationValues,
} from "./schema";

type Props = { open: boolean; onClose: () => void };

// The "New organization" sheet. The form only exists while the sheet is open, so it is blank every time.
export function OrganizationSheet({ open, onClose }: Props) {
  return open ? <NewOrganization onClose={onClose} /> : null;
}

function NewOrganization({ onClose }: { onClose: () => void }) {
  const create = useCreateOrganization();
  const [problem, setProblem] = useState<ApiError | null>(null);
  const form = useForm<OrganizationValues>({
    resolver: zodResolver(organizationSchema),
    defaultValues: emptyOrganization,
  });

  async function save(values: OrganizationValues) {
    setProblem(null);
    try {
      await create.mutateAsync(toInput(values));
      onClose();
    } catch (e) {
      handleSubmitError(e, form.setError, fieldMap, setProblem);
    }
  }

  return (
    <FormSheet
      title="New organization"
      subtitle="A ministry, group or team in your church."
      onClose={onClose}
      formId="organization-form"
      onSubmit={form.handleSubmit(save)}
      submitLabel="Create organization"
      pending={create.isPending}
      failures={Object.keys(form.formState.errors).length}
      problem={
        problem && {
          title: "We could not save this organization",
          message: problem.message,
        }
      }
    >
      <OrganizationFields form={form} />
    </FormSheet>
  );
}
