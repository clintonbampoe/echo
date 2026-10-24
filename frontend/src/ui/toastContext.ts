import { createContext, useContext, type Dispatch, type SetStateAction } from "react";

export type ToastInput = {
  message: string;
  // A small button on the toast, e.g. "Undo". It closes the toast.
  action?: { label: string; onClick: () => void };
};

export type Slot = { toast: (ToastInput & { id: number }) | null; open: boolean };

export const ToastContext = createContext<Dispatch<SetStateAction<Slot>> | null>(
  null,
);

let counter = 0;

// `const toast = useToast(); toast({ message: "Member added" })`
export function useToast() {
  const set = useContext(ToastContext);
  return (input: ToastInput) =>
    set?.({ toast: { ...input, id: ++counter }, open: true });
}
