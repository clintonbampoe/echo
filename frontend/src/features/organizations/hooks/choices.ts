import { ALL_TIME } from "@/lib/dates";
import { useOrganizations } from ".";

// Every organization's id and name, for a filter or a picker on another feature's page.
// (The list is by the day an organization was created, so it asks for all time.)
export function useOrganizationChoices(): { id: string; name: string }[] {
  return useOrganizations(ALL_TIME).rows.map((o) => ({
    id: o.id,
    name: o.name,
  }));
}
