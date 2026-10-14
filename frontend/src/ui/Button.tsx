import type { ComponentProps } from "react";
import { Link } from "react-router-dom";
import { Icon } from "./Icon";
import { buttonClass, type ButtonStyle } from "./buttonClass";
import styles from "./Button.module.css";

type ButtonProps = ComponentProps<"button"> &
  ButtonStyle & { loading?: boolean };

export function Button({
  variant,
  size,
  pill,
  block,
  loading,
  children,
  className,
  disabled,
  ...rest
}: ButtonProps) {
  return (
    <button
      {...rest}
      type={rest.type ?? "button"}
      disabled={disabled || loading}
      aria-busy={loading || undefined}
      className={[buttonClass({ variant, size, pill, block }), className]
        .filter(Boolean)
        .join(" ")}
    >
      {loading && <Icon name="spinner" size={18} className={styles.spinner} />}
      {children}
    </button>
  );
}

type LinkButtonProps = ComponentProps<typeof Link> & ButtonStyle;

export function LinkButton({
  variant,
  size,
  pill,
  block,
  className,
  ...rest
}: LinkButtonProps) {
  return (
    <Link
      {...rest}
      className={[buttonClass({ variant, size, pill, block }), className]
        .filter(Boolean)
        .join(" ")}
    />
  );
}
