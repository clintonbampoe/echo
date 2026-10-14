import { roleLabel, useCongregation, useMe } from "@/features/account";
import { Avatar } from "@/ui";
import styles from "./ChurchHeader.module.css";

// The church's name and what the signed-in person is allowed to do in it.
export function ChurchHeader() {
  const church = useCongregation().data;
  const me = useMe().data;
  return (
    <div className={styles.header}>
      <Avatar name={church?.name ?? ""} size={32} shape="square" />
      <span className={styles.text}>
        <span className={styles.name}>{church ? church.name : "Loading…"}</span>
        <span className={styles.role}>{me ? roleLabel(me.role) : ""}</span>
      </span>
    </div>
  );
}
