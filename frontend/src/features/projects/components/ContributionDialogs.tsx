import { useState } from "react";
import type { Contribution } from "../api";
import { DeleteContributionDialog } from "./DeleteContributionDialog";
import { EditContributionSheet } from "./EditContributionSheet";
import { ViewContributionSheet } from "./ViewContributionSheet";

// Where a contribution can be opened at: read only, to change, or to delete.
export type ContributionStart = "view" | "edit" | "delete";

type Props = {
  contribution: Contribution;
  start: ContributionStart;
  onClose: () => void;
};

// Everything that can happen to one existing contribution, one step at a time: it opens to be read, and from there
// it can be edited or deleted. The ⋯ menu jumps straight to either.
export function ContributionDialogs({ contribution, start, onClose }: Props) {
  const [step, setStep] = useState<ContributionStart>(start);
  if (step === "view")
    return (
      <ViewContributionSheet
        contribution={contribution}
        onEdit={() => setStep("edit")}
        onDelete={() => setStep("delete")}
        onClose={onClose}
      />
    );
  if (step === "delete")
    return (
      <DeleteContributionDialog contribution={contribution} onClose={onClose} />
    );
  return (
    <EditContributionSheet contribution={contribution} onClose={onClose} />
  );
}
