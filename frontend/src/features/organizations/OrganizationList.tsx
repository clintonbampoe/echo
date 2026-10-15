import { Link } from "react-router-dom";
import { paths } from "@/lib/paths";
import { Icon } from "@/ui";
import type { Organization } from "./api";
import styles from "./OrganizationList.module.css";
import { OrganizationTile } from "./OrganizationTile";
import { memberCountLabel } from "./view";

type Props = { organizations: Organization[] };

// One card per organization. The whole card opens it; there is no menu here, because an organization is
// edited and deleted on its own page.
export function OrganizationList({ organizations }: Props) {
  return (
    <ul aria-label="Organizations" className={styles.grid}>
      {organizations.map((o) => (
        <li key={o.id}>
          <Link to={paths.organization(o.id)} className={styles.card}>
            <span className={styles.head}>
              <OrganizationTile name={o.name} />
              <span className={styles.name}>{o.name}</span>
            </span>
            <span className={styles.text}>{o.description}</span>
            <span className={styles.foot}>
              {memberCountLabel(o.memberCount)}
              <Icon name="arrow-right" size={14} />
            </span>
          </Link>
        </li>
      ))}
    </ul>
  );
}
