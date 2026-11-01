import { ActionMenu, Button } from "@/ui";
import type { Setup } from "../../hooks/useSetup";
import { SetupBar } from "./SetupBar";
import { SetupSteps } from "./SetupSteps";
import styles from "./GettingStarted.module.css";

// The card on a wide screen: the whole list, and the next step with the one filled button. Once everything is
// done it says so, and offers to go away.
export function SetupWide({ setup }: { setup: Setup }) {
  const total = setup.steps.length;
  const finished = !setup.next;
  return (
    <section
      aria-labelledby="getting-started"
      className={`${styles.card} anim-rise`}
    >
      <header className={styles.head}>
        <div className={styles.titles}>
          <h2 id="getting-started" className={styles.title}>
            {finished ? "You are set up" : "Getting started"}
          </h2>
          <p className={styles.sub}>
            {finished
              ? "Echo has what it needs. Your dashboard is working from real records now."
              : "Add a little real data and the dashboard fills in."}
          </p>
        </div>
        <span className={styles.count}>
          {setup.done} of {total}
        </span>
        <SetupBar done={setup.done} of={total} />
        {!finished && (
          <ActionMenu
            label="Getting started options"
            title="Getting started"
            items={[{ label: "Hide Getting started", onSelect: setup.dismiss }]}
          />
        )}
      </header>
      <SetupSteps setup={setup} />
      {finished && (
        <footer className={styles.foot}>
          <Button pill onClick={setup.dismiss}>
            Hide this card
          </Button>
        </footer>
      )}
    </section>
  );
}
