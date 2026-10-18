import { useState } from "react";
import type { Paged } from "@/lib/paged";
import { paths } from "@/lib/paths";
import { useUrlFilters } from "@/lib/useUrlFilters";
import { BackLink, Button, Icon, PageHeader, SegmentedField } from "@/ui";
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

// The categories come as one plain list, so they are shown as a list with nothing more to load.
function asList(
  rows: Category[],
  q: ReturnType<typeof useCategories>,
): Paged<Category> {
  return {
    rows,
    loading: q.isPending,
    failed: q.isError,
    hasMore: false,
    loadingMore: false,
    more: () => {},
    retry: () => void q.refetch(),
  };
}

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
        text="What your transactions are filed under. Each category is for income or for expenses, and that cannot be changed after it is created."
        action={
          <Button onClick={() => setOpen({ kind: "new" })}>
            <Icon name="plus" size={16} />
            New category
          </Button>
        }
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
        list={asList(shown, categories)}
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
