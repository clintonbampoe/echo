import { useMedia } from "@/ui";
import type { Setup } from "../../hooks/useSetup";
import { SetupNarrow } from "./SetupNarrow";
import { SetupWide } from "./SetupWide";
import styles from "./GettingStarted.module.css";

// The Getting started card of an admin's dashboard. It waits for every answer before it appears (the numbers
// below it show their own loading meanwhile); for anyone else, or once hidden, it is nothing.
export function GettingStarted({ setup }: { setup: Setup }) {
  const phone = useMedia("(max-width: 700px)");
  if (setup.state !== "on") return null;
  if (phone && setup.next)
    return <SetupNarrow setup={setup} next={setup.next} />;
  return <SetupWide setup={setup} />;
}
