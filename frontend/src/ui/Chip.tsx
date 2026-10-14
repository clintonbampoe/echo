import type { ReactNode } from "react";
import styles from "./Chip.module.css";

// The colours a chip can take. Features map their own statuses onto these (see each feature's labels.ts).
export type ChipTone =
  "neutral" | "positive" | "warning" | "negative" | "strong" | "outline";

type Props = {
  tone?: ChipTone;
  children: ReactNode;
};

// A small label for a status. The words carry the meaning; the colour only backs them up.
export function Chip({ tone = "neutral", children }: Props) {
  return <span className={`${styles.chip} ${styles[tone]}`}>{children}</span>;
}
