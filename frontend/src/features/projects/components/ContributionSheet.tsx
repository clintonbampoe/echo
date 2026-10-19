import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import type { ApiError } from "@/lib/api";
import { handleSubmitError } from "@/lib/forms";
import { formatMoney } from "@/lib/format";
import { FormSheet } from "@/ui";
import type { Project } from "../api";
import {
  contributionFieldMap,
  contributionSchema,
  emptyContribution,
  nextContribution,
  toContributionInput,
  type ContributionValues,
} from "../contributionForm";
import { useCreateContribution } from "../hooks";
import { ContributionFields } from "./ContributionFields";

type Props = {
  open: boolean;
  project: Pick<Project, "id" | "name">;
  today: string;
  onClose: () => void;
};

// The "Record contribution" sheet. The form only exists while the sheet is open, so it starts fresh every time.
export function ContributionSheet({ open, project, today, onClose }: Props) {
  return open ? (
    <RecordContribution project={project} today={today} onClose={onClose} />
  ) : null;
}

function RecordContribution({ project, today, onClose }: Omit<Props, "open">) {
  const create = useCreateContribution();
  const [problem, setProblem] = useState<ApiError | null>(null);
  const [recorded, setRecorded] = useState<{ count: number; note: string }>({
    count: 0,
    note: "",
  });
  const form = useForm<ContributionValues>({
    resolver: zodResolver(contributionSchema),
    defaultValues: emptyContribution(today),
  });

  async function save(values: ContributionValues, another: boolean) {
    setProblem(null);
    try {
      const saved = await create.mutateAsync(
        toContributionInput(project.id, values),
      );
      if (!another) return onClose();
      setRecorded({
        count: recorded.count + 1,
        note: `Recorded ${formatMoney(saved.amount)}. Date and method are kept for the next one.`,
      });
      form.reset(nextContribution(values));
    } catch (e) {
      handleSubmitError(e, form.setError, contributionFieldMap, setProblem);
    }
  }

  return (
    <FormSheet
      title="Record contribution"
      subtitle={`Given towards ${project.name}. Not linked to a member.`}
      onClose={onClose}
      formId="contribution-form"
      onSubmit={form.handleSubmit((v) => save(v, false))}
      submitLabel="Record contribution"
      pending={create.isPending}
      failures={Object.keys(form.formState.errors).length}
      note={recorded.note || undefined}
      problem={
        problem && {
          title: "We could not record this contribution",
          message: problem.message,
        }
      }
      another={{
        label: "Save and add another",
        onClick: () => void form.handleSubmit((v) => save(v, true))(),
      }}
    >
      <ContributionFields
        key={recorded.count}
        form={form}
        today={today}
        projectName={project.name}
      />
    </FormSheet>
  );
}
