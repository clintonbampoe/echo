import { useId, type CSSProperties } from "react";
import { Icon, type IconName } from "./Icon";
import { useFlip } from "./useFlip";
import styles from "./SegmentedField.module.css";

type Props<V extends string> = {
  label: string;
  // `icon` draws a small mark before the word, e.g. an arrow for money in or out.
  options: readonly { value: V; label: string; icon?: IconName }[];
  value: V;
  onChange: (value: V) => void;
  // Keeps the label for screen readers but does not draw it, for a switch whose choices explain themselves.
  hideLabel?: boolean;
  // One joined bar with the picked choice filled in, for a switch that sits above a list (All / Income / Expenses).
  joined?: boolean;
  // With `joined`: the bar spans the whole width on a phone or tablet.
  fill?: boolean;
  // With `joined`: the bar spans the whole width at every size, for a choice inside a form.
  stretch?: boolean;
  // With `joined`: smaller words, for a switch squeezed beside a card's title.
  compact?: boolean;
};

const groupClass = (
  joined?: boolean,
  fill?: boolean,
  compact?: boolean,
  stretch?: boolean,
) =>
  [
    styles.group,
    joined && styles.joined,
    joined && fill && styles.fill,
    joined && compact && styles.compact,
    joined && stretch && styles.stretch,
  ]
    .filter(Boolean)
    .join(" ");

// A few choices side by side, one of them picked: the buttons of a short list that should stay in view (a role, say).
// Real radio buttons underneath, so the arrow keys and screen readers work as they should.
export function SegmentedField<V extends string>({
  label,
  options,
  value,
  onChange,
  hideLabel,
  joined,
  fill,
  compact,
  stretch,
}: Props<V>) {
  const name = useId();
  const flip = useFlip(value);
  // A joined switch slides one thumb behind the picked choice; the stylesheet only needs to know which of how many.
  const index = options.findIndex((o) => o.value === value);
  const thumb = joined
    ? ({
        "--n": options.length,
        "--i": Math.max(index, 0),
        "--thumb": index < 0 ? 0 : 1,
      } as CSSProperties)
    : undefined;
  return (
    <fieldset className={styles.field}>
      <legend className={hideLabel ? styles.hidden : styles.label}>
        {label}
      </legend>
      <div
        className={groupClass(joined, fill, compact, stretch)}
        style={thumb}
        data-flip={flip}
        data-squish="before"
      >
        {options.map((o) => (
          <label key={o.value} className={styles.option}>
            <input
              type="radio"
              name={name}
              value={o.value}
              checked={o.value === value}
              onChange={() => onChange(o.value)}
              className={styles.input}
            />
            <span className={styles.face}>
              {o.icon && <Icon name={o.icon} size={14} />}
              {o.label}
            </span>
          </label>
        ))}
      </div>
    </fieldset>
  );
}
