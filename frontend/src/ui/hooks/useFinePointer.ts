import { useMedia } from "./useMedia";

// True when the main pointer is a mouse or trackpad, false on a touch screen.
// Where matchMedia does not exist (some test environments), assume a mouse.
export const useFinePointer = () =>
  useMedia("(hover: hover) and (pointer: fine)", true);
