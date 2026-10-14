import { useRef, useState, type KeyboardEvent } from "react";
import type { Option } from "./selectTypes";

// The state and keyboard behaviour of a select menu, with no markup.
export function useListbox(
  options: readonly Option[],
  value: string,
  onChange: (value: string) => void,
) {
  const [open, setOpen] = useState(false);
  const [active, setActive] = useState(0);
  const typed = useRef({ text: "", at: 0 });
  const selected = options.findIndex((o) => o.value === value);
  const last = options.length - 1;

  function show() {
    setActive(Math.max(selected, 0));
    setOpen(true);
  }

  function pick(index: number) {
    const option = options[index];
    if (option) onChange(option.value);
    setOpen(false);
  }

  // Typing letters jumps to the option that starts with them, like a native select.
  function search(char: string) {
    const now = Date.now();
    const text = (
      now - typed.current.at > 600 ? char : typed.current.text + char
    ).toLowerCase();
    typed.current = { text, at: now };
    const index = options.findIndex((o) =>
      o.label.toLowerCase().startsWith(text),
    );
    if (index < 0) return;
    if (open) setActive(index);
    else pick(index);
  }

  function onKeyDown(e: KeyboardEvent) {
    if (e.key === "Escape" && !open) return;
    const actions: Record<string, () => void> = {
      ArrowDown: () => (open ? setActive(Math.min(active + 1, last)) : show()),
      ArrowUp: () => (open ? setActive(Math.max(active - 1, 0)) : show()),
      Home: () => open && setActive(0),
      End: () => open && setActive(last),
      Enter: () => (open ? pick(active) : show()),
      " ": () => (open ? pick(active) : show()),
      Escape: () => setOpen(false),
    };
    const action = actions[e.key];
    if (action) {
      e.preventDefault();
      action();
    } else if (e.key.length === 1 && !e.ctrlKey && !e.metaKey) {
      search(e.key);
    }
  }

  return { open, setOpen, active, setActive, selected, show, pick, onKeyDown };
}
