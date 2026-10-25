import type { Paged } from "@/lib/paged";
import { RecordTable } from "@/ui";
import type { ProjectCategory } from "../../api/categories";
import { usedBy } from "../../model/categories";

export type CategoryStart = "edit" | "delete";

type Props = {
  list: Paged<ProjectCategory>;
  onOpen: (c: ProjectCategory, start: CategoryStart) => void;
};

// Every category with how many projects are filed under it. A table on a wide screen, a card for each on a phone.
// There is no detail panel: a category is only a name, so the ⋯ menu is all it needs.
export function ProjectCategoryList({ list, onOpen }: Props) {
  return (
    <RecordTable
      label="Project categories"
      title="Project categories"
      list={list}
      rowKey={(c) => String(c.id)}
      large
      rowName={(c) => c.name}
      actions={(c) => [
        { label: "Edit category", onSelect: () => onOpen(c, "edit") },
        {
          label: "Delete category",
          danger: true,
          separated: true,
          onSelect: () => onOpen(c, "delete"),
        },
      ]}
      noun={["category", "categories"]}
      empty={{
        icon: "projects",
        title: "No categories yet",
        text: "Create a category, then file projects under it.",
      }}
      columns={[
        { header: "Name", cell: (c) => c.name },
        { header: "Used by", cell: (c) => usedBy(c.projectCount) },
      ]}
    />
  );
}
