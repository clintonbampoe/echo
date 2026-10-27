// The fixed choices for an organization's members list. The first means "no filter", so it is the one the list opens on.
import { type Option } from "@/ui";
import { plainOptions, withAny } from "@/lib/options";
import { roles } from "./display";

export const roleOptions: readonly Option[] = withAny(
  "Role",
  plainOptions(roles),
);
