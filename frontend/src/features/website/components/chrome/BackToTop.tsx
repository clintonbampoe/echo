import { useEffect, useState } from "react";
import styles from "./BackToTop.module.css";

// A round button that appears once the page has been scrolled. A ring around it fills as you scroll
// down; pressing it goes back to the top.
export function BackToTop() {
  const [shown, setShown] = useState(false);
  useEffect(() => {
    const update = () => setShown(window.scrollY > 600);
    update();
    window.addEventListener("scroll", update, { passive: true });
    return () => window.removeEventListener("scroll", update);
  }, []);
  return (
    <button
      type="button"
      className={styles.top}
      data-shown={shown || undefined}
      aria-label="Back to top"
      tabIndex={shown ? 0 : -1}
      onClick={() => window.scrollTo({ top: 0, behavior: "smooth" })}
    >
      <svg
        className={styles.ring}
        width="52"
        height="52"
        viewBox="0 0 52 52"
        fill="none"
        aria-hidden="true"
      >
        <circle
          cx="26"
          cy="26"
          r="25"
          stroke="currentColor"
          strokeOpacity=".14"
          strokeWidth="2"
        />
        <circle
          className={styles.fill}
          cx="26"
          cy="26"
          r="25"
          stroke="currentColor"
          strokeWidth="2"
          strokeLinecap="round"
          pathLength="100"
        />
      </svg>
      <svg
        className={styles.arrow}
        width="18"
        height="18"
        viewBox="0 0 20 20"
        fill="none"
        stroke="currentColor"
        strokeWidth="1.8"
        strokeLinecap="round"
        strokeLinejoin="round"
        aria-hidden="true"
      >
        <path d="M10 16V4M5 9l5-5 5 5" />
      </svg>
    </button>
  );
}
