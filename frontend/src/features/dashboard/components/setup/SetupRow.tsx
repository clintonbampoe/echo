import { Link } from "react-router-dom";
import { Icon, LinkButton } from "@/ui";
import { stepLine, type SetupStep } from "../../model/setup";
import styles from "./SetupRow.module.css";

export type RowState = "done" | "next" | "todo";

// The round mark at the start of a step: a dark disc with a tick that springs in when done; a ringed dot for
// the step to do now; an empty ring for the ones that wait.
function Node({ state }: { state: RowState }) {
  return (
    <span
      className={styles.node}
      data-state={state}
      role={state === "done" ? "img" : undefined}
      aria-label={state === "done" ? "Done" : undefined}
      aria-hidden={state === "done" ? undefined : "true"}
    >
      {state === "done" && <Icon name="check" size={14} />}
      {state === "next" && <span className={styles.dot} />}
    </span>
  );
}

type Props = {
  step: SetupStep;
  state: RowState;
  // The slimmer rows of the panel a phone opens: no buttons, a chevron on the next step instead.
  dense?: boolean;
  // A line under a done step's title. Only when it is the one thing done so far.
  note?: boolean;
};

// One step. A done step is quiet. On a wide screen the next step carries the card's only filled button; every
// other step that waits is pressable as a whole row, so the press area is the row and not just a word.
export function SetupRow({ step, state, dense, note }: Props) {
  const sub = stepLine(state, step.text, !!dense, !!note);
  const body = (
    <>
      <Node state={state} />
      <span className={styles.text}>
        <span className={styles.title} data-state={state}>
          {step.title}
        </span>
        {sub && <span className={styles.sub}>{sub}</span>}
      </span>
    </>
  );
  const row = `${styles.row} ${dense ? styles.dense : ""}`;
  if (state === "done")
    return (
      <li>
        <div className={row}>{body}</div>
      </li>
    );
  if (state === "next" && !dense)
    return (
      <li>
        <div className={row} data-next>
          {body}
          <LinkButton to={step.to} size="md" pill className={styles.action}>
            {step.action}
          </LinkButton>
        </div>
      </li>
    );
  return (
    <li>
      <Link
        to={step.to}
        className={`${row} ${styles.link}`}
        data-next={state === "next" || undefined}
      >
        {body}
        <Icon
          name="chevron-right"
          size={18}
          className={styles.go}
          data-state={state}
        />
      </Link>
    </li>
  );
}
