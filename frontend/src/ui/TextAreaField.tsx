import { useId, type ComponentProps } from "react";
import { Shell, type Shared } from "./FieldShell";
import field from "./Field.module.css";
import styles from "./TextAreaField.module.css";

type Props = Shared &
  ComponentProps<"textarea"> & {
    // How many characters are in the box now. With `maxLength`, shows "16 / 2000" under it.
    count?: number;
  };

// A box for a few lines of text, e.g. notes. Works with react-hook-form's `register` like Field does.
export function TextAreaField({
  label,
  hint,
  error,
  aside,
  optional,
  count,
  ...input
}: Props) {
  const id = useId();
  const described = error || hint ? `${id}-msg` : undefined;
  return (
    <Shell
      id={id}
      label={label}
      hint={hint}
      error={error}
      aside={aside}
      optional={optional}
    >
      <textarea
        {...input}
        id={id}
        aria-invalid={error ? true : undefined}
        aria-describedby={described}
        className={`${field.control} ${styles.area}`}
      />
      {count !== undefined && input.maxLength && (
        <p className={styles.counter} aria-hidden="true">
          {count} / {input.maxLength}
        </p>
      )}
    </Shell>
  );
}
