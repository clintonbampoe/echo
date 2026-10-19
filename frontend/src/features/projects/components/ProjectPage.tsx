import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { isApiError } from "@/lib/api";
import { todayText } from "@/lib/dates";
import { paths } from "@/lib/paths";
import { useUrlFilters } from "@/lib/useUrlFilters";
import { Alert, BackLink, Button } from "@/ui";
import type { Contribution, Project } from "../api";
import {
  contributionFilters,
  toContributionFilters,
} from "../contributionView";
import { useContributions, useContributionSummary, useProject } from "../hooks";
import {
  ContributionDialogs,
  type ContributionStart,
} from "./ContributionDialogs";
import { ContributionSheet } from "./ContributionSheet";
import { ContributionsSection } from "./ContributionsSection";
import { DeleteProjectDialog } from "./DeleteProjectDialog";
import { EditProjectSheet } from "./EditProjectSheet";
import styles from "./ProjectPage.module.css";
import { ProjectDetails } from "./ProjectDetails";
import { ProjectHeader } from "./ProjectHeader";
import { ProjectProgress } from "./ProjectProgress";

// One project: what it is for, how far along it is, and what has been given towards it.
export function ProjectPage() {
  const { id = "" } = useParams();
  const project = useProject(id);

  if (project.isPending)
    return (
      <p role="status" className={styles.note}>
        Loading project…
      </p>
    );
  if (project.isError) {
    const missing = isApiError(project.error) && project.error.status === 404;
    return (
      <div className={styles.note}>
        <BackLink to={paths.projects}>Projects</BackLink>
        <Alert
          title={
            missing
              ? "We could not find this project"
              : "We could not load this project"
          }
        >
          {missing
            ? "It may have been deleted."
            : "Check your connection and try again."}
        </Alert>
        {!missing && (
          <Button variant="secondary" onClick={() => void project.refetch()}>
            Try again
          </Button>
        )}
      </div>
    );
  }
  return <Detail project={project.data} />;
}

function Detail({ project: p }: { project: Project }) {
  const navigate = useNavigate();
  const [editing, setEditing] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [recording, setRecording] = useState(false);
  const [opened, setOpened] = useState<{
    contribution: Contribution;
    start: ContributionStart;
  } | null>(null);
  const { filters, update } = useUrlFilters(contributionFilters);
  const query = toContributionFilters(p.id, filters, todayText());
  const list = useContributions(query);
  const summary = useContributionSummary(query);
  // The count in the delete question is of every gift, so it is only given while nothing narrows the list.
  const given = contributionFilters.isFiltered(filters)
    ? undefined
    : summary.data?.totalContributions;

  return (
    <div className="anim-rise">
      <BackLink to={paths.projects}>Projects</BackLink>
      <ProjectHeader
        project={p}
        onEdit={() => setEditing(true)}
        onDelete={() => setDeleting(true)}
        onRecord={() => setRecording(true)}
      />
      <ProjectProgress project={p} />
      <div className={styles.layout}>
        <ProjectDetails project={p} />
        <ContributionsSection
          filters={filters}
          onChange={update}
          list={list}
          summary={summary}
          onRecord={() => setRecording(true)}
          onAction={(contribution, start) => setOpened({ contribution, start })}
        />
      </div>
      <ContributionSheet
        open={recording}
        project={p}
        today={todayText()}
        onClose={() => setRecording(false)}
      />
      {opened && (
        <ContributionDialogs
          key={opened.contribution.id}
          contribution={opened.contribution}
          start={opened.start}
          onClose={() => setOpened(null)}
        />
      )}
      {editing && (
        <EditProjectSheet project={p} onClose={() => setEditing(false)} />
      )}
      {deleting && (
        <DeleteProjectDialog
          project={p}
          contributions={given}
          onClose={() => setDeleting(false)}
          onDeleted={() => navigate(paths.projects, { replace: true })}
        />
      )}
    </div>
  );
}
