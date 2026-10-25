// What the project form accepts, and the words shown when it does not. The server has the final say; this catches
// the obvious mistakes first. No React in this file.
import { z } from "zod";
import { required } from "@/lib/validation";
import type { Option } from "@/ui";
import type { Project, ProjectInput, ProjectUpdate } from "../api/projects";
import type { ProjectCategory } from "../api/categories";
import { projectStatuses } from "../model/filters";
import { statusLabel } from "../model/labels";

export const NAME_MAX = 100;
export const DESCRIPTION_MAX = 2000;
export const TARGET_MAX = 1_000_000;

const TARGET_RANGE = "Enter a target between 0.01 and 1,000,000";

// The first thing wrong with a target as typed, or nothing. Commas are allowed ("80,000").
function targetProblem(text: string): string | null {
  if (text === "") return "Enter the target";
  const plain = text.replace(/,/g, "");
  if (!/^\d+(\.\d{1,2})?$/.test(plain)) return TARGET_RANGE;
  const value = Number(plain);
  return value < 0.01 || value > TARGET_MAX ? TARGET_RANGE : null;
}

// An end date already set can be changed, not removed: the API has no way to clear it.
export const projectSchema = (endLocked = false) =>
  z
    .object({
      name: required("Enter a project name").max(
        NAME_MAX,
        `Use ${NAME_MAX} characters or fewer`,
      ),
      categoryId: z.string().min(1, "Choose a category"),
      managerId: z.string().min(1, "Choose a manager"),
      targetAmount: z
        .string()
        .trim()
        .superRefine((v, ctx) => {
          const message = targetProblem(v);
          if (message) ctx.addIssue({ code: "custom", message });
        }),
      status: z.enum(projectStatuses as [string, ...string[]]),
      startDate: z.string().min(1, "Enter the start date"),
      endDate: z.string(),
      description: z
        .string()
        .max(DESCRIPTION_MAX, `Use ${DESCRIPTION_MAX} characters or fewer`),
    })
    .superRefine((v, ctx) => {
      const say = (message: string) =>
        ctx.addIssue({ code: "custom", path: ["endDate"], message });
      if (v.startDate && v.endDate && v.endDate < v.startDate)
        say("The end date cannot be before the start date");
      else if (endLocked && !v.endDate)
        say("An end date can be changed, not removed");
    });

export type ProjectValues = z.infer<ReturnType<typeof projectSchema>>;

// A new project starts in planning, with everything else to fill in.
export const emptyProject: ProjectValues = {
  name: "",
  categoryId: "",
  managerId: "",
  targetAmount: "",
  status: "Planning",
  startDate: "",
  endDate: "",
  description: "",
};

export function fromProject(p: Project): ProjectValues {
  return {
    name: p.name,
    categoryId: String(p.categoryId),
    managerId: p.managerId,
    targetAmount: p.targetAmount.toLocaleString("en-US", {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    }),
    status: p.status,
    startDate: p.startDate.slice(0, 10),
    endDate: p.endDate?.slice(0, 10) ?? "",
    description: p.description ?? "",
  };
}

export function toProjectInput(v: ProjectValues): ProjectInput {
  return {
    categoryId: Number(v.categoryId),
    managerId: v.managerId,
    name: v.name.trim(),
    targetAmount: Number(v.targetAmount.replace(/,/g, "")),
    status: v.status as ProjectInput["status"],
    startDate: v.startDate,
    endDate: v.endDate || undefined,
    description: v.description.trim() || undefined,
  };
}

// Saving changes: an emptied description is sent as empty text, because leaving it out would keep the old one.
export const toProjectUpdate = (v: ProjectValues): ProjectUpdate => ({
  ...toProjectInput(v),
  description: v.description.trim(),
});

export const categoryChoices = (
  categories: ProjectCategory[] | undefined,
): readonly Option[] =>
  (categories ?? []).map((c) => ({ value: String(c.id), label: c.name }));

export const statusChoices: readonly Option[] = projectStatuses.map((s) => ({
  value: s,
  label: statusLabel[s],
}));

// The API reports "categoryId", "targetAmount" and so on; the form uses the same names.
export const projectFieldMap = Object.fromEntries(
  Object.keys(emptyProject).map((k) => [k, k]),
) as Record<keyof ProjectValues, keyof ProjectValues>;
