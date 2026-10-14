import type { Member } from "./api";
import { MemberRow } from "./MemberRow";
import styles from "./MemberList.module.css";

type Props = {
  members: Member[];
  onEdit: (m: Member) => void;
  onDelete: (m: Member) => void;
};

export function MemberList({ members, onEdit, onDelete }: Props) {
  return (
    <div className={styles.card}>
      <div className={styles.head} aria-hidden="true">
        <span>Name</span>
        <span className={styles.phone}>Phone</span>
        <span className={styles.place}>Location</span>
        <span className={styles.gender}>Gender</span>
        <span>Status</span>
        <span>Joined</span>
        <span />
      </div>
      <ul aria-label="Members" className={styles.list}>
        {members.map((m) => (
          <MemberRow
            key={m.id}
            member={m}
            onEdit={onEdit}
            onDelete={onDelete}
          />
        ))}
      </ul>
    </div>
  );
}
