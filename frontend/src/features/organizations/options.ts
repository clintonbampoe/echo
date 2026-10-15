// The fixed choices for an organization's members list. The first means "no filter", so it is the one the list opens on.
import { plainOptions, withAny, type Option } from "@/ui";
import { roles } from "./view";

export const roleOptions: readonly Option[] = withAny(
  "Role",
  plainOptions(roles),
);
