// The Project categories page: how a category is named and counted, where its projects can move to, and what the form
// accepts. No React in this file.
import { z } from "zod";
import type { Option } from "@/ui";
import type { ProjectCategory } from "./api";

export const CATEGORY_NAME_MAX = 100;

// "1 project", "3 projects".
export const plural = (n: number, one: string, many = `${one}s`) =>
  `${n} ${n === 1 ? one : many}`;

export const usedBy = (n: number) => plural(n, "project");

// Where a category's projects can move to: any other category.
export const mergeTargets = (
  categories: ProjectCategory[],
  source: ProjectCategory,
): readonly Option[] =>
  categories
    .filter((c) => c.id !== source.id)
    .map((c) => ({ value: String(c.id), label: c.name }));

export const categorySchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, "Enter a name")
    .max(
      CATEGORY_NAME_MAX,
      `Keep the name to ${CATEGORY_NAME_MAX} characters or fewer`,
    ),
});

export type CategoryValues = z.infer<typeof categorySchema>;

export const emptyCategory: CategoryValues = { name: "" };

// The API reports "name"; the form uses the same name.
export const categoryFieldMap = { name: "name" } as const;
