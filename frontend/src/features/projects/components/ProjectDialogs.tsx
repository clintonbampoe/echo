import type { Project } from "../api";
import { DeleteProjectDialog } from "./DeleteProjectDialog";
import { EditProjectSheet } from "./EditProjectSheet";

// Where an existing project can be opened at: to change, or to delete.
export type ProjectStart = "edit" | "delete";

type Props = { project: Project; start: ProjectStart; onClose: () => void };

// What can happen to one existing project from the list's ⋯ menu.
export function ProjectDialogs({ project, start, onClose }: Props) {
  return start === "delete" ? (
    <DeleteProjectDialog project={project} onClose={onClose} />
  ) : (
    <EditProjectSheet project={project} onClose={onClose} />
  );
}
