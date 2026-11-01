import { useState } from "react";
import { Button, CenterPanel, LinkButton } from "@/ui";
import type { Setup } from "../../hooks/useSetup";
import type { SetupStep } from "../../model/setup";
import { SetupBar } from "./SetupBar";
import { SetupSteps } from "./SetupSteps";
import styles from "./GettingStarted.module.css";

type Props = { setup: Setup; next: SetupStep };

// The card on a phone shows only the step to do now. "See all" opens every step in a centred panel, which is
// also where the card can be hidden.
export function SetupNarrow({ setup, next }: Props) {
  const [all, setAll] = useState(false);
  const total = setup.steps.length;
  return (
    <section
      aria-labelledby="getting-started"
      className={`${styles.card} ${styles.narrow} anim-rise`}
    >
      <header className={styles.narrowHead}>
        <h2 id="getting-started" className={styles.title}>
          Getting started
        </h2>
        <span className={styles.count}>
          {setup.done} of {total}
        </span>
      </header>
      <SetupBar done={setup.done} of={total} />
      <div className={styles.one}>
        <span className={styles.ring} aria-hidden="true">
          <span className={styles.ringDot} />
        </span>
        <div>
          <p className={styles.oneTitle}>{next.title}</p>
          <p className={styles.oneText}>{next.text}</p>
        </div>
      </div>
      <LinkButton to={next.to} size="xl" pill block className={styles.big}>
        {next.action}
      </LinkButton>
      <button
        type="button"
        className={styles.seeAll}
        onClick={() => setAll(true)}
      >
        See all {total} steps
      </button>
      <CenterPanel
        open={all}
        onClose={() => setAll(false)}
        title="Getting started"
      >
        <p className={styles.panelCount}>
          {setup.done} of {total} done
        </p>
        <SetupBar done={setup.done} of={total} />
        <SetupSteps setup={setup} dense />
        <div className={styles.panelFoot}>
          <Button variant="ghost" pill onClick={setup.dismiss}>
            Hide Getting started
          </Button>
        </div>
      </CenterPanel>
    </section>
  );
}
