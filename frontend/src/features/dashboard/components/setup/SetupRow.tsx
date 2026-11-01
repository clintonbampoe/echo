import { Link } from "react-router-dom";
import { Icon, LinkButton } from "@/ui";
import type { SetupStep } from "../../model/setup";
import styles from "./SetupRow.module.css";

// The round mark at the start of a step: empty until it is done, then filled with a tick that springs in.
function Node({ done }: { done: boolean }) {
  return (
    <span
      className={styles.node}
      data-done={done || undefined}
      role={done ? "img" : undefined}
      aria-label={done ? "Done" : undefined}
      aria-hidden={done ? undefined : "true"}
    >
      {done && <Icon name="check" size={14} />}
    </span>
  );
}

type Props = { step: SetupStep; next: boolean };

// One step. A done step is quiet. The next step carries the card's only filled button. Any other step is
// pressable as a whole row, so the press area is the row and not just a word.
export function SetupRow({ step, next }: Props) {
  const body = (
    <>
      <Node done={step.done} />
      <span className={styles.text}>
        <span className={styles.title}>{step.title}</span>
        {!step.done && <span className={styles.sub}>{step.text}</span>}
      </span>
    </>
  );
  if (step.done)
    return (
      <li className={styles.row} data-done>
        {body}
      </li>
    );
  if (next)
    return (
      <li className={styles.row} data-next>
        {body}
        <LinkButton to={step.to} size="md" pill className={styles.action}>
          {step.action}
        </LinkButton>
      </li>
    );
  return (
    <li className={styles.item}>
      <Link to={step.to} className={`${styles.row} ${styles.link}`}>
        {body}
        <Icon name="arrow-right" size={16} className={styles.go} />
      </Link>
    </li>
  );
}
