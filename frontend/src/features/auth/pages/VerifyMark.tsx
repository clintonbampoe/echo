import { StatusTile } from "@/ui";
import type { VerifyPhase } from "../model/verifyPhase";
import styles from "./VerifyMark.module.css";

const ICON = { verified: "check", sending: "spinner", failed: "mail" } as const;

// The tile at the top of the verify page. While the link is out it breathes (a soft pulse says "listening").
// When the account is verified it fills, the tick springs in and a ring leaves it, once.
export function VerifyMark({ phase }: { phase: VerifyPhase }) {
  const verified = phase === "verified";
  const listening = phase === "waiting" || phase === "slow";
  const icon = phase in ICON ? ICON[phase as keyof typeof ICON] : "mail";
  return (
    <div className={styles.mark}>
      <StatusTile icon={icon} on={verified} />
      {listening && (
        <>
          <span className={styles.pulse} aria-hidden="true" />
          <span
            className={`${styles.pulse} ${styles.late}`}
            aria-hidden="true"
          />
        </>
      )}
      {verified && (
        <>
          <span className={styles.ripple} aria-hidden="true" />
          <span
            className={`${styles.ripple} ${styles.later}`}
            aria-hidden="true"
          />
        </>
      )}
    </div>
  );
}
