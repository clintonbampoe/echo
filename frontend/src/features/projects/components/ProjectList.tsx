import { formatMoney } from "@/lib/format";
import type { Paged } from "@/lib/paged";
import { paths } from "@/lib/paths";
import { Chip, ProgressBar, RecordTable } from "@/ui";
import type { Project } from "../api";
import { statusLabel, statusTone } from "../labels";
import { percentRaised, projectDates } from "../view";
import type { ProjectStart } from "./ProjectDialogs";
import styles from "./ProjectList.module.css";

type Props = {
  list: Paged<Project>;
  filtered: boolean;
  everything: boolean;
  onAction: (project: Project, start: ProjectStart) => void;
};

// What was raised, with a bar showing how near the target it is.
function Raised({ project }: { project: Project }) {
  const percent = percentRaised(project);
  return (
    <div className={styles.raised}>
      <span>{formatMoney(project.raisedAmount)} raised</span>
      <div className={styles.progress}>
        <ProgressBar
          value={percent / 100}
          label={`${project.name}: raised against target`}
        />
        <span className={styles.percent}>{percent}%</span>
      </div>
    </div>
  );
}

// A project's name with the category it is filed under, so the first column says what it is at a glance.
function Name({ project }: { project: Project }) {
  return (
    <span className={styles.name}>
      <strong>{project.name}</strong>
      <span className={styles.category}>{project.categoryName}</span>
    </span>
  );
}

const empty = (filtered: boolean, everything: boolean) =>
  filtered
    ? {
        icon: "search" as const,
        title: "No projects found",
        text: "Nothing matches these filters. Try a wider date range, or reset the filters.",
      }
    : everything
      ? {
          icon: "projects" as const,
          title: "Start your first project",
          text: "A project is something the church is raising money for, like a roof, a camp or a sound system. Set a target, then record what comes in.",
        }
      : {
          icon: "projects" as const,
          title: "No projects in this period",
          text: "Choose a wider date range to see earlier projects.",
        };

// Every project in the period, newest start first. A table on a wide screen, a card for each on a phone. A row opens the
// project's page; the ⋯ menu goes straight to editing or deleting.
export function ProjectList({ list, filtered, everything, onAction }: Props) {
  return (
    <RecordTable
      label="Projects"
      list={list}
      rowKey={(p) => p.id}
      large
      href={(p) => paths.project(p.id)}
      rowName={(p) => p.name}
      actions={(p) => [
        { label: "Edit project", onSelect: () => onAction(p, "edit") },
        {
          label: "Delete project",
          danger: true,
          onSelect: () => onAction(p, "delete"),
        },
      ]}
      noun={["project", "projects"]}
      summary=". Sorted by start date."
      empty={empty(filtered, everything)}
      columns={[
        { header: "Project", cell: (p) => <Name project={p} /> },
        { header: "Manager", cell: (p) => p.managerName },
        {
          header: "Status",
          cell: (p) => (
            <Chip tone={statusTone[p.status]}>{statusLabel[p.status]}</Chip>
          ),
        },
        { header: "Raised", cell: (p) => <Raised project={p} />, full: true },
        { header: "Dates", cell: (p) => projectDates(p), full: true },
      ]}
    />
  );
}
