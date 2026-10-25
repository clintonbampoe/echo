// What the organization form accepts, and the words shown when it does not. The server has the final
// say; this catches the obvious mistakes before a round trip. No React in this file.
import { z } from "zod";
import { required } from "@/lib/validation";
import type { Organization, OrganizationInput } from "../api/organizations";

export const NAME_MAX = 100;
export const DESCRIPTION_MAX = 2000;

// Only a name is needed. The description may be left empty.
export const organizationSchema = z.object({
  name: required("Enter a name").max(
    NAME_MAX,
    `Use ${NAME_MAX} characters or fewer`,
  ),
  description: z
    .string()
    .max(DESCRIPTION_MAX, `Use ${DESCRIPTION_MAX} characters or fewer`),
});

export type OrganizationValues = z.infer<typeof organizationSchema>;

export const emptyOrganization: OrganizationValues = {
  name: "",
  description: "",
};

// The form holds text for every field; the API wants the description left out when empty.
export function toInput(v: OrganizationValues): OrganizationInput {
  return {
    name: v.name.trim(),
    description: v.description.trim() || undefined,
  };
}

// Fills the edit form from an organization the server sent. A missing description becomes empty text.
export function fromOrganization(o: Organization): OrganizationValues {
  return { name: o.name, description: o.description ?? "" };
}

// The API reports a problem with "name"; the form calls that field "name" too.
export const fieldMap = Object.fromEntries(
  Object.keys(emptyOrganization).map((k) => [k, k]),
) as Record<keyof OrganizationValues, keyof OrganizationValues>;
