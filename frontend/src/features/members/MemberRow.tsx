import { Link } from "react-router-dom";
import { formatDate } from "@/lib/format";
import { regionLabel } from "@/lib/regions";
import { ActionMenu, Avatar, Chip } from "@/ui";
import type { Member } from "./api";
import { statusTone } from "./labels";
import styles from "./MemberList.module.css";

// One member. A table row on a wide screen, a card on a phone. The same markup both times,
// and the layout never depends on the data inside it (design rule S1).
type Props = {
  member: Member;
  onEdit: (m: Member) => void;
  onDelete: (m: Member) => void;
};

export function MemberRow({ member: m, onEdit, onDelete }: Props) {
  return (
    <li className={styles.row}>
      <div className={styles.who}>
        <Avatar name={m.name} size={36} />
        <div className={styles.names}>
          <p className={styles.name}>
            <Link to={`/members/${m.id}`} className={styles.link}>
              {m.name}
            </Link>
          </p>
          <p className={styles.email}>{m.emailAddress || "No email"}</p>
        </div>
      </div>
      <p className={`${styles.cell} ${styles.phone}`} data-label="Phone">
        {m.phoneNumber}
      </p>
      <p className={`${styles.cell} ${styles.place}`} data-label="Location">
        {m.city}, {regionLabel(m.region)}
      </p>
      <p className={`${styles.cell} ${styles.gender}`} data-label="Gender">
        {m.gender}
      </p>
      <div className={styles.status}>
        <Chip tone={statusTone[m.status]}>{m.status}</Chip>
      </div>
      <p className={`${styles.cell} ${styles.joined}`} data-label="Joined">
        {m.joinedDate ? formatDate(m.joinedDate) : "–"}
      </p>
      <div className={styles.actions}>
        <ActionMenu
          label={`Actions for ${m.name}`}
          title={m.name}
          items={[
            { label: "Edit", onSelect: () => onEdit(m) },
            { label: "Delete", danger: true, onSelect: () => onDelete(m) },
          ]}
        />
      </div>
    </li>
  );
}
