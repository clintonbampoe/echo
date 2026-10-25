import { useState } from "react";
import { plainList } from "@/lib/paged";
import { paths } from "@/lib/paths";
import { useUrlFilters } from "@/lib/useUrlFilters";
import { BackLink, PageHeader, SegmentedField } from "@/ui";
import type { Category } from "../api";
import { categoriesOfType, categoryFilters } from "../categories";
import { typeOptions } from "../filters";
import { useCategories } from "../hooks";
import styles from "./CategoriesPage.module.css";
import { CategoryList, type CategoryStart } from "./CategoryList";
import { CategorySheet } from "./CategorySheet";
import { CategoryDialogs } from "./CategoryDialogs";

type Open =
  { kind: "new" } | { kind: CategoryStart; category: Category } | null;

// What transactions are filed under. Each category is for income or for expenses, and that is fixed when it is created.
export function CategoriesPage() {
  const categories = useCategories();
  const { filters, update } = useUrlFilters(categoryFilters);
  const [open, setOpen] = useState<Open>(null);
  const close = () => setOpen(null);
  const shown = categoriesOfType(categories.data, filters.type);

  return (
    <div className="anim-rise">
      <BackLink to={paths.finance}>Finance</BackLink>
      <PageHeader
        title="Categories"
        add={{ label: "New category", onClick: () => setOpen({ kind: "new" }) }}
      />
      <div className={styles.switch}>
        <SegmentedField
          label="Show"
          hideLabel
          joined
          fill
          options={typeOptions}
          value={filters.type}
          onChange={(type) => update({ type })}
        />
      </div>
      <CategoryList
        list={plainList(shown, categories)}
        filtered={categoryFilters.isFiltered(filters)}
        onOpen={(category, kind) => setOpen({ kind, category })}
      />
      {open?.kind === "new" && <CategorySheet onClose={close} />}
      {open && open.kind !== "new" && (
        <CategoryDialogs
          category={open.category}
          start={open.kind}
          categories={categories.data ?? []}
          onClose={close}
        />
      )}
    </div>
  );
}
