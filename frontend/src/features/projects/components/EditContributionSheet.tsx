import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import type { ApiError } from "@/lib/api";
import { handleSubmitError } from "@/lib/forms";
import { todayText } from "@/lib/dates";
import { FormSheet } from "@/ui";
import type { Contribution } from "../api";
import {
  contributionFieldMap,
  contributionSchema,
  fromContribution,
  toContributionUpdate,
  type ContributionValues,
} from "../contributionForm";
import { useUpdateContribution } from "../hooks";
import { ContributionFields } from "./ContributionFields";

type Props = { contribution: Contribution; onClose: () => void };

// The "Edit contribution" sheet: the same form as Record contribution, filled with what was recorded.
export function EditContributionSheet({ contribution, onClose }: Props) {
  const update = useUpdateContribution(contribution.id);
  const [problem, setProblem] = useState<ApiError | null>(null);
  const form = useForm<ContributionValues>({
    resolver: zodResolver(contributionSchema),
    defaultValues: fromContribution(contribution),
  });

  async function save(values: ContributionValues) {
    setProblem(null);
    try {
      await update.mutateAsync(toContributionUpdate(values));
      onClose();
    } catch (e) {
      handleSubmitError(e, form.setError, contributionFieldMap, setProblem);
    }
  }

  return (
    <FormSheet
      title="Edit contribution"
      subtitle="Changes update the amount raised straight away."
      onClose={onClose}
      formId="contribution-edit-form"
      onSubmit={form.handleSubmit(save)}
      submitLabel="Save changes"
      pending={update.isPending}
      failures={Object.keys(form.formState.errors).length}
      problem={
        problem && {
          title: "We could not save this contribution",
          message: problem.message,
        }
      }
    >
      <ContributionFields
        form={form}
        today={todayText()}
        projectName={contribution.projectName}
      />
    </FormSheet>
  );
}
