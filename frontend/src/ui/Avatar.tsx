import styles from "./Avatar.module.css";
import { initials } from "./initials";

type Props = {
  name: string;
  size?: number;
  // A rounded square for a church, a circle for a person.
  shape?: "circle" | "square";
};

// A name always lands on the same one of four soft greys, so a list is not a wall of one colour.
const tint = (name: string) =>
  [...name].reduce((sum, ch) => sum + ch.charCodeAt(0), 0) % 4;

// Decorative: the name is always written out next to it.
export function Avatar({ name, size = 32, shape = "circle" }: Props) {
  return (
    <span
      aria-hidden="true"
      className={`av ${styles.avatar} ${shape === "square" ? styles.square : styles[`tint${tint(name)}`]}`}
      style={{ width: size, height: size, fontSize: Math.round(size * 0.34) }}
    >
      {initials(name)}
    </span>
  );
}
