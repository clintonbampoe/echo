import { useSyncExternalStore } from "react";

export type Theme = "light" | "dark";

const KEY = "echo-theme";
const listeners = new Set<() => void>();

function stored(): Theme | null {
  try {
    const value = localStorage.getItem(KEY);
    return value === "light" || value === "dark" ? value : null;
  } catch {
    return null;
  }
}

function systemTheme(): Theme {
  return typeof matchMedia === "function" &&
    matchMedia("(prefers-color-scheme: dark)").matches
    ? "dark"
    : "light";
}

function current(): Theme {
  return stored() ?? systemTheme();
}

function subscribe(listener: () => void) {
  listeners.add(listener);
  const media =
    typeof matchMedia === "function"
      ? matchMedia("(prefers-color-scheme: dark)")
      : null;
  media?.addEventListener("change", listener);
  return () => {
    listeners.delete(listener);
    media?.removeEventListener("change", listener);
  };
}

function choose(theme: Theme) {
  try {
    localStorage.setItem(KEY, theme);
  } catch {
    // Private mode: the choice lasts until the page is closed, which is fine.
  }
  listeners.forEach((l) => l());
}

// The light or dark choice for the public pages (website and auth). The person's own
// choice wins; until they make one it follows the device. The signed-in app is light only.
export function useTheme() {
  const theme = useSyncExternalStore(subscribe, current);
  const toggle = () => choose(theme === "dark" ? "light" : "dark");
  return { theme, toggle };
}
