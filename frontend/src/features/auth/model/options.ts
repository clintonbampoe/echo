// The kinds of congregation, as a dropdown list (design rule S2). Values are what the API expects.
import type { Option } from "@/ui";

export const congregationTypes: readonly Option[] = [
  { value: "Church", label: "Church" },
  { value: "Mosque", label: "Mosque" },
  { value: "NonProfit", label: "Non-profit" },
  { value: "Other", label: "Other" },
];
