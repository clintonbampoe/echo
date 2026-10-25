import { useState } from "react";
import { useUrlFilters } from "@/lib/useUrlFilters";
import { useUrlFlag } from "@/lib/useUrlFlag";
import { ListBody, PageHeader, Presence, StatStrip } from "@/ui";
import { NewMemberSheet } from "../components/form/NewMemberSheet";
import {
  MemberActionHost,
  type MemberAction,
} from "../components/dialogs/MemberActionHost";
import { MemberFilterBar } from "../components/list/MemberFilterBar";
import { MemberList } from "../components/list/MemberList";
import { useMemberFigures, useMembers } from "../hooks";
import { memberFilters, toApiFilters, windowOf } from "../model/filters";

// The members list: header, numbers, filters and the rows. The page only decides what goes where.
export function MembersPage() {
  const { filters, update, clear, resetKey } = useUrlFilters(memberFilters);
  const [adding, setAdding] = useUrlFlag("new");
  const [action, setAction] = useState<MemberAction | null>(null);

  const query = toApiFilters(filters);
  const list = useMembers(query);
  const figures = useMemberFigures(query, windowOf(filters));

  return (
    <div className="anim-rise">
      <PageHeader
        title="Members"
        add={{ label: "Add member", onClick: () => setAdding(true) }}
      />
      <StatStrip
        label="Member summary"
        stats={figures.stats}
        busy={figures.busy}
      />
      <MemberFilterBar
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
        footerInside
      >
        {(members, footer) => (
          <MemberList members={members} footer={footer} onAct={setAction} />
        )}
      </ListBody>
      <MemberActionHost action={action} onClose={() => setAction(null)} />
      <Presence open={adding}>
        <NewMemberSheet onClose={() => setAdding(false)} />
      </Presence>
    </div>
  );
}
