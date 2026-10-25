import { useState } from "react";
import { plainList } from "@/lib/paged";
import { paths } from "@/lib/paths";
import { BackLink, PageHeader } from "@/ui";
import type { AssetCategory } from "../api/categories";
import { AssetCategoryDialogs } from "../components/categories/AssetCategoryDialogs";
import {
  AssetCategoryList,
  type CategoryStart,
} from "../components/categories/AssetCategoryList";
import { AssetCategorySheet } from "../components/categories/AssetCategorySheet";
import { useAssetCategories } from "../hooks/queries";

type Open =
  { kind: "new" } | { kind: CategoryStart; category: AssetCategory } | null;

// What assets are grouped under: a name, and how many assets use it.
export function AssetCategoriesPage() {
  const categories = useAssetCategories();
  const [open, setOpen] = useState<Open>(null);
  const close = () => setOpen(null);

  return (
    <div className="anim-rise">
      <BackLink to={paths.assets}>Assets</BackLink>
      <PageHeader
        title="Asset categories"
        add={{ label: "New category", onClick: () => setOpen({ kind: "new" }) }}
      />
      <AssetCategoryList
        list={plainList(categories.data ?? [], categories)}
        onOpen={(category, kind) => setOpen({ kind, category })}
      />
      {open?.kind === "new" && <AssetCategorySheet onClose={close} />}
      {open && open.kind !== "new" && (
        <AssetCategoryDialogs
          category={open.category}
          start={open.kind}
          categories={categories.data ?? []}
          onClose={close}
        />
      )}
    </div>
  );
}
