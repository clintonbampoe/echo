import styles from "./Avatar.module.css";
import { initials } from "./initials";

type Props = {
  name: string;
  size?: number;
  // A rounded square for a church, a circle for a person.
  shape?: "circle" | "square";
};

// Decorative: the name is always written out next to it.
export function Avatar({ name, size = 32, shape = "circle" }: Props) {
  return (
    <span
      aria-hidden="true"
      className={`${styles.avatar} ${shape === "square" ? styles.square : ""}`}
      style={{ width: size, height: size, fontSize: Math.round(size * 0.38) }}
    >
      {initials(name)}
    </span>
  );
}
