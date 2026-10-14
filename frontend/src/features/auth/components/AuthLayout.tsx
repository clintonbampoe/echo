import type { ReactNode } from "react";
import { useLocation } from "react-router-dom";
import { getVerse } from "@/lib/verses";
import { Quote, useSurface } from "@/ui";
import styles from "./AuthLayout.module.css";

// Every auth screen sits in this frame: a quiet verse on the left (large screens only) and
// the screen's content on the right. Screens never build their own page chrome.
export function AuthLayout({ children }: { children: ReactNode }) {
  useSurface("dark");
  const { pathname } = useLocation();
  const verse = getVerse();
  return (
    <div className={styles.page}>
      <section className={styles.visual}>
        <div className={`${styles.light} anim-settle`} aria-hidden="true">
          <div className={styles.glowA} />
          <div className={styles.glowB} />
          <div className={styles.vignette} />
        </div>
        <svg aria-hidden="true" className={styles.grain}>
          <filter id="grain">
            <feTurbulence
              type="fractalNoise"
              baseFrequency="0.85"
              numOctaves="2"
              stitchTiles="stitch"
            />
          </filter>
          <rect width="100%" height="100%" filter="url(#grain)" />
        </svg>
        <div className={styles.verse}>
          <Quote text={verse.text} reference={verse.reference} />
        </div>
      </section>
      <main className={styles.main}>
        {/* Keyed by path: each new page rises into place while the frame stays put. */}
        <div key={pathname} className={`${styles.card} anim-rise`}>
          {children}
        </div>
      </main>
    </div>
  );
}
