import { useId, type FocusEvent } from "react";
import { Icon } from "./Icon";
import { OptionList } from "./OptionList";
import type { Option } from "./selectTypes";
import { useFinePointer } from "./useFinePointer";
import { useListbox } from "./useListbox";
import styles from "./FilterSelect.module.css";

type Props = {
  // The word on the pill while nothing is chosen, e.g. "Status".
  label: string;
  // The first option means "no filter" and must have the value ''.
  options: readonly Option[];
  value: string;
  onChange: (value: string) => void;
  // For a choice that always has a value (like a date range): no highlight, since nothing is being filtered out.
  quiet?: boolean;
};

// A pill that narrows a list. With a mouse it opens our menu; on a touch screen the phone's own picker.
export function FilterSelect(props: Props) {
  return useFinePointer() ? <MenuPill {...props} /> : <NativePill {...props} />;
}

function text(label: string, options: readonly Option[], value: string) {
  return value
    ? (options.find((o) => o.value === value)?.label ?? label)
    : label;
}

function MenuPill({ label, options, value, onChange, quiet }: Props) {
  const id = useId();
  const { open, setOpen, active, setActive, selected, show, pick, onKeyDown } =
    useListbox(options, value, onChange);

  function onFocusOut(e: FocusEvent<HTMLDivElement>) {
    if (!e.currentTarget.contains(e.relatedTarget)) setOpen(false);
  }

  return (
    <div className={styles.wrap} onBlur={onFocusOut}>
      <button
        type="button"
        id={id}
        aria-haspopup="listbox"
        aria-expanded={open}
        aria-label={`${label}: ${text("Any", options, value)}`}
        className={`${styles.pill} ${value && !quiet ? styles.on : ""}`}
        onClick={() => (open ? setOpen(false) : show())}
        onKeyDown={onKeyDown}
        onKeyUp={(e) => e.key === " " && e.preventDefault()}
      >
        {text(label, options, value)}
        <Icon name="chevrons-up-down" size={14} className={styles.chevron} />
      </button>
      {open && (
        <OptionList
          id={`${id}-list`}
          labelId={id}
          options={options}
          selected={selected}
          active={active}
          onActive={setActive}
          onPick={pick}
          className={styles.menu}
        />
      )}
    </div>
  );
}

// The real <select> sits invisibly over the pill, so a tap opens the phone's picker.
function NativePill({ label, options, value, onChange, quiet }: Props) {
  return (
    <div className={styles.wrap}>
      <span
        className={`${styles.pill} ${value && !quiet ? styles.on : ""}`}
        aria-hidden="true"
      >
        {text(label, options, value)}
        <Icon name="chevrons-up-down" size={14} className={styles.chevron} />
      </span>
      <select
        aria-label={label}
        value={value}
        onChange={(e) => onChange(e.target.value)}
        className={styles.native}
      >
        {options.map((o) => (
          <option key={o.value} value={o.value}>
            {o.value ? o.label : `${label}: any`}
          </option>
        ))}
      </select>
    </div>
  );
}
