import { categoryMark } from "../../model/categoryIcons";
import styles from "./CategoryMark.module.css";

// The tile at the left of a category: a mark for its kind, drawn quiet. It is decoration; the name beside it says it all.
export function CategoryMark({ name }: { name: string }) {
  return (
    <span className={`av liquid-av ${styles.tile}`} aria-hidden="true">
      <svg
        width="20"
        height="20"
        viewBox="0 0 20 20"
        fill="none"
        stroke="currentColor"
        strokeWidth="1.6"
        strokeLinecap="round"
        strokeLinejoin="round"
      >
        <path d={categoryMark(name).path} />
      </svg>
    </span>
  );
}
