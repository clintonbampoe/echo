import { useId, type FocusEvent } from "react";
import fieldStyles from "./Field.module.css";
import { Icon } from "./Icon";
import { OptionList } from "./OptionList";
import type { ControlProps } from "./selectTypes";
import styles from "./Select.module.css";
import { useListbox } from "./useListbox";

// Our own dropdown, for mouse and keyboard. It follows the standard "select" pattern:
// the button holds focus the whole time and points at the highlighted option.
export function Listbox({
  id,
  options,
  value,
  onChange,
  onBlur,
  disabled,
  invalid,
  describedBy,
}: ControlProps) {
  const listId = useId();
  const { open, setOpen, active, setActive, selected, show, pick, onKeyDown } =
    useListbox(options, value, onChange);

  // Focus leaving the whole control (not just moving within it) closes the menu.
  function onFocusOut(e: FocusEvent<HTMLDivElement>) {
    if (e.currentTarget.contains(e.relatedTarget)) return;
    setOpen(false);
    onBlur?.();
  }

  return (
    <div className={styles.wrap} onBlur={onFocusOut}>
      <button
        type="button"
        id={id}
        role="combobox"
        aria-haspopup="listbox"
        aria-expanded={open}
        aria-controls={open ? listId : undefined}
        aria-activedescendant={open ? `${listId}-${active}` : undefined}
        aria-invalid={invalid || undefined}
        aria-describedby={describedBy}
        disabled={disabled}
        className={`${fieldStyles.control} ${fieldStyles.trigger}`}
        onClick={() => (open ? setOpen(false) : show())}
        onKeyDown={onKeyDown}
        onKeyUp={(e) => e.key === " " && e.preventDefault()}
      >
        <span>{options[selected]?.label}</span>
        <Icon
          name="chevrons-up-down"
          size={16}
          className={fieldStyles.chevron}
        />
      </button>
      {open && (
        <OptionList
          id={listId}
          labelId={`${id}-label`}
          options={options}
          selected={selected}
          active={active}
          onActive={setActive}
          onPick={pick}
        />
      )}
    </div>
  );
}
