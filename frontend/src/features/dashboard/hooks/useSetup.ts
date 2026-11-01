import { useCongregation, useMe } from "@/features/account";
import {
  factsKnown,
  nextStep,
  nothingYet,
  setupState,
  setupSteps,
  stepsDone,
} from "../model/setup";
import { useSetupFacts } from "./setup";
import { useSetupDismissed } from "./useSetupDismissed";

// Where an admin's Getting started stands. "off" for anyone else, or once hidden on this device; "loading" until
// every answer is in; "on" with the steps. `empty` says the church has recorded nothing yet, so the dashboard
// greets it instead of showing charts of zeros.
export function useSetup() {
  const admin = useMe().data?.role === "Admin";
  const church = useCongregation().data;
  const [dismissed, dismiss] = useSetupDismissed(church?.id);
  const facts = useSetupFacts(admin);
  const steps = setupSteps(facts, church?.name ?? "Your church");
  const ready = factsKnown(facts) && !!church;
  const state = setupState(admin, dismissed, ready);
  return {
    state,
    steps,
    next: nextStep(steps),
    done: stepsDone(steps),
    empty: state === "on" && nothingYet(facts),
    churchName: church?.name ?? "",
    dismiss,
  };
}

export type Setup = ReturnType<typeof useSetup>;
