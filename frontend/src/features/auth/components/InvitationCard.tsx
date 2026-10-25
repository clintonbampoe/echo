import { regionLabel } from "@/lib/regions";
import type { InvitationInfo } from "../api/auth";
import styles from "./InvitationCard.module.css";

// "Grace Chapel, Osu, Greater Accra" with the church's initials, and who invited you.
export function InvitationCard({ invitation }: { invitation: InvitationInfo }) {
  const initials = invitation.congregationName
    .split(/\s+/)
    .slice(0, 2)
    .map((w) => w.charAt(0).toUpperCase())
    .join("");
  return (
    <div className={styles.card}>
      <span className={styles.avatar} aria-hidden="true">
        {initials}
      </span>
      <div className={styles.text}>
        <p className={styles.name}>{invitation.congregationName}</p>
        <p className={styles.place}>
          {invitation.city}, {regionLabel(invitation.region)}
        </p>
        {invitation.invitedBy && (
          <p className={styles.place}>Invited by {invitation.invitedBy}</p>
        )}
      </div>
    </div>
  );
}
