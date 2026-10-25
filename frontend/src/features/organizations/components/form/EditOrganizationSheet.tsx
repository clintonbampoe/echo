import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import type { ApiError } from "@/lib/api";
import { handleSubmitError } from "@/lib/forms";
import { FormSheet } from "@/ui";
import type { Organization } from "../../api/organizations";
import { useUpdateOrganization } from "../../hooks/mutations";
import { OrganizationFields } from "./OrganizationFields";
import {
  fieldMap,
  fromOrganization,
  organizationSchema,
  toInput,
  type OrganizationValues,
} from "../../forms/schema";

type Props = { organization: Organization; onClose: () => void };

// The "Edit" sheet: the same form as New organization, filled with what the group has now.
// Note: the server treats an empty description as "leave unchanged", so it cannot clear one.
export function EditOrganizationSheet({ organization, onClose }: Props) {
  const update = useUpdateOrganization(organization.id);
  const [problem, setProblem] = useState<ApiError | null>(null);
  const form = useForm<OrganizationValues>({
    resolver: zodResolver(organizationSchema),
    defaultValues: fromOrganization(organization),
  });

  async function save(values: OrganizationValues) {
    setProblem(null);
    try {
      await update.mutateAsync(toInput(values));
      onClose();
    } catch (e) {
      handleSubmitError(e, form.setError, fieldMap, setProblem);
    }
  }

  return (
    <FormSheet
      title={`Edit ${organization.name}`}
      subtitle="Changes save to this group only."
      onClose={onClose}
      formId="organization-edit-form"
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
      <OrganizationFields form={form} />
    </FormSheet>
  );
}
