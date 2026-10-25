import type { ReactNode } from "react";
import { Link, useLocation } from "react-router-dom";
import { Atmosphere, Icon, ThemeToggle, usePublicSurface } from "@/ui";
import styles from "./AuthLayout.module.css";

// Every auth screen sits in this frame: the slow light behind, a light/dark button and a way
// home at the edges, and the screen in the middle. Screens never build their own page chrome.
export function AuthLayout({ children }: { children: ReactNode }) {
  usePublicSurface();
  const { pathname } = useLocation();
  return (
    <div className={styles.page}>
      <Atmosphere />
      <Link to="/" className={styles.home}>
        <Icon name="arrow-right" size={16} className={styles.flip} />
        Back to home
      </Link>
      <ThemeToggle className={styles.theme} />
      <main className={styles.main}>
        {/* Keyed by path: each new page rises into place while the frame stays put. */}
        <div key={pathname} className="anim-rise">
          {children}
        </div>
      </main>
    </div>
  );
}
