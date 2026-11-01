import { Navigate } from "react-router-dom";
import { LinkButton, Icon } from "@/ui";
import { useAutoContinue } from "./useAutoContinue";
import styles from "./VerifySentPage.module.css";

const CONTINUE_MS = 3000;

type Props = { signedIn: boolean; leave: string };

// What sits under the address once the account is verified: the way on, and a short countdown that takes the
// person there by itself. Hovering or focusing the card pauses it, and "Stay here" ends it.
export function VerifyDone({ signedIn, leave }: Props) {
  const next = useAutoContinue(CONTINUE_MS);
  if (next.done) return <Navigate to={leave} replace />;

  return (
    <div
      className={styles.done}
      onMouseEnter={() => next.pause(true)}
      onMouseLeave={() => next.pause(false)}
      onFocus={() => next.pause(true)}
      onBlur={() => next.pause(false)}
    >
      <LinkButton to={leave} replace size="lg" pill block>
        {signedIn ? "Go to the dashboard" : "Sign in"}
        <Icon name="arrow-right" size={16} />
      </LinkButton>
      {!next.stopped && (
        <div className={styles.countdown}>
          <div className={styles.track} aria-hidden="true">
            <div
              className={styles.fill}
              style={{ transform: `scaleX(${next.progress})` }}
            />
          </div>
          <span>Continuing in {next.seconds}</span>
          <button type="button" className={styles.stay} onClick={next.stop}>
            Stay here
          </button>
        </div>
      )}
    </div>
  );
}
