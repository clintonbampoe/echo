import styles from "./Button.module.css";

export type ButtonStyle = {
  variant?: "primary" | "secondary" | "ghost" | "danger";
  size?: "sm" | "md" | "lg" | "xl";
  // Buttons are fully round pills (design system §3). Pass pill={false} for a squarer button inside a dense control.
  pill?: boolean;
  block?: boolean;
};

// Shared by Button and LinkButton so a link can look exactly like a button.
export function buttonClass({
  variant = "primary",
  size = "md",
  pill = true,
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
