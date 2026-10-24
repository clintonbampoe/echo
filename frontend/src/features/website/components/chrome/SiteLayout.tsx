import { Outlet, ScrollRestoration } from "react-router-dom";
import { Atmosphere, usePublicSurface } from "@/ui";
import { SiteFooter } from "./SiteFooter";
import { SiteHeader } from "./SiteHeader";
import styles from "./SiteLayout.module.css";

// The frame around every website page. It stays mounted while someone moves between pages,
// so the light in the background does not restart.
export function SiteLayout() {
  usePublicSurface();
  return (
    <div className={styles.site}>
      <Atmosphere />
      <SiteHeader />
      <main className={styles.main}>
        <Outlet />
      </main>
      <SiteFooter />
      <ScrollRestoration />
    </div>
  );
}
