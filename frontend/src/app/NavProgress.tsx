import { useNavigation } from "react-router-dom";
import styles from "./NavProgress.module.css";

// A thin bar across the top while the next page is on its way, so a slow click is never just a pause. It only
// shows after a short wait, so a fast page never flickers it.
export function NavProgress() {
  const busy = useNavigation().state !== "idle";
  if (!busy) return null;
  return (
    <div role="progressbar" aria-label="Loading page" className={styles.bar}>
      <span className={styles.run} />
    </div>
  );
}
