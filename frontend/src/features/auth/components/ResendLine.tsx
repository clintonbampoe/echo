import { clock } from "../model/clock";
import styles from "./ResendButton.module.css";

type Props = {
  // Seconds before another link may be sent; 0 when ready.
  wait: number;
  onSend: () => void;
};

// The quiet way to ask for another link: a line of words with the action as an underlined link, then a
// countdown. The page owns the sending and the countdown, so the link that went out as the page opened
// counts too.
export function ResendLine({ wait, onSend }: Props) {
  return (
    <p className={styles.line} aria-live="polite">
      {wait > 0 ? (
        `Send another link in ${clock(wait)}`
      ) : (
        <>
          Did not get it?{" "}
          <button type="button" className={styles.link} onClick={onSend}>
            Send another link
          </button>
        </>
      )}
    </p>
  );
}
