import styles from "./Button.module.css";

export type ButtonStyle = {
  variant?: "primary" | "secondary" | "ghost";
  size?: "sm" | "md" | "lg" | "xl";
  pill?: boolean;
  block?: boolean;
};

// Shared by Button and LinkButton so a link can look exactly like a button.
export function buttonClass({
  variant = "primary",
  size = "md",
  pill,
  block,
}: ButtonStyle): string {
  return [
    styles.button,
    styles[variant],
    styles[size],
    pill && styles.pill,
    block && styles.block,
  ]
    .filter(Boolean)
    .join(" ");
}
