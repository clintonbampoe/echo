// The fixed choices for a member, used by the list's filters and by the form.
import { plainOptions, withAny } from "@/lib/options";
import type { Gender } from "../api/members";

export const statusChoices = plainOptions([
  "Active",
  "Inactive",
  "Archived",
  "Transferred",
]);
export const genderChoices: { value: Gender; label: string }[] = [
  { value: "Male", label: "Male" },
  { value: "Female", label: "Female" },
  { value: "NonBinary", label: "Non-binary" },
];
export const genderLabel = (gender: string) =>
  genderChoices.find((g) => g.value === gender)?.label ?? gender;
export const maritalChoices = plainOptions(["Single", "Married", "Widowed"]);

// For the form, where these may be left unset.
export const optionalMaritalChoices = withAny("Not set", maritalChoices);
