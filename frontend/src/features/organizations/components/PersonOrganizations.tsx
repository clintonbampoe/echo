import { formatDate } from "@/lib/format";
import { paths } from "@/lib/paths";
import { Chip, RecordTable, Section } from "@/ui";
import { usePersonMemberships } from "../hooks";
import { roleLabel, roleTone } from "../labels";

// The organizations a person belongs to, with their role. People are added from the organization's own
// page, never from here. A row opens that organization.
export function PersonOrganizations({ personId }: { personId: string }) {
  return (
    <Section hint="Ministries, groups and teams this person belongs to.">
      <RecordTable
        label="Organizations"
        title="Organizations"
        list={usePersonMemberships(personId)}
        rowKey={(m) => m.id}
        href={(m) => paths.organization(m.organizationId)}
        large
        empty={{
          icon: "organizations",
          title: "Not in any organization",
          text: "Ministries, groups and teams they belong to appear here. People are added to an organization from its own page.",
        }}
        noun={["organization", "organizations"]}
        columns={[
          { header: "Organization", cell: (m) => m.organizationName },
          {
            header: "Role",
            cell: (m) => (
              <Chip tone={roleTone[m.role]}>{roleLabel[m.role]}</Chip>
            ),
          },
          { header: "Joined", cell: (m) => formatDate(m.joinedAt) },
        ]}
      />
    </Section>
  );
}
