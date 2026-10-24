import { useTheme } from "./theme";
import styles from "./ThemeToggle.module.css";

// A round moon/sun button. Shows what you will switch to.
export function ThemeToggle({ className }: { className?: string }) {
  const { theme, toggle } = useTheme();
  const toDark = theme === "light";
  return (
    <button
      type="button"
      onClick={toggle}
      className={[styles.toggle, className].filter(Boolean).join(" ")}
      aria-label={toDark ? "Switch to dark mode" : "Switch to light mode"}
    >
      <svg
        width="17"
        height="17"
        viewBox="0 0 20 20"
        fill="none"
        stroke="currentColor"
        strokeWidth="1.6"
        strokeLinecap="round"
        strokeLinejoin="round"
        aria-hidden="true"
      >
        {toDark ? (
          <path d="M16 11.5A6.5 6.5 0 018.5 4a6.5 6.5 0 107.5 7.5z" />
        ) : (
          <>
            <circle cx="10" cy="10" r="3.5" />
            <path d="M10 2.5v1.8M10 15.7v1.8M2.5 10h1.8M15.7 10h1.8M4.7 4.7l1.3 1.3M14 14l1.3 1.3M15.3 4.7L14 6M6 14l-1.3 1.3" />
          </>
        )}
      </svg>
    </button>
  );
}
