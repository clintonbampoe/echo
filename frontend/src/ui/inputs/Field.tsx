import { useId, useState, type ComponentProps } from "react";
import { Icon } from "../layout/Icon";
import { Shell, type Shared } from "./FieldShell";
import { useCapsLock } from "./useCapsLock";
import { PasswordStrength } from "./PasswordStrength";
import styles from "./Field.module.css";

type FieldProps = Shared &
  ComponentProps<"input"> & {
    // A short mark shown inside the left edge, e.g. a currency like "GH₵".
    unit?: string;
    // A pale completion shown after what has been typed (`typed` is only measured, never seen).
    // The page decides what to suggest and what Tab does with it. Pass it on every render (with an
    // empty `rest` when there is nothing to suggest) so the input is never rebuilt while someone types.
    ghost?: { typed: string; rest: string };
  };

// At the right edge of the field: a lock when the value cannot be changed, a ring and tick once an
// email address looks finished.
function edgeMark(readOnly: boolean | undefined, type: string | undefined) {
  if (readOnly) return <Icon name="lock" size={15} className={styles.lock} />;
  if (type === "email")
    return <span className={styles.tick} aria-hidden="true" />;
  return null;
}

export function Field({
  label,
  hint,
  error,
  aside,
  optional,
  unit,
  ghost,
  ...input
}: FieldProps) {
  const id = useId();
  const described = error || hint ? `${id}-msg` : undefined;
  const mark = edgeMark(input.readOnly, input.type);
  const control = (
    <input
      // Off unless the caller names what the field holds (sign-in and sign-up do): a form for someone else's
      // details must not offer the signed-in person's own name, phone or address from the phone's contacts.
      autoComplete="off"
      {...input}
      id={id}
      aria-invalid={error ? true : undefined}
      aria-describedby={described}
      className={`${styles.control} ${unit ? styles.withUnit : ""} ${mark ? styles.padRight : ""}`}
    />
  );
  return (
    <Shell
      id={id}
      label={label}
      hint={hint}
      error={error}
      aside={aside}
      optional={optional}
    >
      {ghost ? (
        <div className={styles.ghostWrap}>
          {control}
          {ghost.rest && (
            <span className={styles.ghost} aria-hidden="true">
              <span>
                <span className={styles.ghostTyped}>{ghost.typed}</span>
                {ghost.rest}
              </span>
              <kbd className={styles.ghostKey}>Tab</kbd>
            </span>
          )}
        </div>
      ) : unit ? (
        <div className={styles.unitWrap}>
          <span className={styles.unit} aria-hidden="true">
            {unit}
          </span>
          {control}
        </div>
      ) : mark ? (
        <div className={styles.unitWrap}>
          {control}
          {mark}
        </div>
      ) : (
        control
      )}
    </Shell>
  );
}

export function PasswordField({
  label,
  hint,
  error,
  aside,
  optional,
  strengthOf,
  ...input
}: Omit<FieldProps, "type"> & {
  // Pass the password so far to show a strength meter under the field. Leave it out for a sign-in field.
  strengthOf?: string;
}) {
  const id = useId();
  const [shown, setShown] = useState(false);
  const { caps, handlers } = useCapsLock(input.onBlur);
  const hintId = `${id}-strength`;
  const described =
    [error || hint ? `${id}-msg` : "", strengthOf === undefined ? "" : hintId]
      .filter(Boolean)
      .join(" ") || undefined;
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
          className={`${styles.control} ${styles.padRight} ${shown ? styles.shown : ""}`}
          {...handlers}
        />
        {/* The icon shows what the button will do: an open eye while the password is hidden (press to
            show it), a crossed-out eye while it is shown (press to hide it). */}
        <button
          type="button"
          className={styles.toggle}
          aria-label={shown ? "Hide password" : "Show password"}
          data-shown={shown}
          onClick={() => setShown(!shown)}
        >
          <span className={`${styles.glyph} ${styles.glyphOpen}`}>
            <Icon name="eye" />
          </span>
          <span className={`${styles.glyph} ${styles.glyphCrossed}`}>
            <Icon name="eye-off" />
          </span>
        </button>
      </div>
      <PasswordNotes
        caps={caps && !shown}
        strengthOf={strengthOf}
        hintId={hintId}
      />
    </Shell>
  );
}

function PasswordNotes({
  caps,
  strengthOf,
  hintId,
}: {
  caps: boolean;
  strengthOf: string | undefined;
  hintId: string;
}) {
  return (
    <>
      {caps && (
        <p role="status" className={styles.caps}>
          Caps Lock is on
        </p>
      )}
      {strengthOf !== undefined && (
        <PasswordStrength value={strengthOf} hintId={hintId} />
      )}
    </>
  );
}
