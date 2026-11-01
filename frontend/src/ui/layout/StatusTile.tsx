import { cssVars } from "../hooks/cssVars";
import { Icon, type IconName } from "./Icon";
import styles from "./StatusTile.module.css";

type Props = {
  icon: IconName;
  // Filled: the thing it says has happened (a tick). Quiet otherwise.
  on?: boolean;
  // Red icon, for something that went wrong.
  tone?: "neg";
  size?: number;
};

// A rounded square with one icon, saying what state something is in. When the icon changes, the new one
// springs in from small, so the change is seen.
export function StatusTile({ icon, on, tone, size = 52 }: Props) {
  return (
    <span
      className={styles.tile}
      data-on={on || undefined}
      data-tone={tone}
      style={cssVars({ "--size": `${size}px` })}
      aria-hidden="true"
    >
      <Icon key={icon} name={icon} size={Math.round(size * 0.46)} />
    </span>
  );
}
