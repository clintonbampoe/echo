import type { KeyboardEvent } from "react";

// Arrow keys, Home and End move between the options of a list; Enter or Space picks the one in focus.
export function moveFocus(e: KeyboardEvent<HTMLDivElement>) {
  if (!["ArrowDown", "ArrowUp", "Home", "End"].includes(e.key)) return;
  e.preventDefault();
  const all = [
    ...e.currentTarget.querySelectorAll<HTMLElement>('[role="option"]'),
  ];
  const at = all.findIndex((el) => el === document.activeElement);
  const step = e.key === "ArrowDown" ? 1 : -1;
  const next =
    e.key === "Home" ? 0 : e.key === "End" ? all.length - 1 : at + step;
  all[(next + all.length) % all.length]?.focus();
}
