import { useId, useState } from "react";
import { spoken } from "@/lib/time";
import fieldStyles from "../inputs/Field.module.css";
import { Shell, type Shared } from "../inputs/FieldShell";
import { Icon } from "../layout/Icon";
import { PickerAnchor } from "./PickerAnchor";
import { TimePanel } from "./TimePanel";

type Props = Shared & {
  // A time as "14:05" (24 hours), or '' for none.
  value: string;
  onChange: (time: string) => void;
  onBlur?: () => void;
  disabled?: boolean;
};

// A form field for a time of day. Our own wheels, never the browser's time input (design rule S10).
export function TimeField({
  label,
  hint,
  error,
  aside,
  optional,
  value,
  onChange,
  onBlur,
  disabled,
}: Props) {
  const id = useId();
  const [open, setOpen] = useState(false);

  function close() {
    setOpen(false);
    onBlur?.();
  }

  function pick(time: string) {
    onChange(time);
    close();
  }

  const panel = (
    <TimePanel
      value={value}
      onPick={pick}
      onClose={close}
      onClear={optional && value ? () => pick("") : undefined}
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
      <PickerAnchor open={open} onClose={close} title={label} panel={panel}>
        <button
          type="button"
          id={id}
          data-trigger
          aria-haspopup="dialog"
          aria-expanded={open}
          aria-labelledby={`${id}-label ${id}-value`}
          aria-invalid={error ? true : undefined}
          aria-describedby={error || hint ? `${id}-msg` : undefined}
          disabled={disabled}
          className={`${fieldStyles.control} ${fieldStyles.trigger}`}
          onClick={() => (open ? close() : setOpen(true))}
        >
          <span
            id={`${id}-value`}
            className={value ? undefined : fieldStyles.placeholder}
          >
            {value ? spoken(value) : "Choose a time"}
          </span>
          <Icon name="clock" size={16} className={fieldStyles.chevron} />
        </button>
      </PickerAnchor>
    </Shell>
  );
}
