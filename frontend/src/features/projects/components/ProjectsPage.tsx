import { useState } from "react";
import { todayText } from "@/lib/dates";
import { paths } from "@/lib/paths";
import { useUrlFilters } from "@/lib/useUrlFilters";
import { LinkButton, PageHeader, StatStrip } from "@/ui";
import { periodOptions, projectFilters, toApiFilters } from "../filters";
import {
  useProjectCategories,
  useProjectManagers,
  useProjects,
  useProjectSummary,
} from "../hooks";
import { managerOptions, projectStats } from "../view";
import { ProjectFilterBar } from "./ProjectFilterBar";
import { ProjectList } from "./ProjectList";
import { ProjectSheet } from "./ProjectSheet";

// The Projects page: what is being raised for and how far along each is, with the numbers, the filters and every project.
export function ProjectsPage() {
  const today = todayText();
  const [creating, setCreating] = useState(false);
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
      />
      <ProjectSheet open={creating} onClose={() => setCreating(false)} />
    </div>
  );
}
