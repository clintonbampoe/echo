import type { Paged } from "@/lib/paged";
import { RecordTable } from "@/ui";
import { formatNumber } from "@/lib/format";
import type { AssetCategory } from "../../api/categories";
import { usedBy } from "../../model/categories";
import { CategoryMark } from "./CategoryMark";

export type CategoryStart = "edit" | "delete";

type Props = {
  list: Paged<AssetCategory>;
  onOpen: (c: AssetCategory, start: CategoryStart) => void;
};

// Every category with how many assets are filed under it. A table on a wide screen, a card for each on a phone. There
// is no detail panel: a category is only a name, so the ⋯ menu is all it needs.
export function AssetCategoryList({ list, onOpen }: Props) {
  return (
    <RecordTable
      label="Asset categories"
      title="Categories"
      aside={`${formatNumber(list.rows.length)} ${list.rows.length === 1 ? "category" : "categories"} · ${usedBy(list.rows.reduce((sum, c) => sum + c.assetCount, 0))}`}
      list={list}
      line={(c) => ({
        lead: <CategoryMark name={c.name} />,
        title: c.name,
        sub: usedBy(c.assetCount),
      })}
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
        icon: "assets",
        title: "No categories yet",
        text: "Create a category, then file assets under it.",
      }}
      columns={[
        { header: "Name", cell: (c) => c.name },
        { header: "Used by", cell: (c) => usedBy(c.assetCount) },
      ]}
    />
  );
}
