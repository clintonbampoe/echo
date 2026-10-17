import {
  useEffect,
  useRef,
  useState,
  type KeyboardEvent,
  type RefObject,
} from "react";
import { searchKeys, triggerKeys } from "./listboxKeys";
import { filterOptions, SEARCH_MIN } from "./options";
import type { Option } from "./selectTypes";

type Refs = {
  trigger: RefObject<HTMLButtonElement | null>;
  searchBox: RefObject<HTMLInputElement | null>;
};

// The state and keyboard behaviour of a select menu, with no markup. A long list gets a search box: focus
// moves into it while the menu is open and goes back to the trigger when the menu closes. The component
// owns the two elements and hands their refs in.
export function useListbox(
  options: readonly Option[],
  value: string,
  onChange: (value: string) => void,
  { trigger, searchBox }: Refs,
) {
  const [open, setOpen] = useState(false);
  const [active, setActive] = useState(0);
  const [query, setQuery] = useState("");
  const typed = useRef({ text: "", at: 0 });
  const searchable = options.length >= SEARCH_MIN;
  const shown = filterOptions(options, query);
  const selected = shown.findIndex((o) => o.value === value);
  const chosen = options.find((o) => o.value === value);

  useEffect(() => {
    if (open && searchable) searchBox.current?.focus();
  }, [open, searchable, searchBox]);

  function show(seed = "") {
    setQuery(seed);
    setActive(
      Math.max(
        filterOptions(options, seed).findIndex((o) => o.value === value),
        0,
      ),
    );
    setOpen(true);
  }

  // Closes without touching focus: for when focus has already moved elsewhere.
  function dismiss() {
    setOpen(false);
    setQuery("");
  }

  function close() {
    dismiss();
    trigger.current?.focus();
  }

  function pick(index: number) {
    const option = shown[index];
    if (option) onChange(option.value);
    close();
  }

  // Typing letters on a short list jumps to the option that starts with them, like a native select.
  // On a long list the first letter opens the menu with that letter in the search box.
  function jump(char: string) {
    if (searchable) return show(char);
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
    else onChange(options[index]?.value ?? value);
  }

  const keys = {
    open,
    active,
    last: shown.length - 1,
    setActive,
    show,
    close,
    pick,
    jump,
  };
  return {
    open,
    active,
    setActive,
    shown,
    selected,
    chosen,
    query,
    searchable,
    show,
    dismiss,
    close,
    pick,
    search: (text: string) => {
      setQuery(text);
      setActive(0);
    },
    onKeyDown: (e: KeyboardEvent) => triggerKeys(keys, e),
    onSearchKeyDown: (e: KeyboardEvent) => searchKeys(keys, e),
  };
}
