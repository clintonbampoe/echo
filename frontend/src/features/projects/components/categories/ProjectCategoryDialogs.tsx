import { useState } from "react";
import type { ProjectCategory } from "../../api/categories";
import { DeleteProjectCategoryDialog } from "./DeleteProjectCategoryDialog";
import { MergeProjectCategoryDialog } from "./MergeProjectCategoryDialog";
import type { CategoryStart } from "./ProjectCategoryList";
import { ProjectCategoryInUseDialog } from "./ProjectCategoryInUseDialog";
import { ProjectCategorySheet } from "./ProjectCategorySheet";

type Step = CategoryStart | "merge";

type Props = {
  category: ProjectCategory;
  start: CategoryStart;
  categories: ProjectCategory[];
  onClose: () => void;
};

// Everything that can happen to one existing category: rename it, or delete it. A category in use cannot be deleted,
// so that path says so first and then moves its projects to another category.
export function ProjectCategoryDialogs({
  category,
  start,
  categories,
  onClose,
}: Props) {
  const [step, setStep] = useState<Step>(start);
  if (step === "edit")
    return <ProjectCategorySheet category={category} onClose={onClose} />;
  if (step === "merge")
    return (
      <MergeProjectCategoryDialog
        category={category}
        categories={categories}
        onClose={onClose}
      />
    );
  if (category.projectCount > 0)
    return (
      <ProjectCategoryInUseDialog
        category={category}
        onMove={() => setStep("merge")}
        onClose={onClose}
      />
    );
  return <DeleteProjectCategoryDialog category={category} onClose={onClose} />;
}
