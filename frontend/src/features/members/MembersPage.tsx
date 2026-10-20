import { useState } from "react";
import { useUrlFilters } from "@/lib/useUrlFilters";
import { Button, Icon, ListBody, PageHeader } from "@/ui";
import type { Member } from "./api";
import { DeleteMemberDialog } from "./DeleteMemberDialog";
import { EditMemberSheet } from "./EditMemberSheet";
import { FilterBar } from "./FilterBar";
import { memberFilters, toApiFilters } from "./filters";
import { useMembers, useMemberSummary } from "./hooks";
import { MemberList } from "./MemberList";
import { MemberSheet } from "./MemberSheet";
import { SummaryStrip } from "./SummaryStrip";
import { useUrlFlag } from "@/lib/useUrlFlag";

// The members list: header, numbers, filters and the rows. The page only decides what a row is.
export function MembersPage() {
  const { filters, update, clear, resetKey } = useUrlFilters(memberFilters);
  const [adding, setAdding] = useUrlFlag("new");
  const [editing, setEditing] = useState<Member | null>(null);
  const [deleting, setDeleting] = useState<Member | null>(null);

  const api = toApiFilters(filters);
  const list = useMembers(api);
  const summary = useMemberSummary(api);

  return (
    <div className="anim-rise">
      <PageHeader
        title="Members"
        text="Everyone in your church, in one place."
        action={
          <Button onClick={() => setAdding(true)}>
            <Icon name="plus" size={16} />
            New member
          </Button>
        }
      />
      <SummaryStrip summary={summary.data} />
      <FilterBar
        filters={filters}
        onChange={update}
        resetKey={resetKey}
        onClear={clear}
      />
      <ListBody
        list={list}
        icon="user"
        noun={["member", "members"]}
        emptyText="Members you add will appear here."
        filtered={memberFilters.isFiltered(filters)}
        onClear={clear}
      >
        {(members) => (
          <MemberList
            members={members}
            onEdit={setEditing}
            onDelete={setDeleting}
          />
        )}
      </ListBody>
      <MemberSheet open={adding} onClose={() => setAdding(false)} />
      {editing && (
        <EditMemberSheet member={editing} onClose={() => setEditing(null)} />
      )}
      {deleting && (
        <DeleteMemberDialog
          member={deleting}
          onClose={() => setDeleting(null)}
        />
      )}
    </div>
  );
}
