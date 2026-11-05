import { useState } from "react";
import type { CSSProperties } from "react";
import { assessPassword } from "./passwordStrength";
import styles from "./PasswordStrength.module.css";

const SEGMENTS = [1, 2, 3, 4] as const;

type Props = {
  // The password so far.
  value: string;
  // The id of the hint line, so the field can name it as its description.
  hintId: string;
};

// A quiet strength meter for a new password. It informs and never blocks: only the length is a rule.
export function PasswordStrength({ value, hintId }: Props) {
  const { level, label, hint } = assessPassword(value);

  // Remember where the meter came from, so a jump of several segments fills them in turn, and the
  // ripple plays only on arriving at Strong.
  const [path, setPath] = useState({ level, from: level });
  if (path.level !== level) setPath({ level, from: path.level });
  const arrived = level === 4 && path.from < 4;

  return (
    <div className={styles.meter} data-level={level}>
      <div className={styles.bars} aria-hidden="true">
        {SEGMENTS.map((n) => (
          <span
            key={n}
            className={styles.track}
            data-ripple={arrived}
            style={{ "--i": n } as CSSProperties}
          >
            <span
              className={styles.fill}
              data-on={n <= level}
              style={
                { "--stagger": Math.max(0, n - path.from - 1) } as CSSProperties
              }
            />
          </span>
        ))}
      </div>
      <div className={styles.row}>
        <span key={hint} id={hintId} className={styles.hint}>
          {hint}
        </span>
        {/* The word is announced politely when it changes; the hint is the field's description. */}
        <span role="status" className={styles.word}>
          <span key={label}>{label}</span>
        </span>
      </div>
    </div>
  );
}
