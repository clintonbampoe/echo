import { formatMoney } from "@/lib/format";
import { paths } from "@/lib/paths";
import { Chip, RecordTable, Section } from "@/ui";
import { useManagedProjects } from "../hooks";
import { statusLabel, statusTone } from "../labels";

// The projects a person leads, with what is raised against target. A row opens the project.
export function ManagedProjects({ personId }: { personId: string }) {
  return (
    <Section
      title="Projects managed"
      hint="Projects this person leads, with what is raised against target."
    >
      <RecordTable
        label="Projects managed"
        list={useManagedProjects(personId)}
        rowKey={(p) => p.id}
        href={(p) => paths.project(p.id)}
        scroll
        empty={{
          icon: "projects",
          title: "Not managing a project",
          text: "Projects they lead are listed here with what is raised against target.",
        }}
        noun={["project", "projects"]}
        columns={[
          { header: "Project", cell: (p) => p.name },
          {
            header: "Status",
            cell: (p) => (
              <Chip tone={statusTone[p.status]}>{statusLabel[p.status]}</Chip>
            ),
          },
          { header: "Raised", cell: (p) => formatMoney(p.raisedAmount) },
          {
            header: "Target",
            cell: (p) => formatMoney(p.targetAmount),
            end: true,
          },
        ]}
      />
    </Section>
  );
}
