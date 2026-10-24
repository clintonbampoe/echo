import type { ReactNode } from "react";
import { LeavingContext } from "./presenceContext";
import { usePresence } from "./usePresence";

type Props = {
  open: boolean;
  // How long the exit animation of a sheet or dialog inside runs.
  ms?: number;
  children: ReactNode;
};

// Keeps a sheet or dialog on the page while it plays its exit animation. The thing inside is written as it
// always was, mounted fresh each time it opens; it learns it is leaving from here.
//
//   <Presence open={editing !== null}>{held && <EditSheet member={held} />}</Presence>
export function Presence({ open, ms = 220, children }: Props) {
  const { mounted, closing } = usePresence(open, ms);
  if (!mounted) return null;
  return (
    <LeavingContext.Provider value={closing}>{children}</LeavingContext.Provider>
  );
}
