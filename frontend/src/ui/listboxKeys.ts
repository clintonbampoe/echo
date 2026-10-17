import type { KeyboardEvent } from "react";

export type Keys = {
  open: boolean;
  active: number;
  last: number;
  setActive: (index: number) => void;
  show: () => void;
  close: () => void;
  pick: (index: number) => void;
  // A letter typed on the trigger.
  jump: (char: string) => void;
};

function run(actions: Record<string, () => void>, e: KeyboardEvent) {
  const action = actions[e.key];
  if (!action) return false;
  e.preventDefault();
  action();
  return true;
}

// The keys of a select's trigger button. Letters jump to a matching option or open the search.
export function triggerKeys(k: Keys, e: KeyboardEvent) {
  if (e.key === "Escape" && !k.open) return;
  const done = run(
    {
      ArrowDown: () =>
        k.open ? k.setActive(Math.min(k.active + 1, k.last)) : k.show(),
      ArrowUp: () =>
        k.open ? k.setActive(Math.max(k.active - 1, 0)) : k.show(),
      Home: () => k.open && k.setActive(0),
      End: () => k.open && k.setActive(k.last),
      Enter: () => (k.open ? k.pick(k.active) : k.show()),
      " ": () => (k.open ? k.pick(k.active) : k.show()),
      Escape: k.close,
    },
    e,
  );
  if (done || e.key.length !== 1 || e.ctrlKey || e.metaKey) return;
  // The letter is handed over by hand, so it must not also be typed into a search box that takes focus next.
  e.preventDefault();
  k.jump(e.key);
}

// The keys while the search box has focus. Home, End and space belong to the text.
export function searchKeys(k: Keys, e: KeyboardEvent) {
  run(
    {
      ArrowDown: () => k.setActive(Math.min(k.active + 1, k.last)),
      ArrowUp: () => k.setActive(Math.max(k.active - 1, 0)),
      Enter: () => k.pick(k.active),
      Escape: k.close,
    },
    e,
  );
}
