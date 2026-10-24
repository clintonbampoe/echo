// Settings kept on this device: how roomy lists are, whether animations are calmer, how money is written.
// No React in this file; ui and pages read it through usePreferences.
import { useSyncExternalStore } from "react";

export type Density = "compact" | "comfortable" | "spacious";

export type Preferences = {
  density: Density;
  // Calmer animations. On by itself when the device asks for less motion.
  reduceMotion: boolean;
  // "GH₵ 6,400" instead of "6,400".
  currencySymbol: boolean;
};

const KEY = "echo.preferences";

const defaults: Preferences = {
  density: "comfortable",
  reduceMotion: false,
  currencySymbol: true,
};

function read(): Preferences {
  try {
    const saved: unknown = JSON.parse(localStorage.getItem(KEY) ?? "{}");
    return { ...defaults, ...(typeof saved === "object" ? saved : {}) };
  } catch {
    return defaults;
  }
}

let current = read();
const listeners = new Set<() => void>();

// Puts the choices where CSS can see them: attributes on <html>.
export function applyPreferences(p: Preferences = current) {
  const root = document.documentElement;
  root.dataset.density = p.density;
  root.dataset.reduceMotion = String(p.reduceMotion);
}

export const getPreferences = () => current;

export function setPreferences(patch: Partial<Preferences>) {
  current = { ...current, ...patch };
  try {
    localStorage.setItem(KEY, JSON.stringify(current));
  } catch {
    // A full or blocked store only means the choice is forgotten on reload.
  }
  applyPreferences(current);
  listeners.forEach((l) => l());
}

export function usePreferences() {
  const prefs = useSyncExternalStore((l) => {
    listeners.add(l);
    return () => listeners.delete(l);
  }, getPreferences);
  return [prefs, setPreferences] as const;
}

// Test hook: forget what was chosen.
export function resetPreferences() {
  current = defaults;
  localStorage.removeItem(KEY);
  applyPreferences(current);
}
