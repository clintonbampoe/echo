import { useState, type ReactNode } from "react";
import type { CheckIn } from "../api";
import { EditCheckInTimeSheet, RemoveCheckInDialog } from "./CheckInDialogs";
import { CheckInPersonPanel } from "./CheckInPersonPanel";

// Opening a check-in is the same three steps wherever it is listed: the panel that reads it, the sheet that
// changes its time, and the dialog that undoes it. A list keeps `panels` on the page and calls `open` on a row.
export function useCheckInPanels(): {
  open: (checkIn: CheckIn) => void;
  edit: (checkIn: CheckIn) => void;
  remove: (checkIn: CheckIn) => void;
  panels: ReactNode;
} {
  const [viewing, setViewing] = useState<CheckIn | null>(null);
  const [editing, setEditing] = useState<CheckIn | null>(null);
  const [removing, setRemoving] = useState<CheckIn | null>(null);

  const panels = (
    <>
      {viewing && (
        <CheckInPersonPanel
          checkIn={viewing}
          onClose={() => setViewing(null)}
          onEdit={() => {
            setEditing(viewing);
            setViewing(null);
          }}
          onRemove={() => {
            setRemoving(viewing);
            setViewing(null);
          }}
        />
      )}
      {editing && (
        <EditCheckInTimeSheet
          checkIn={editing}
          onClose={() => setEditing(null)}
        />
      )}
      {removing && (
        <RemoveCheckInDialog
          checkIn={removing}
          onClose={() => setRemoving(null)}
        />
      )}
    </>
  );

  return { open: setViewing, edit: setEditing, remove: setRemoving, panels };
}
