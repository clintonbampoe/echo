import { useState } from "react";
import { Button, StatusTile, useMedia } from "@/ui";
import styles from "./DesktopHint.module.css";

const KEY = "echo.desktopHint.dismissed";

const dismissed = () => {
  try {
    return sessionStorage.getItem(KEY) === "1";
  } catch {
    return false;
  }
};

// Every page is built for a desktop first. Until each role sees only the pages that suit it (and members are linked to
// their accounts), a tablet or phone shows the same pages with less room, so this says so once per visit. Remove it
// when role-based access lands.
export function DesktopHint() {
  const small = useMedia("(max-width: 900px)");
  const [hidden, setHidden] = useState(dismissed);
  if (!small || hidden) return null;

  const close = () => {
    try {
      sessionStorage.setItem(KEY, "1");
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
