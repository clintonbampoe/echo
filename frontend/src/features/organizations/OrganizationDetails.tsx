import { formatMonth, formatNumber } from "@/lib/format";
import { paths } from "@/lib/paths";
import { DashCard, FactList, PersonList } from "@/ui";
import type { Organization, OrganizationMember } from "./api";
import { roleLabel } from "./labels";

type Props = { organization: Organization; leaders: OrganizationMember[] };

// The Details tab: what the group is, then the people who lead it. Everyone else is on the Members tab.
export function OrganizationDetails({ organization: o, leaders }: Props) {
  return (
    <>
      <FactList
        rows={[
          ["Formed", formatMonth(o.createdAt)],
          ["Members", formatNumber(o.memberCount)],
          ["Description", o.description],
        ]}
      />
      {leaders.length > 0 && (
        <DashCard title="Leadership">
          <PersonList
            label="Leadership"
            rows={leaders}
            rowKey={(m) => m.id}
            person={(m) => ({
              name: m.memberName,
              detail: roleLabel[m.role],
              href: paths.member(m.memberId),
            })}
            columns={[]}
            compact
            flat
          />
        </DashCard>
      )}
    </>
  );
}
