import type { ComponentProps } from "react";
import { LinkButton } from "@/ui";
import styles from "./Cta.module.css";

// The page's own large button: 54px tall on a wide screen, 48px on a tablet or phone.
export function Cta({
  className,
  ...rest
}: Omit<ComponentProps<typeof LinkButton>, "size" | "pill">) {
  return (
    <LinkButton
      {...rest}
      size="lg"
      pill
      className={[styles.cta, className].filter(Boolean).join(" ")}
    />
  );
}
