import { useId, useState, type ComponentProps } from "react";
import { Icon } from "./Icon";
import { Shell, type Shared } from "./FieldShell";
import styles from "./Field.module.css";

type FieldProps = Shared & ComponentProps<"input">;

export function Field({
  label,
  hint,
  error,
  aside,
  optional,
  ...input
}: FieldProps) {
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
      <input
        {...input}
        id={id}
        aria-invalid={error ? true : undefined}
        aria-describedby={described}
        className={styles.control}
      />
    </Shell>
  );
}

export function PasswordField({
  label,
  hint,
  error,
  aside,
  optional,
  ...input
}: Omit<FieldProps, "type">) {
  const id = useId();
  const [shown, setShown] = useState(false);
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
      <div className={styles.withButton}>
        <input
          {...input}
          id={id}
          type={shown ? "text" : "password"}
          aria-invalid={error ? true : undefined}
          aria-describedby={described}
          className={`${styles.control} ${styles.padRight}`}
        />
        <button
          type="button"
          className={styles.toggle}
          aria-label={shown ? "Hide password" : "Show password"}
          aria-pressed={shown}
          onClick={() => setShown(!shown)}
        >
          <Icon name="eye" />
        </button>
      </div>
    </Shell>
  );
}
