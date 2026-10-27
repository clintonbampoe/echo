import { Link } from "react-router-dom";
import { formatMoney } from "@/lib/format";
import type { Paged } from "@/lib/paged";
import { paths } from "@/lib/paths";
import { ActionMenu, Chip, ListBody, ProgressBar } from "@/ui";
import type { Project } from "../../api/projects";
import { statusLabel, statusTone } from "../../model/labels";
import { percentRaised } from "../../model/display";
import styles from "./ProjectList.module.css";

type Props = {
  list: Paged<Project>;
  filtered: boolean;
  everything: boolean;
  onClear: () => void;
  onEdit: (project: Project) => void;
  onDelete: (project: Project) => void;
};

type Act = Pick<Props, "onEdit" | "onDelete">;

// One project as a card: its name and category, how much of the target is raised, and a bar. The whole card opens it.
function ProjectCard({
  project: p,
  onEdit,
  onDelete,
}: Act & { project: Project }) {
  const percent = percentRaised(p);
  return (
    <li className={styles.card}>
      <div className={styles.top}>
        <div className={styles.who}>
          <Link to={paths.project(p.id)} className={styles.name}>
            {p.name}
          </Link>
          <p className={styles.category}>{p.categoryName}</p>
        </div>
        {p.status !== "OnTrack" && (
          <Chip tone={statusTone[p.status]}>{statusLabel[p.status]}</Chip>
        )}
        <span className={styles.menu}>
          <ActionMenu
            label={`Actions for ${p.name}`}
            title={p.name}
            items={[
              { label: "Edit", onSelect: () => onEdit(p) },
              { label: "Delete", danger: true, onSelect: () => onDelete(p) },
            ]}
          />
        </span>
      </div>
      <p className={styles.percent}>
        {percent}%<span className={styles.funded}> funded</span>
      </p>
      <ProgressBar
        value={Math.min(percent, 100) / 100}
        label={`${p.name}: raised against target`}
      />
      <p className={styles.amounts}>
        <span>{formatMoney(p.raisedAmount)}</span>
        <span>of {formatMoney(p.targetAmount)}</span>
      </p>
    </li>
  );
}

// Every project in the period, newest start first, as a grid of cards.
export function ProjectList({
  list,
  filtered,
  everything,
  onClear,
  onEdit,
  onDelete,
}: Props) {
  return (
    <ListBody
      list={list}
      icon="projects"
      noun={["project", "projects"]}
      emptyTitle={
        everything ? "Start your first project" : "No projects in this period"
      }
      emptyText={
        everything
          ? "A project is something the church is raising money for, like a roof, a camp or a sound system. Set a target, then record what comes in."
          : "Choose a wider date range to see earlier projects."
      }
      noMatchText="Nothing matches these filters. Try a wider date range, or reset the filters."
      filtered={filtered}
      onClear={onClear}
      order="Sorted by start date."
    >
      {(rows) => (
        <ul aria-label="Projects" className={styles.grid}>
          {rows.map((p) => (
            <ProjectCard
              key={p.id}
              project={p}
              onEdit={onEdit}
              onDelete={onDelete}
            />
          ))}
        </ul>
      )}
    </ListBody>
  );
}
