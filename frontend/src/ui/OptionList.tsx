import type { Option } from "./selectTypes";
import { Icon } from "./Icon";
import styles from "./Select.module.css";

function scrollIntoView(el: HTMLElement | null) {
  el?.scrollIntoView?.({ block: "nearest" });
}

// A menu opens under its trigger, lined up with the trigger's left edge. If that would push it
// past the right edge of the screen (a pill at the far right), it lines up with the right edge
// instead, so the page never grows sideways. Set directly on the element, as it only matters once.
function keepOnScreen(el: HTMLElement | null) {
  if (
    !el ||
    el.getBoundingClientRect().right <= document.documentElement.clientWidth - 8
  )
    return;
  el.style.left = "auto";
  el.style.right = "0";
}

type Props = {
  id: string;
  labelId: string;
  options: readonly Option[];
  selected: number;
  active: number;
  onActive: (index: number) => void;
  onPick: (index: number) => void;
  // Lets a compact trigger give the menu its own width.
  className?: string;
};

// The popup list of options, shared by every select-like control.
export function OptionList({
  id,
  labelId,
  options,
  selected,
  active,
  onActive,
  onPick,
  className,
}: Props) {
  return (
    <ul
      ref={keepOnScreen}
      id={id}
      role="listbox"
      aria-labelledby={labelId}
      className={`${styles.menu} ${className ?? ""}`}
    >
      {options.map((o, i) => (
        <li
          key={o.value}
          id={`${id}-${i}`}
          role="option"
          aria-selected={i === selected}
          ref={i === active ? scrollIntoView : undefined}
          className={`${styles.option} ${i === active ? styles.active : ""} ${i === selected ? styles.selected : ""}`}
          // Keep focus on the button while the pointer is on the menu.
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
  );
}
