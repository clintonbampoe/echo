// The fixed choices for a member, used by the list's filters and by the form.
import { plainOptions } from "@/lib/options";

export const statusChoices = plainOptions([
  "Active",
  "Inactive",
  "Archived",
  "Transferred",
]);
export const genderChoices = plainOptions(["Male", "Female", "Other"]);
export const maritalChoices = plainOptions(["Single", "Married", "Widowed"]);
