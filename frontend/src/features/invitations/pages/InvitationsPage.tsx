import { useState } from "react";
import { useUrlFlag } from "@/lib/useUrlFlag";
import { ListBody, PageHeader, Presence, StatStrip } from "@/ui";
import {
  InvitationActionHost,
  type InvitationAction,
} from "../components/InvitationActionHost";
import { InvitationList } from "../components/InvitationList";
import { InviteSheet } from "../components/InviteSheet";
import { useInvitations, useInvitationSummary } from "../hooks";
import { invitationStats } from "../model/figures";
import styles from "./InvitationsPage.module.css";

// Admin only: the invitations people join with. The server turns anyone else away, so a clerk who opens the
// address sees the same "could not load" card as for any failed list.
export function InvitationsPage() {
  const list = useInvitations();
  const summary = useInvitationSummary();
  const [inviting, setInviting] = useUrlFlag("new");
  const [action, setAction] = useState<InvitationAction | null>(null);

  return (
    <div className={`${styles.page} anim-rise`}>
      <PageHeader
        title="Invitations"
        badge="Admin only"
        text="Each code works until it expires, reaches its limit or you switch it off."
        add={{ label: "Invite people", onClick: () => setInviting(true) }}
      />
      <StatStrip
        label="Invitation summary"
        stats={invitationStats(summary.data)}
        busy={summary.isPending}
      />
      <ListBody
        list={list}
        icon="inbox"
        noun={["invitation", "invitations"]}
        emptyText="Invite people with a code and they can create their own account."
        filtered={false}
        onClear={() => {}}
        skeleton={<RowsSkeleton />}
      >
        {(rows) => (
          <InvitationList
            invitations={rows}
            onView={(invitation) => setAction({ kind: "view", invitation })}
            onEdit={(invitation) => setAction({ kind: "edit", invitation })}
            onRevoke={(invitation) => setAction({ kind: "revoke", invitation })}
            onDelete={(invitation) => setAction({ kind: "delete", invitation })}
          />
        )}
      </ListBody>
      <Presence open={inviting}>
        <InviteSheet onClose={() => setInviting(false)} />
      </Presence>
      <InvitationActionHost action={action} onClose={() => setAction(null)} />
    </div>
  );
}

function RowsSkeleton() {
  return (
    <div className={styles.skeleton} aria-hidden="true">
      {[0, 1, 2].map((i) => (
        <span key={i} className="skeleton" />
      ))}
    </div>
  );
}
