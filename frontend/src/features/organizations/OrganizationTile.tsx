import { initials } from "@/ui/initials";
import styles from "./OrganizationTile.module.css";

type Props = { name: string; size?: number };

// The rounded square with an organization's initials. Decorative: the name is always written next to it.
export function OrganizationTile({ name, size = 44 }: Props) {
  return (
    <span
      aria-hidden="true"
      className={styles.tile}
      style={{ width: size, height: size, fontSize: Math.round(size * 0.34) }}
    >
      {initials(name)}
    </span>
  );
}
