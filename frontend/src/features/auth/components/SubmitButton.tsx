import type { ComponentProps } from "react";
import { Button } from "@/ui";
import styles from "./SubmitButton.module.css";

type Props = Omit<ComponentProps<typeof Button>, "loading" | "indicator"> & {
  // True while the request is out: the label gives way to three dots.
  loading?: boolean;
  // Said on the button once it worked ("Welcome back"). Leave it out to keep the label.
  done?: string;
};

// The one button that sends an auth form. Its width never changes: the label, the three dots and the
// result take the same place, and only one of them shows at a time.
export function SubmitButton({
  loading,
  done,
  children,
  className,
  ...rest
}: Props) {
  const state = done ? "done" : loading ? "wait" : "idle";
  return (
    <Button
      type="submit"
      size="lg"
      pill
      block
      {...rest}
      loading={loading || Boolean(done)}
      indicator={false}
      className={[styles.submit, className].filter(Boolean).join(" ")}
    >
      <span className={styles.stack} data-state={state}>
        <span className={styles.label}>{children}</span>
        <span className={styles.dots} aria-hidden="true">
          <i />
          <i />
          <i />
        </span>
        {done && <span className={styles.result}>{done}</span>}
      </span>
    </Button>
  );
}
