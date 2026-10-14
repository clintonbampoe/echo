import type { ComponentProps } from "react";
import { Icon } from "./Icon";
import styles from "./SearchField.module.css";

type Props = Omit<ComponentProps<"input">, "type" | "aria-label"> & {
  label: string;
};

// A compact search box for the top of a list. The label is for screen readers.
export function SearchField({ label, className, ...input }: Props) {
  return (
    <div className={`${styles.wrap} ${className ?? ""}`}>
      <Icon name="search" size={16} className={styles.icon} />
      <input
        {...input}
        type="search"
        aria-label={label}
        className={styles.input}
      />
    </div>
  );
}
