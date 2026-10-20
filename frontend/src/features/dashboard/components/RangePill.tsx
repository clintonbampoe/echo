import { useState } from "react";
import { Icon, PickerAnchor } from "@/ui";
import { moveFocus } from "./optionKeys";
import styles from "./RangePill.module.css";

type Props<V extends string> = {
  // Names the control: "Range".
  label: string;
  value: V;
  options: readonly { value: V; label: string }[];
  onChange: (value: V) => void;
};

// A pill that opens a short list of ranges ("Last 8 weeks"). A popover on a mouse, a bottom sheet on touch.
export function RangePill<V extends string>({
  label,
  value,
  options,
  onChange,
}: Props<V>) {
  const [open, setOpen] = useState(false);
  const current = options.find((o) => o.value === value);

  const pick = (v: V) => {
    onChange(v);
    setOpen(false);
  };

  const panel = (
    <div
      role="listbox"
      aria-label={label}
      className={styles.list}
      onKeyDown={moveFocus}
    >
      {options.map((o) => {
        const selected = o.value === value;
        return (
          <button
            key={o.value}
            type="button"
            role="option"
            aria-selected={selected}
            tabIndex={selected ? 0 : -1}
            data-autofocus={selected ? "" : undefined}
            className={`${styles.option} ${selected ? styles.selected : ""}`}
            onClick={() => pick(o.value)}
          >
            <span className={styles.check}>
              {selected && <Icon name="check" size={16} />}
            </span>
            {o.label}
          </button>
        );
      })}
    </div>
  );

  return (
    <PickerAnchor
      open={open}
      onClose={() => setOpen(false)}
      title={label}
      panel={panel}
      className={styles.anchor}
    >
      <button
        type="button"
        data-trigger
        aria-haspopup="listbox"
        aria-expanded={open}
        aria-label={`${label}: ${current?.label ?? ""}`}
        className={styles.pill}
        onClick={() => setOpen(!open)}
      >
        {current?.label}
        <Icon name="chevron-down" size={14} className={styles.chevron} />
      </button>
    </PickerAnchor>
  );
}
