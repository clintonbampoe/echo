import { useState } from "react";
import type { Tithe } from "../../api/tithes";
import { DeleteTitheDialog } from "./DeleteTitheDialog";
import { EditTitheSheet } from "../form/EditTitheSheet";
import { ViewTitheSheet } from "../profile/ViewTitheSheet";

// Where a tithe can be opened at: read only, to change, to top up (the day starts as today), or to delete.
export type TitheStart = "view" | "edit" | "top-up" | "delete";

type Props = {
  tithe: Tithe;
  start: TitheStart;
  today: string;
  onClose: () => void;
};

// Everything that can happen to one existing tithe, one step at a time: a tithe opens to be read, and from there
// it can be edited or deleted.
export function TitheDialogs({ tithe, start, today, onClose }: Props) {
  const [step, setStep] = useState<TitheStart>(start);
  if (step === "view")
    return (
      <ViewTitheSheet
        tithe={tithe}
        onEdit={() => setStep("edit")}
        onDelete={() => setStep("delete")}
        onClose={onClose}
      />
    );
  if (step === "delete")
    return <DeleteTitheDialog tithe={tithe} onClose={onClose} />;
  return (
    <EditTitheSheet
      tithe={tithe}
      topUp={step === "top-up"}
      today={today}
      onClose={onClose}
    />
  );
}
