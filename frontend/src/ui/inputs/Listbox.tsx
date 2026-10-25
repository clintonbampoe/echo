import { useId, useRef, type FocusEvent } from "react";
import fieldStyles from "./Field.module.css";
import { Icon } from "../layout/Icon";
import { OptionList } from "./OptionList";
import type { ControlProps } from "./selectTypes";
import styles from "./Select.module.css";
import { useListbox } from "./useListbox";
import { usePresence } from "../overlays/usePresence";

// Our own dropdown, for mouse and keyboard. It follows the standard "select" pattern: the button holds
// focus and points at the highlighted option. A long list adds a search box, which then holds focus.
export function Listbox({
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
  const listId = useId();
  const trigger = useRef<HTMLButtonElement>(null);
  const searchBox = useRef<HTMLInputElement>(null);
  const lb = useListbox(options, value, onChange, { trigger, searchBox });
  const menu = usePresence(lb.open, 140);

  // Focus leaving the whole control (not just moving within it) closes the menu.
  function onFocusOut(e: FocusEvent<HTMLDivElement>) {
    if (e.currentTarget.contains(e.relatedTarget)) return;
    lb.dismiss();
    onBlur?.();
  }

  return (
    <div className={styles.wrap} onBlur={onFocusOut}>
      <button
        ref={trigger}
        type="button"
        id={id}
        role="combobox"
        aria-haspopup="listbox"
        aria-expanded={lb.open}
        aria-controls={lb.open ? listId : undefined}
        aria-activedescendant={
          lb.open && !lb.searchable ? `${listId}-${lb.active}` : undefined
        }
        aria-invalid={invalid || undefined}
        aria-describedby={describedBy}
        disabled={disabled}
        className={`${fieldStyles.control} ${fieldStyles.trigger}`}
        onClick={() => (lb.open ? lb.close() : lb.show())}
        onKeyDown={lb.onKeyDown}
        onKeyUp={(e) => e.key === " " && e.preventDefault()}
      >
        <span>{lb.chosen?.label ?? placeholder}</span>
        <Icon name="chevron-down" size={16} className={fieldStyles.chevron} />
      </button>
      {menu.mounted && (
        <OptionList
          closing={menu.closing}
          id={listId}
          labelId={`${id}-label`}
          options={lb.shown}
          selected={lb.selected}
          active={lb.active}
          onActive={lb.setActive}
          onPick={lb.pick}
          searchRef={searchBox}
          search={
            lb.searchable
              ? {
                  label: `Search ${label.toLowerCase()}`,
                  value: lb.query,
                  onChange: lb.search,
                  onKeyDown: lb.onSearchKeyDown,
                }
              : undefined
          }
        />
      )}
    </div>
  );
}
