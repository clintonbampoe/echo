import { useId, useRef, useState, type FocusEvent } from "react";
import { Icon } from "../layout/Icon";
import { OptionList } from "./OptionList";
import { OptionPanel } from "../overlays/OptionPanel";
import type { Option } from "./selectTypes";
import { useFinePointer } from "../hooks/useFinePointer";
import { useListbox } from "./useListbox";
import { usePresence } from "../overlays/usePresence";
import styles from "./FilterSelect.module.css";

type Props = {
  // The word on the pill while nothing is chosen, e.g. "Status".
  label: string;
  // The first option means "no filter" and must have the value ''.
  options: readonly Option[];
  value: string;
  onChange: (value: string) => void;
  // Said before the chosen option on the pill, e.g. "Joined:" gives "Joined: All time".
  prefix?: string;
  // For a choice that always has a value (like a date range): no highlight, since nothing is being filtered out.
  quiet?: boolean;
};

// A pill that narrows a list. With a mouse it opens our menu; on a touch screen a bottom sheet.
export function FilterSelect(props: Props) {
  return useFinePointer() ? <MenuPill {...props} /> : <PanelPill {...props} />;
}

const applied = (value: string, quiet?: boolean) => !!value && !quiet;

function text(label: string, options: readonly Option[], value: string) {
  return value
    ? (options.find((o) => o.value === value)?.label ?? label)
    : label;
}

// What the pill says: the choice, or the filter's name while nothing is chosen.
const shown = (p: Props) =>
  `${p.prefix ? `${p.prefix} ` : ""}${text(p.label, p.options, p.value)}`;

function MenuPill(props: Props) {
  const { label, options, value, onChange, quiet } = props;
  const id = useId();
  const trigger = useRef<HTMLButtonElement>(null);
  const searchBox = useRef<HTMLInputElement>(null);
  const lb = useListbox(options, value, onChange, { trigger, searchBox });
  const menu = usePresence(lb.open, 140);

  function onFocusOut(e: FocusEvent<HTMLDivElement>) {
    if (!e.currentTarget.contains(e.relatedTarget)) lb.dismiss();
  }

  return (
    <div className={styles.wrap} onBlur={onFocusOut}>
      <button
        ref={trigger}
        type="button"
        id={id}
        aria-haspopup="listbox"
        aria-expanded={lb.open}
        aria-label={`${label}: ${text("Any", options, value)}`}
        className={`${styles.pill} ${applied(value, quiet) ? styles.on : ""}`}
        onClick={() => (lb.open ? lb.close() : lb.show())}
        onKeyDown={lb.onKeyDown}
        onKeyUp={(e) => e.key === " " && e.preventDefault()}
      >
        {shown(props)}
        {!applied(value, quiet) && (
          <Icon name="chevron-down" size={12} className={styles.chevron} />
        )}
      </button>
      <Clear
        label={label}
        show={applied(value, quiet)}
        onClear={() => onChange("")}
      />
      {menu.mounted && (
        <OptionList
          closing={menu.closing}
          id={`${id}-list`}
          labelId={id}
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
          className={styles.menu}
        />
      )}
    </div>
  );
}

// On a touch screen the pill opens the choices in a bottom sheet.
function PanelPill(props: Props) {
  const { label, options, value, onChange, quiet } = props;
  const [open, setOpen] = useState(false);
  return (
    <div className={styles.wrap}>
      <button
        type="button"
        aria-haspopup="dialog"
        aria-expanded={open}
        aria-label={`${label}: ${text("Any", options, value)}`}
        className={`${styles.pill} ${applied(value, quiet) ? styles.on : ""}`}
        onClick={() => setOpen(true)}
      >
        {shown(props)}
        {!applied(value, quiet) && (
          <Icon name="chevron-down" size={12} className={styles.chevron} />
        )}
      </button>
      <Clear
        label={label}
        show={applied(value, quiet)}
        onClear={() => onChange("")}
      />
      <OptionPanel
        open={open}
        onClose={() => setOpen(false)}
        title={label}
        options={options}
        value={value}
        onPick={onChange}
      />
    </div>
  );
}

// The ✕ on a pill that is filtering: puts the list back to everything without opening the menu.
function Clear({
  label,
  show,
  onClear,
}: {
  label: string;
  show: boolean;
  onClear: () => void;
}) {
  if (!show) return null;
  return (
    <button
      type="button"
      aria-label={`Clear ${label.toLowerCase()} filter`}
      className={styles.clear}
      onClick={onClear}
    >
      <Icon name="x" size={12} />
    </button>
  );
}
