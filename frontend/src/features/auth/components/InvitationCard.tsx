import { regionLabel } from "../options";
import type { InvitationInfo } from "../api";
import styles from "./InvitationCard.module.css";

// "You have been invited to join: Grace Chapel, Osu, Greater Accra, Member".
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
      </div>
      <span className={styles.role}>{invitation.allowedRole}</span>
    </div>
  );
}
