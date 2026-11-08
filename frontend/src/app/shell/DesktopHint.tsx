import { useState } from "react";
import { Button, StatusTile, useMedia } from "@/ui";
import styles from "./DesktopHint.module.css";

const KEY = "echo.desktopHint.shownAt";
// A dismissed hint stays away this long, then comes back once. Long enough not to nag, short enough not to be forgotten.
const QUIET_DAYS = 3;
const DAY = 24 * 60 * 60 * 1000;

const quiet = () => {
  try {
    const at = Number(localStorage.getItem(KEY));
    return at > 0 && Date.now() - at < QUIET_DAYS * DAY;
  } catch {
    return false;
  }
};

// Every page is built for a desktop first. Until each role sees only the pages that suit it (and members are linked to
// their accounts), a tablet or phone shows the same pages with less room, so this says so every few days until it is
// dismissed. Remove it when role-based access lands.
export function DesktopHint() {
  const small = useMedia("(max-width: 900px)");
  const [hidden, setHidden] = useState(quiet);
  if (!small || hidden) return null;

  const close = () => {
    try {
      localStorage.setItem(KEY, String(Date.now()));
    } catch {
      // Without storage the hint simply comes back on the next page load.
    }
    setHidden(true);
  };

  return (
    <section aria-label="Best on a desktop" className={`${styles.hint} pop`}>
      <StatusTile icon="dashboard" size={40} />
      <div className={styles.words}>
        <p className={styles.title}>Best on a desktop</p>
        <p className={styles.text}>
          Echo works here too, but it is easier on a computer.
        </p>
      </div>
      <Button variant="ghost" size="sm" onClick={close}>
        Got it
      </Button>
    </section>
  );
}
