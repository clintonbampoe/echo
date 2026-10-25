import { useState } from "react";
import fieldStyles from "../inputs/Field.module.css";
import { Icon } from "../layout/Icon";
import { OptionSheet } from "./OptionSheet";
import type { ControlProps } from "../inputs/selectTypes";

// The dropdown for a touch screen: a field that opens the choices in a bottom sheet.
export function SheetSelect({
  id,
  label,
  options,
  value,
  onChange,
  onBlur,
  disabled,
  placeholder,
  invalid,
  describedBy,
}: ControlProps) {
  const [open, setOpen] = useState(false);

  function close() {
    setOpen(false);
    onBlur?.();
  }

  return (
    <>
      <button
        type="button"
        id={id}
        aria-haspopup="dialog"
        aria-expanded={open}
        aria-labelledby={`${id}-label ${id}-value`}
        aria-invalid={invalid || undefined}
        aria-describedby={describedBy}
        disabled={disabled}
        className={`${fieldStyles.control} ${fieldStyles.trigger}`}
        onClick={() => setOpen(true)}
      >
        <span id={`${id}-value`}>
          {options.find((o) => o.value === value)?.label ?? placeholder}
        </span>
        <Icon name="chevron-down" size={16} className={fieldStyles.chevron} />
      </button>
      <OptionSheet
        open={open}
        onClose={close}
        title={label}
        options={options}
        value={value}
        onPick={onChange}
      />
    </>
  );
}
