import { useSyncExternalStore } from "react";

// True when the main pointer is a mouse or trackpad, false on a touch screen.
const query = "(hover: hover) and (pointer: fine)";
const supported = () => typeof window.matchMedia === "function";

function subscribe(onChange: () => void) {
  if (!supported()) return () => {};
  const list = window.matchMedia(query);
  list.addEventListener("change", onChange);
  return () => list.removeEventListener("change", onChange);
}

// Where matchMedia does not exist (some test environments), assume a mouse.
const read = () => !supported() || window.matchMedia(query).matches;

export function useFinePointer() {
  return useSyncExternalStore(subscribe, read);
}
