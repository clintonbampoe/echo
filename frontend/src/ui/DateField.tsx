import { useId, useState } from "react";
import { formatDate } from "@/lib/format";
import { todayText } from "@/lib/dates";
import { BirthPanel } from "./BirthPanel";
import { DatePanel } from "./DatePanel";
import fieldStyles from "./Field.module.css";
import { Shell, type Shared } from "./FieldShell";
import { Icon } from "./Icon";
import { PickerAnchor } from "./PickerAnchor";
import { useFinePointer } from "./useFinePointer";

type Props = Shared & {
  // A day as "2026-10-11", or '' for none.
  value: string;
  onChange: (day: string) => void;
  onBlur?: () => void;
  // Days outside these cannot be picked. A date of birth cannot be in the future; other dates reach 5 years ahead.
  min?: string;
  max?: string;
  // 'birth' is for a date far back: a touch screen gets day, month and year wheels instead of a calendar.
  kind?: "birth";
  disabled?: boolean;
};

// The last day that can be picked: today for a birthday, 5 years ahead for anything else, unless the form says.
const lastDay = (max: string | undefined, kind: Props["kind"], today: string) =>
  max ?? (kind === "birth" ? today : `${Number(today.slice(0, 4)) + 5}-12-31`);

type PanelProps = {
  value: string;
  today: string;
  min: string;
  max: string;
  kind: Props["kind"];
  onPick: (day: string) => void;
  onClose: () => void;
  onClear?: () => void;
};

// A calendar, or on a touch screen for a birthday, day, month and year wheels.
function Panel({ kind, onClose, ...rest }: PanelProps) {
  const fine = useFinePointer();
  return kind === "birth" && !fine ? (
    <BirthPanel {...rest} onClose={onClose} />
  ) : (
    <DatePanel {...rest} />
  );
}

// A form field for a day. Our own calendar, never the browser's date input (design rule S10).
export function DateField({
  label,
  hint,
  error,
  aside,
  optional,
  value,
  onChange,
  onBlur,
  min = "1900-01-01",
  max,
  kind,
  disabled,
}: Props) {
  const id = useId();
  const [open, setOpen] = useState(false);
  const today = todayText();

  function close() {
    setOpen(false);
    onBlur?.();
  }

  function pick(day: string) {
    onChange(day);
    close();
  }

  const panel = (
    <Panel
      kind={kind}
      value={value}
      today={today}
      min={min}
      max={lastDay(max, kind, today)}
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
            {value ? formatDate(value) : "Choose a date"}
          </span>
          <Icon name="calendar" size={16} className={fieldStyles.chevron} />
        </button>
      </PickerAnchor>
    </Shell>
  );
}
