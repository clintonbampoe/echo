// The fixed choices for the visitors list. The first means "no filter", so it is the one the list opens on.
import type { Option } from "@/ui";

export const statusOptions: readonly Option[] = [
  { value: "", label: "Not yet members" },
  { value: "became", label: "Became members" },
];
