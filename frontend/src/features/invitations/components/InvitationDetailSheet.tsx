import { useState } from "react";
import { roleLabel } from "@/features/account";
import { formatDate } from "@/lib/format";
import { Button, DetailGroup, Icon, Sheet } from "@/ui";
import type { Invitation } from "../api/invitations";
import { useInvitation } from "../hooks";
import { joinLink, statusLabel, titleOf } from "../model/display";
import styles from "./InvitationDetailSheet.module.css";

type Props = { id: string; onClose: () => void };
type Copied = "code" | "link" | null;

// One invitation in full, so the admin never has to write the code down: the code and the join link to
// copy, then who it is for, how many have joined and when it stops working.
export function InvitationDetailSheet({ id, onClose }: Props) {
  const query = useInvitation(id);
  const [copied, setCopied] = useState<Copied>(null);
  const invitation = query.data;

  async function copy(what: "code" | "link", text: string) {
    await navigator.clipboard?.writeText(text);
    setCopied(what);
  }

  return (
    <Sheet
      open
      onClose={onClose}
      title="Invitation"
      subtitle={invitation ? titleOf(invitation) : undefined}
      footer={
        invitation?.code ? (
          <CopyButtons code={invitation.code} copied={copied} onCopy={copy} />
        ) : undefined
      }
    >
      <div className={styles.body}>
        {query.isPending && <p className={styles.note}>Loading…</p>}
        {query.isError && (
          <p className={styles.note}>
            We could not load this invitation. It may have been deleted.
          </p>
        )}
        {invitation && <Detail invitation={invitation} />}
      </div>
    </Sheet>
  );
}

type CopyProps = {
  code: string;
  copied: Copied;
  onCopy: (what: "code" | "link", text: string) => Promise<void>;
};

function CopyButtons({ code, copied, onCopy }: CopyProps) {
  return (
    <div className={styles.footer}>
      <Button
        variant="secondary"
        onClick={() => void onCopy("link", joinLink(code))}
      >
        <Icon name={copied === "link" ? "check" : "copy"} size={16} />
        {copied === "link" ? "Link copied" : "Copy link"}
      </Button>
      <Button onClick={() => void onCopy("code", code)}>
        <Icon name={copied === "code" ? "check" : "copy"} size={16} />
        {copied === "code" ? "Copied" : "Copy code"}
      </Button>
    </div>
  );
}

function Detail({ invitation }: { invitation: Invitation }) {
  const { code, maxUses, useCount, expiresAt } = invitation;
  return (
    <>
      {code ? (
        <>
          <p className={styles.code} aria-label="Invitation code">
            {code}
          </p>
          <p className={styles.link}>{joinLink(code)}</p>
        </>
      ) : (
        <p className={styles.note}>
          This invitation was made before codes were kept, so its code cannot be
          shown. Create a new invitation to get one you can copy.
        </p>
      )}
      <DetailGroup
        title="Details"
        rows={[
          ["Status", statusLabel[invitation.status]],
          ["Role", roleLabel(invitation.allowedRole)],
          [
            "People joined",
            maxUses ? `${useCount} of ${maxUses}` : String(useCount),
          ],
          ["Expires", expiresAt ? formatDate(expiresAt.slice(0, 10)) : "Never"],
          ["Created by", invitation.createdByName],
          ["Created on", formatDate(invitation.createdAt.slice(0, 10))],
        ]}
      />
    </>
  );
}
