import type { Paged } from "@/lib/paged";
import { Chip, RecordTable } from "@/ui";
import type { Category } from "../api";
import { usedBy } from "../categories";
import { typeLabel, typeTone } from "../labels";

export type CategoryStart = "edit" | "delete";

type Props = {
  list: Paged<Category>;
  filtered: boolean;
  onOpen: (c: Category, start: CategoryStart) => void;
};

// Every category with what it is for and how many transactions are filed under it. A table on a wide screen, a card
// for each on a phone. There is no detail panel: a category is a name and a type, so the ⋯ menu is all it needs.
export function CategoryList({ list, filtered, onOpen }: Props) {
  return (
    <RecordTable
      label="Categories"
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
      empty={
        filtered
          ? {
              icon: "search",
              title: "No categories of this type",
              text: "Choose All to see every category, or create one of this type.",
            }
          : {
              icon: "finance",
              title: "No categories yet",
              text: "Create a category, then file money in and money out under it.",
            }
      }
      columns={[
        { header: "Name", cell: (c) => c.name },
        {
          header: "Type",
          cell: (c) => (
            <Chip tone={typeTone[c.categoryType]}>
              {typeLabel[c.categoryType]}
            </Chip>
          ),
        },
        { header: "Used by", cell: (c) => usedBy(c.transactionCount) },
      ]}
    />
  );
}
