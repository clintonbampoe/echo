import { Link } from "react-router-dom";
import { formatNumber } from "@/lib/format";
import { paths } from "@/lib/paths";
import { ProgressBar } from "@/ui";
import type { Organization } from "../../api/organizations";
import styles from "./OrganizationList.module.css";

type Props = { organizations: Organization[] };

// One card per organization: name, how many members, and a bar against the biggest group in view. The whole card
// opens it; there is no menu here, because an organization is edited and deleted on its own page.
export function OrganizationList({ organizations }: Props) {
  const biggest = Math.max(1, ...organizations.map((o) => o.memberCount));
  return (
    <ul aria-label="Organizations" className={styles.grid}>
      {organizations.map((o) => (
        <li key={o.id} className={styles.card}>
          <Link to={paths.organization(o.id)} className={styles.name}>
            {o.name}
          </Link>
          <p className={styles.text}>{o.description}</p>
          <p className={styles.count}>
            {formatNumber(o.memberCount)}
            <span className={styles.unit}>
              {o.memberCount === 1 ? " member" : " members"}
            </span>
          </p>
          <ProgressBar
            value={o.memberCount / biggest}
            label={`${o.name}: size against the largest group`}
          />
        </li>
      ))}
    </ul>
  );
}
