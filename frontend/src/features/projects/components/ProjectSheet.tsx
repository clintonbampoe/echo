import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import type { MemberMatch } from "@/features/members";
import type { ApiError } from "@/lib/api";
import { handleSubmitError } from "@/lib/forms";
import { FormSheet } from "@/ui";
import { useCreateProject, useProjectCategories } from "../hooks";
import {
  emptyProject,
  projectFieldMap,
  projectSchema,
  toProjectInput,
  type ProjectValues,
} from "../projectForm";
import { ProjectFields } from "./ProjectFields";

type Props = { open: boolean; onClose: () => void };

// The "New project" sheet. The form only exists while the sheet is open, so it is blank every time.
export function ProjectSheet({ open, onClose }: Props) {
  return open ? <NewProject onClose={onClose} /> : null;
}

function NewProject({ onClose }: { onClose: () => void }) {
  const create = useCreateProject();
  const categories = useProjectCategories();
  const [manager, setManager] = useState<MemberMatch | null>(null);
  const [problem, setProblem] = useState<ApiError | null>(null);
  const form = useForm<ProjectValues>({
    resolver: zodResolver(projectSchema()),
    defaultValues: emptyProject,
  });

  function choose(next: MemberMatch | null) {
    setManager(next);
    form.setValue("managerId", next?.id ?? "", {
      shouldValidate: form.formState.isSubmitted,
    });
  }

  async function save(values: ProjectValues) {
    setProblem(null);
    try {
      await create.mutateAsync(toProjectInput(values));
      onClose();
    } catch (e) {
      handleSubmitError(e, form.setError, projectFieldMap, setProblem);
    }
  }

  return (
    <FormSheet
      title="New project"
      subtitle="Set a target, then record what comes in."
      onClose={onClose}
      formId="project-form"
      onSubmit={form.handleSubmit(save)}
      submitLabel="Create project"
      pending={create.isPending}
      failures={Object.keys(form.formState.errors).length}
      problem={
        problem && {
          title: "We could not save this project",
          message: problem.message,
        }
      }
    >
      <ProjectFields
        form={form}
        categories={categories.data}
        manager={manager}
        onManager={choose}
      />
    </FormSheet>
  );
}
