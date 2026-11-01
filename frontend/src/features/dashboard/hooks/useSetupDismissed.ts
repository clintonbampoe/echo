import { useState } from "react";

const key = (congregationId: string) =>
  `echo.setup.dismissed.${congregationId}`;

const read = (congregationId: string) => {
  try {
    return localStorage.getItem(key(congregationId)) === "1";
  } catch {
    return false;
  }
};

// Whether this church's Getting started card has been hidden on this device, and a way to hide it. Kept per
// church, so an admin of two churches decides for each. Storage can be blocked; then it simply comes back.
export function useSetupDismissed(congregationId: string | undefined) {
  const [hidden, setHidden] = useState<string[]>([]);
  const id = congregationId ?? "";
  const dismissed = id !== "" && (hidden.includes(id) || read(id));
  const dismiss = () => {
    if (!id) return;
    setHidden((was) => [...was, id]);
    try {
      localStorage.setItem(key(id), "1");
    } catch {
      // Hidden for this visit only.
    }
  };
  return [dismissed, dismiss] as const;
}
