import { useState } from "react";
import { plainList } from "@/lib/paged";
import { paths } from "@/lib/paths";
import { BackLink, PageHeader } from "@/ui";
import type { ProjectCategory } from "../api/categories";
import { useProjectCategories } from "../hooks/queries";
import { ProjectCategoryDialogs } from "../components/categories/ProjectCategoryDialogs";
import {
  ProjectCategoryList,
  type CategoryStart,
} from "../components/categories/ProjectCategoryList";
import { ProjectCategorySheet } from "../components/categories/ProjectCategorySheet";

type Open =
  { kind: "new" } | { kind: CategoryStart; category: ProjectCategory } | null;

// What projects are grouped under: a name, and how many projects use it.
export function ProjectCategoriesPage() {
  const categories = useProjectCategories();
  const [open, setOpen] = useState<Open>(null);
  const close = () => setOpen(null);

  return (
    <div className="anim-rise">
      <BackLink to={paths.projects}>Projects</BackLink>
      <PageHeader
        title="Project categories"
        add={{ label: "New category", onClick: () => setOpen({ kind: "new" }) }}
      />
      <ProjectCategoryList
        list={plainList(categories.data ?? [], categories)}
        onOpen={(category, kind) => setOpen({ kind, category })}
      />
      {open?.kind === "new" && <ProjectCategorySheet onClose={close} />}
      {open && open.kind !== "new" && (
        <ProjectCategoryDialogs
          category={open.category}
          start={open.kind}
          categories={categories.data ?? []}
          onClose={close}
        />
      )}
    </div>
  );
}
