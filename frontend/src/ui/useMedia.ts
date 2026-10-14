import { useSyncExternalStore } from "react";

const supported = () => typeof window.matchMedia === "function";

// True while the screen matches a CSS media query, e.g. useMedia('(max-width: 1180px)'). It updates when
// the window is resized or the tablet is turned. Where matchMedia does not exist (some test
// environments) it answers `fallback`, which is false unless you say otherwise.
export function useMedia(query: string, fallback = false) {
  const subscribe = (onChange: () => void) => {
    if (!supported()) return () => {};
    const list = window.matchMedia(query);
    list.addEventListener("change", onChange);
    return () => list.removeEventListener("change", onChange);
  };
  return useSyncExternalStore(subscribe, () =>
    supported() ? window.matchMedia(query).matches : fallback,
  );
}
