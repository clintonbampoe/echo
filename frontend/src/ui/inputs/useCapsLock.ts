import { useState, type FocusEvent, type KeyboardEvent } from "react";

// Whether Caps Lock is on while someone types in a field, plus the handlers that find out.
export function useCapsLock(
  onBlur?: (e: FocusEvent<HTMLInputElement>) => void,
) {
  const [caps, setCaps] = useState(false);
  const read = (e: KeyboardEvent<HTMLInputElement>) =>
    setCaps(e.getModifierState("CapsLock"));
  return {
    caps,
    handlers: {
      onKeyDown: read,
      onKeyUp: read,
      onBlur: (e: FocusEvent<HTMLInputElement>) => {
        setCaps(false);
        onBlur?.(e);
      },
    },
  };
}
