import { useState } from "react";
import { roleLabel } from "@/features/account";
import { formatDayMonth } from "@/lib/format";
import { Button, Icon, Sheet } from "@/ui";
import type { CreatedInvitation } from "../api/invitations";
import styles from "./InviteCreated.module.css";

type Props = { invitation: CreatedInvitation; onClose: () => void };

// What the person sees once an invitation exists: the code, large, with a Copy button. The server keeps only a
// hash of the code, so this is the one time it can be read.
export function InviteCreated({ invitation, onClose }: Props) {
  const [copied, setCopied] = useState(false);

  async function copy() {
    await navigator.clipboard?.writeText(invitation.token);
    setCopied(true);
  }

  return (
    <Sheet
      open
      onClose={onClose}
      title="Invitation created"
      subtitle={`${roleLabel(invitation.allowedRole)} · expires ${formatDayMonth(invitation.expiresAt)}`}
      footer={
        <div className={styles.footer}>
          <Button variant="secondary" onClick={() => void copy()}>
            <Icon name={copied ? "check" : "copy"} size={16} />
            {copied ? "Copied" : "Copy code"}
          </Button>
          <Button onClick={onClose}>Done</Button>
        </div>
      }
    >
      <p className={styles.code} aria-label="Invitation code">
        {invitation.token}
      </p>
      <p className={styles.note}>
        Send this code to the people you want to join. It will not be shown
        again, so copy it now.
      </p>
    </Sheet>
  );
}
