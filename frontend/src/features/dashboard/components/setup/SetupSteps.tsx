import type { Setup } from "../../hooks/useSetup";
import { SetupRow, type RowState } from "./SetupRow";
import styles from "./GettingStarted.module.css";

// The steps, each in the state it is in. Only one step is ever "next": the first not done.
export function SetupSteps({
  setup,
  dense,
}: {
  setup: Setup;
  dense?: boolean;
}) {
  return (
    <ul className={styles.list}>
      {setup.steps.map((s) => {
        const state: RowState = s.done
          ? "done"
          : s === setup.next
            ? "next"
            : "todo";
        return (
          <SetupRow
            key={s.key}
            step={s}
            state={state}
            dense={dense}
            note={setup.done === 1}
          />
        );
      })}
    </ul>
  );
}
