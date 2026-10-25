import { formatNumber } from "@/lib/format";
import { DeleteDialog } from "@/ui";
import type { Project } from "../../api/projects";
import { useDeleteProject } from "../../hooks/mutations";

// onDeleted runs after a successful delete (the project's page uses it to go back to the list). `contributions` is how
// many were given, when the page knows.
type Props = {
  project: Pick<Project, "id" | "name">;
  contributions?: number;
  onClose: () => void;
  onDeleted?: () => void;
};

// "Delete" removes the project from the list and the totals. What was given is kept, but nothing new can be added.
export function DeleteProjectDialog({
  project,
  contributions,
  onClose,
  onDeleted,
}: Props) {
  const remove = useDeleteProject();
  const kept =
    contributions === undefined
      ? "contributions are"
      : `${formatNumber(contributions)} ${contributions === 1 ? "contribution is" : "contributions are"}`;
  return (
    <DeleteDialog
      title={`Delete ${project.name}?`}
      noun="project"
      onDelete={() => remove.mutateAsync(project.id)}
      onClose={onClose}
      onDeleted={onDeleted}
    >
      <p>It will be removed from the projects list and from the totals.</p>
      <p>
        Its {kept} kept in history, and money already given is not lost. You
        cannot add new contributions to a deleted project.
      </p>
    </DeleteDialog>
  );
}
