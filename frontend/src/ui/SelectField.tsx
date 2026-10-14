import { useId } from "react";
import fieldStyles from "./Field.module.css";
import { Shell, type Shared } from "./FieldShell";
import { Listbox } from "./Listbox";
import type { ControlProps } from "./selectTypes";
import { useFinePointer } from "./useFinePointer";

type Props = Shared &
  Pick<ControlProps, "options" | "value" | "onChange" | "onBlur" | "disabled">;

// A field whose value comes from a fixed list (design rule S2).
// With a mouse or trackpad it shows our own menu. On a touch screen it uses the phone's own
// picker, which is what people expect there and is easier to use than a small popup.
export function SelectField({ label, hint, error, aside, ...rest }: Props) {
  const id = useId();
  const control = {
    ...rest,
    id,
    invalid: Boolean(error),
    describedBy: error || hint ? `${id}-msg` : undefined,
  };
  return (
    <Shell id={id} label={label} hint={hint} error={error} aside={aside}>
      {useFinePointer() ? (
        <Listbox {...control} />
      ) : (
        <NativeSelect {...control} />
      )}
    </Shell>
  );
}

function NativeSelect({
  id,
  options,
  value,
  onChange,
  onBlur,
  disabled,
  invalid,
  describedBy,
}: ControlProps) {
  return (
    <select
      id={id}
      value={value}
      disabled={disabled}
      aria-invalid={invalid || undefined}
      aria-describedby={describedBy}
      onChange={(e) => onChange(e.target.value)}
      onBlur={onBlur}
      className={`${fieldStyles.control} ${fieldStyles.select}`}
    >
      {options.map((o) => (
        <option key={o.value} value={o.value}>
          {o.label}
        </option>
      ))}
    </select>
  );
}
