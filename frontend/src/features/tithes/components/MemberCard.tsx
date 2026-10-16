import { paths } from "@/lib/paths";
import { Avatar, LinkButton } from "@/ui";
import styles from "./MemberCard.module.css";

// The member of a tithe that already exists: shown, not editable, with a deliberate way to their page (on the
// Tithes tab). The button is the same one the organization's membership panel uses.
export function MemberCard({ id, name }: { id: string; name: string }) {
  return (
    <div className={styles.card}>
      <Avatar name={name} size={36} />
      <p className={styles.name}>{name}</p>
      <LinkButton to={paths.member(id, "tithes")} variant="secondary" size="sm">
        View profile
      </LinkButton>
    </div>
  );
}
