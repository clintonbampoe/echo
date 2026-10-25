import { formatDate } from "@/lib/format";
import { DetailCard, DetailGroup } from "@/ui";
import type { Project } from "../../api/projects";
import { statusLabel } from "../../model/labels";
import styles from "./ProjectDetails.module.css";

// Who looks after the project, what it is filed under, when it runs and where it stands.
export function ProjectDetails({ project: p }: { project: Project }) {
  return (
    <DetailCard label="Project details">
      <DetailGroup
        title="Details"
        rows={[
          ["Manager", p.managerName],
          ["Category", p.categoryName],
          ["Starts", formatDate(p.startDate)],
          ["Ends", p.endDate ? formatDate(p.endDate) : "Not set"],
          ["Status", statusLabel[p.status]],
        ]}
      />
      <p className={styles.note}>
        Status is set by hand when you edit the project. It does not change on
        its own.
      </p>
    </DetailCard>
  );
}
