import { useState } from "react";
import { useCongregation, useMe } from "@/features/account";
import { Button, CenterPanel, ProgressBar, useMedia } from "@/ui";
import { useSetupDismissed } from "../../hooks/useSetupDismissed";
import { useSetupFacts } from "../../hooks/setup";
import { factsKnown, nextStep, setupSteps, stepsDone } from "../../model/setup";
import { SetupRow } from "./SetupRow";
import styles from "./GettingStarted.module.css";

// The card that walks an admin through setting up a new church. It asks nothing until it knows the person is an
// admin, waits for every answer so it never flashes, and goes for good once hidden on this device.
export function GettingStarted() {
  const role = useMe().data?.role;
  if (role !== "Admin") return null;
  return <Card />;
}

function Card() {
  const church = useCongregation().data;
  const [dismissed, dismiss] = useSetupDismissed(church?.id);
  const facts = useSetupFacts();
  const phone = useMedia("(max-width: 700px)");
  const [all, setAll] = useState(false);

  if (dismissed || !church || !factsKnown(facts)) return null;
  const steps = setupSteps(facts);
  const next = nextStep(steps);
  const done = stepsDone(steps);

  const list = (
    <ul className={styles.list}>
      {steps.map((s) => (
        <SetupRow key={s.key} step={s} next={s === next} />
      ))}
    </ul>
  );

  return (
    <section
      aria-labelledby="getting-started"
      className={`${styles.card} anim-rise`}
    >
      <header className={styles.head}>
        <div>
          <h2 id="getting-started" className={styles.title}>
            {next ? "Get your church set up" : "You are set up"}
          </h2>
          <p className={styles.sub}>
            {next
              ? `${done} of ${steps.length} done`
              : "Everything is in place. You can hide this card."}
          </p>
        </div>
        {!next && (
          <Button variant="secondary" pill onClick={dismiss}>
            Hide this card
          </Button>
        )}
      </header>
      <ProgressBar value={done / steps.length} label="Setup progress" />
      {phone && next ? (
        <>
          <ul className={styles.list}>
            <SetupRow step={next} next />
          </ul>
          <Button variant="ghost" pill onClick={() => setAll(true)}>
            See all {steps.length} steps
          </Button>
          <CenterPanel
            open={all}
            onClose={() => setAll(false)}
            title="Your steps"
          >
            {list}
          </CenterPanel>
        </>
      ) : (
        list
      )}
    </section>
  );
}
