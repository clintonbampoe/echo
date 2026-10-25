import type { KeyboardEvent, Ref } from "react";
import type { Option } from "./selectTypes";
import { Icon } from "../layout/Icon";
import { keepOnScreen } from "../overlays/placement";
import { SearchField } from "./SearchField";
import styles from "./Select.module.css";

function scrollIntoView(el: HTMLElement | null) {
  el?.scrollIntoView?.({ block: "nearest" });
}

// A long list has a search box on top. Focus stays in it, and it points at the highlighted option.
export type MenuSearch = {
  label: string;
  value: string;
  onChange: (text: string) => void;
  onKeyDown: (e: KeyboardEvent) => void;
};

type Props = {
  id: string;
  labelId: string;
  // The options to show: all of them, or the ones that match the search.
  options: readonly Option[];
  selected: number;
  active: number;
  onActive: (index: number) => void;
  onPick: (index: number) => void;
  search?: MenuSearch;
  // The search box, so the menu's owner can move focus into it.
  searchRef?: Ref<HTMLInputElement>;
  // True while the menu plays its exit animation.
  closing?: boolean;
  // Lets a compact trigger give the menu its own width.
  className?: string;
};

// The popup list of options, shared by every select-like control on a mouse or keyboard.
export function OptionList({
  id,
  labelId,
  options,
  selected,
  active,
  onActive,
  onPick,
  search,
  searchRef,
  closing,
  className,
}: Props) {
  return (
    <div
      ref={keepOnScreen}
      className={`${styles.menu} ${className ?? ""}`}
      data-closing={closing}
    >
      {search && (
        <SearchField
          ref={searchRef}
          label={search.label}
          placeholder="Search"
          value={search.value}
          onChange={(e) => search.onChange(e.target.value)}
          onKeyDown={search.onKeyDown}
          aria-controls={id}
          aria-activedescendant={options.length ? `${id}-${active}` : undefined}
          autoComplete="off"
          className={styles.search}
        />
      )}
      <ul
        id={id}
        role="listbox"
        aria-labelledby={labelId}
        className={styles.list}
        hidden={!options.length}
      >
        {options.map((o, i) => (
          <li
            key={o.value}
            id={`${id}-${i}`}
            role="option"
            aria-selected={i === selected}
            ref={i === active ? scrollIntoView : undefined}
            className={`${styles.option} ${i === active ? styles.active : ""} ${i === selected ? styles.selected : ""}`}
            // Keep focus where it is while the pointer is on the menu.
            onMouseDown={(e) => e.preventDefault()}
            onMouseMove={() => onActive(i)}
            onClick={() => onPick(i)}
          >
            <span>{o.label}</span>
            {i === selected && (
              <Icon name="check" size={16} className={styles.check} />
            )}
          </li>
        ))}
      </ul>
      {!options.length && (
        <p role="status" className={styles.none}>
          No matches
        </p>
      )}
    </div>
  );
}
