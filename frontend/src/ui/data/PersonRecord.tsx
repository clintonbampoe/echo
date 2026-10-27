import type { ReactNode } from "react";
import { Link } from "react-router-dom";
import { Avatar } from "../layout/Avatar";
import { Icon } from "../layout/Icon";
import { FactList } from "../layout/FactList";
import styles from "./PersonRecord.module.css";

type Props = {
  name: string;
  // Under the name, in words: "Member · Present".
  meta: ReactNode;
  rows: [label: string, value: string | null | undefined][];
  // The person's own page, and which tab of it the record is on: "Abena Mensah · opens the Events tab".
  profileHref: string;
  profileNote: string;
  children?: ReactNode;
};

// What a record about a person looks like when it opens in a panel: who it is, the facts as label and value lines,
// and the one deliberate way out to the person's own page (design rule S6). The buttons go in the Sheet's footer.
export function PersonRecord({
  name,
  meta,
  rows,
  profileHref,
  profileNote,
  children,
}: Props) {
  return (
    <div className={styles.body}>
      <div className={styles.who}>
        <Avatar name={name} size={56} />
        <div className={styles.names}>
          <p className={styles.name}>{name}</p>
          <p className={styles.meta}>{meta}</p>
        </div>
      </div>
      <FactList rows={rows} />
      <Link to={profileHref} className={styles.profile}>
        <Avatar name={name} size={40} />
        <span className={styles.text}>
          <span className={styles.link}>View profile</span>
          <span className={styles.note}>{profileNote}</span>
        </span>
        <Icon name="chevron-right" size={16} />
      </Link>
      {children}
    </div>
  );
}
