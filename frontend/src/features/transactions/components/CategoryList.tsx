import type { Paged } from "@/lib/paged";
import { EmptyState, IconTile, RecordLines } from "@/ui";
import type { Category } from "../api";
import { usedBy } from "../categories";

export type CategoryStart = "edit" | "delete";

type Props = {
  list: Paged<Category>;
  // Income or Expense: which of the two is on show.
  type: string;
  onOpen: (c: Category, start: CategoryStart) => void;
};

// The categories of one type, each with how many transactions are filed under it. There is no detail panel: a
// category is a name and a type, so a row opens the editor and the ⋯ menu has the rest.
export function CategoryList({ list, type, onOpen }: Props) {
  if (list.rows.length === 0 && !list.loading)
    return (
      <EmptyState
        icon="finance"
        title={`No ${type === "Income" ? "income" : "expense"} categories yet`}
      >
        Create a category, then file money in and money out under it.
      </EmptyState>
    );
  return (
    <RecordLines
      label="Categories"
      list={list}
      rowKey={(c) => String(c.id)}
      line={(c) => ({
        lead: <IconTile icon="finance" />,
        title: c.name,
        sub: usedBy(c.transactionCount),
      })}
      onOpen={(c) => onOpen(c, "edit")}
      actions={(c) => ({
        label: `Actions for ${c.name}`,
        title: c.name,
        items: [
          { label: "Edit category", onSelect: () => onOpen(c, "edit") },
          {
            label: "Delete category",
            danger: true,
            separated: true,
            onSelect: () => onOpen(c, "delete"),
          },
        ],
      })}
      noun={["category", "categories"]}
    />
  );
}
