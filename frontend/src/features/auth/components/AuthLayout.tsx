import type { ReactNode } from "react";
import { useLocation } from "react-router-dom";
import { Atmosphere, PulseLine, useMedia, usePublicSurface } from "@/ui";
import { AuthTopBar } from "./AuthTopBar";
import { SPLIT_QUERY, SplitContext } from "./splitMode";
import styles from "./AuthLayout.module.css";

// Every auth screen sits in this frame: the slow light behind, the screen in the middle and the
// promise at the foot. A tablet or phone also gets a top bar (a wide screen carries the way back to the
// site and the light/dark button in its own rail).
// Screens never build their own page chrome.
export function AuthLayout({ children }: { children: ReactNode }) {
  usePublicSurface();
  const { pathname } = useLocation();
  const split = useMedia(SPLIT_QUERY, true);
  return (
    <SplitContext.Provider value={split}>
      <div className={`${styles.page} ${split ? styles.split : ""}`}>
        <Atmosphere />
        {!split && <AuthTopBar />}
        <main className={styles.main}>
          {/* Keyed by path: each new page rises into place while the frame stays put. */}
          <div key={pathname} className={`anim-rise ${styles.slot}`}>
            {children}
          </div>
        </main>
        {!split && (
          <footer className={styles.foot}>
            <PulseLine />
          </footer>
        )}
      </div>
    </SplitContext.Provider>
  );
}
