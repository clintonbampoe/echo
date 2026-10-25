import {
  useEffect,
  useId,
  useRef,
  useState,
  type KeyboardEvent,
  type RefObject,
} from "react";
import { createPortal } from "react-dom";
import { Icon } from "../layout/Icon";
import { usePresence } from "./usePresence";
import styles from "./ActionMenu.module.css";

export type MenuItem = {
  label: string;
  onSelect: () => void;
  // Marks an action that removes something.
  danger?: boolean;
  // For a choice among several (a role, say): true for the one that is current. Shows a radio mark.
  checked?: boolean;
  // A heading shown above this item, and above the next ones until another heading comes.
  group?: string;
  // A line above this item, to set it apart from the ones before it.
  separated?: boolean;
};

type Props = {
  // Names the ⋯ button for screen readers, e.g. "Actions for Kwame Asante".
  label: string;
  // Heads the menu when it opens as a sheet on a phone.
  title: string;
  items: MenuItem[];
};

// Puts the menu just under the button (or above it when there is no room below). On a touch
// screen the CSS ignores these numbers and docks the menu to the bottom of the screen instead.
function place(trigger: HTMLElement | null) {
  return (menu: HTMLElement | null) => {
    if (!menu || !trigger) return;
    const t = trigger.getBoundingClientRect();
    menu.style.setProperty(
      "--right",
      `${document.documentElement.clientWidth - t.right}px`,
    );
    menu.style.setProperty("--top", `${t.bottom + 6}px`);
    const m = menu.getBoundingClientRect();
    if (m.bottom > window.innerHeight - 8)
      menu.style.setProperty("--top", `${Math.max(8, t.top - m.height - 6)}px`);
  };
}

// Escape closes, Tab leaves, and the arrow keys move between the items (wrapping round).
function navigate(
  e: KeyboardEvent,
  menu: HTMLElement | null,
  close: () => void,
  leave: () => void,
) {
  if (e.key === "Escape") return close();
  if (e.key === "Tab") return leave();
  const buttons = [
    ...(menu?.querySelectorAll<HTMLElement>("[role^=menuitem]") ?? []),
  ];
  const at = buttons.indexOf(document.activeElement as HTMLElement);
  const next = {
    ArrowDown: at + 1,
    ArrowUp: at - 1,
    Home: 0,
    End: buttons.length - 1,
  }[e.key];
  if (next === undefined) return;
  e.preventDefault();
  buttons[(next + buttons.length) % buttons.length]?.focus();
}

// While the menu is open: focus the first item, and close it if the page moves out from under it.
function useDismissOnMove(
  open: boolean,
  menu: RefObject<HTMLElement | null>,
  setOpen: (open: boolean) => void,
) {
  useEffect(() => {
    if (!open) return;
    // preventScroll: moving focus must not scroll the page, or the scroll handler below would close the menu.
    menu.current
      ?.querySelector<HTMLElement>("[role^=menuitem]")
      ?.focus({ preventScroll: true });
    const width = window.innerWidth;
    // The menu is fixed to the screen, so it would drift away from its row if the page moved.
    // Scrolling inside the menu is fine, and so is a phone's address bar growing or shrinking
    // (that fires "resize" too, but the width stays the same).
    const onScroll = (e: Event) => {
      if (!menu.current?.contains(e.target as Node)) setOpen(false);
    };
    const onResize = () => {
      if (window.innerWidth !== width) setOpen(false);
    };
    window.addEventListener("scroll", onScroll, true);
    window.addEventListener("resize", onResize);
    return () => {
      window.removeEventListener("scroll", onScroll, true);
      window.removeEventListener("resize", onResize);
    };
  }, [open, menu, setOpen]);
}

type RowProps = {
  item: MenuItem;
  newGroup: boolean;
  onChoose: (item: MenuItem) => void;
};

// One line of the menu: its heading and separator when it has them, then the button.
function MenuRow({ item, newGroup, onChoose }: RowProps) {
  return (
    <>
      {newGroup && <p className={styles.group}>{item.group}</p>}
      {item.separated && <hr className={styles.rule} />}
      <button
        type="button"
        role={item.checked === undefined ? "menuitem" : "menuitemradio"}
        aria-checked={item.checked}
        className={`${styles.item} ${item.danger ? styles.danger : ""}`}
        onClick={() => onChoose(item)}
      >
        {item.checked !== undefined && (
          <span
            className={`${styles.radio} ${item.checked ? styles.on : ""}`}
            aria-hidden="true"
          >
            {item.checked && <Icon name="check" size={12} />}
          </span>
        )}
        {item.label}
      </button>
    </>
  );
}

// A ⋯ button that opens a short list of actions for one row. A dropdown with a mouse, a sheet
// from the bottom on a phone. Arrow keys move, Escape closes, and focus returns to the button.
export function ActionMenu({ label, title, items }: Props) {
  const [open, setOpen] = useState(false);
  const trigger = useRef<HTMLButtonElement>(null);
  const menu = useRef<HTMLDivElement>(null);
  const titleId = useId();
  const { mounted, closing } = usePresence(open, 160);

  useDismissOnMove(open, menu, setOpen);

  function close() {
    setOpen(false);
    trigger.current?.focus();
  }

  function choose(item: MenuItem) {
    close();
    item.onSelect();
  }

  return (
    <>
      <button
        ref={trigger}
        type="button"
        aria-label={label}
        aria-haspopup="menu"
        aria-expanded={open}
        className={styles.trigger}
        onClick={() => setOpen(!open)}
      >
        <Icon name="more" size={18} />
      </button>
      {mounted &&
        createPortal(
          <div className={styles.root}>
            <div
              className={`${styles.scrim} scrim-fade`}
              data-closing={closing}
              onClick={close}
              aria-hidden="true"
            />
            <div
              ref={(el) => {
                menu.current = el;
                place(trigger.current)(el);
              }}
              role="menu"
              aria-labelledby={titleId}
              className={styles.menu}
              data-closing={closing}
              onKeyDown={(e) =>
                navigate(e, menu.current, close, () => setOpen(false))
              }
            >
              <p id={titleId} className={styles.title}>
                {title}
              </p>
              <div className={styles.items}>
                {items.map((item, i) => (
                  <MenuRow
                    key={item.label}
                    item={item}
                    newGroup={
                      !!item.group && item.group !== items[i - 1]?.group
                    }
                    onChoose={choose}
                  />
                ))}
              </div>
              {/* Only drawn in the bottom sheet, where there is no click-outside to lean on. */}
              <button type="button" className={styles.cancel} onClick={close}>
                Cancel
              </button>
            </div>
          </div>,
          document.body,
        )}
    </>
  );
}
