import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import type { MemberMatch } from "@/features/members";
import type { ApiError } from "@/lib/api";
import { handleSubmitError } from "@/lib/forms";
import { FormSheet } from "@/ui";
import type { Project } from "../api";
import { useProjectCategories, useUpdateProject } from "../hooks";
import {
  fromProject,
  projectFieldMap,
  projectSchema,
  toProjectUpdate,
  type ProjectValues,
} from "../projectForm";
import { ProjectFields } from "./ProjectFields";

type Props = { project: Project; onClose: () => void };

// The "Edit" sheet: the same form as New project, filled with what the project has now. An end date that is set can be
// changed, not removed: the server has no way to clear it.
export function EditProjectSheet({ project, onClose }: Props) {
  const update = useUpdateProject(project.id);
  const categories = useProjectCategories();
  const [manager, setManager] = useState<
    (Pick<MemberMatch, "id" | "name"> & { phoneNumber?: string }) | null
  >({ id: project.managerId, name: project.managerName });
  const [problem, setProblem] = useState<ApiError | null>(null);
  const form = useForm<ProjectValues>({
    resolver: zodResolver(projectSchema(Boolean(project.endDate))),
    defaultValues: fromProject(project),
  });

  function choose(next: MemberMatch | null) {
    setManager(next);
    form.setValue("managerId", next?.id ?? "", {
      shouldValidate: true,
      shouldDirty: true,
    });
  }

  async function save(values: ProjectValues) {
    setProblem(null);
    try {
      await update.mutateAsync(toProjectUpdate(values));
      onClose();
    } catch (e) {
      handleSubmitError(e, form.setError, projectFieldMap, setProblem);
    }
  }

  return (
    <FormSheet
      title={`Edit ${project.name}`}
      subtitle="Contributions already recorded stay as they are."
      onClose={onClose}
      formId="project-edit-form"
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
      <ProjectFields
        form={form}
        categories={categories.data}
        manager={manager}
        onManager={choose}
      />
    </FormSheet>
  );
}
