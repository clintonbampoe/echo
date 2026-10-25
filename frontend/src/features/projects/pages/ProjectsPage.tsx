import { useState } from "react";
import { todayText } from "@/lib/dates";
import { paths } from "@/lib/paths";
import { useUrlFilters } from "@/lib/useUrlFilters";
import { LinkButton, PageHeader, StatStrip } from "@/ui";
import { periodOptions, projectFilters, toApiFilters } from "../model/filters";
import {
  useProjectCategories,
  useProjectManagers,
  useProjects,
  useProjectSummary,
} from "../hooks/queries";
import { managerOptions, projectStats } from "../model/display";
import type { Project } from "../api/projects";
import { DeleteProjectDialog } from "../components/dialogs/DeleteProjectDialog";
import { EditProjectSheet } from "../components/form/EditProjectSheet";
import { ProjectFilterBar } from "../components/list/ProjectFilterBar";
import { ProjectList } from "../components/list/ProjectList";
import { ProjectSheet } from "../components/form/ProjectSheet";

// The Projects page: what is being raised for and how far along each is, with the numbers, the filters and every project.
export function ProjectsPage() {
  const today = todayText();
  const [creating, setCreating] = useState(false);
  const [editing, setEditing] = useState<Project | null>(null);
  const [deleting, setDeleting] = useState<Project | null>(null);
  const { filters, update, clear, resetKey } = useUrlFilters(projectFilters);
  const categories = useProjectCategories();
  const managers = useProjectManagers();
  const query = toApiFilters(filters, today);
  const list = useProjects(query);
  const summary = useProjectSummary(query);
  const period =
    periodOptions.find((o) => o.value === filters.window)?.label ?? "";
  const wide =
    filters.window === "all" &&
    !projectFilters.isFiltered({ ...filters, window: "year" });

  return (
    <div className="anim-rise">
      <PageHeader
        title="Projects"
        action={
          <LinkButton to={paths.projectCategories} variant="secondary">
            Categories
          </LinkButton>
        }
        add={{ label: "New project", onClick: () => setCreating(true) }}
      />
      <StatStrip
        label="Projects summary"
        stats={projectStats(summary.data, period)}
        busy={summary.isPending}
      />
      <ProjectFilterBar
        filters={filters}
        onChange={update}
        resetKey={resetKey}
        onClear={clear}
        categories={categories.data}
        managers={managerOptions(managers.data?.data)}
      />
      <ProjectList
        list={list}
        filtered={projectFilters.isFiltered(filters) && !wide}
        everything={wide}
        onClear={clear}
        onEdit={setEditing}
        onDelete={setDeleting}
      />
      <ProjectSheet open={creating} onClose={() => setCreating(false)} />
      {editing && (
        <EditProjectSheet project={editing} onClose={() => setEditing(null)} />
      )}
      {deleting && (
        <DeleteProjectDialog
          project={deleting}
          onClose={() => setDeleting(null)}
        />
      )}
    </div>
  );
}
