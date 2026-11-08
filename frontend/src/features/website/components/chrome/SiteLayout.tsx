import { useEffect, type ReactNode } from "react";
import { Outlet, ScrollRestoration } from "react-router-dom";
import { Atmosphere, usePublicSurface } from "@/ui";
import { BackToTop } from "./BackToTop";
import { SiteFooter } from "./SiteFooter";
import { SiteHeader } from "./SiteHeader";
import styles from "./SiteLayout.module.css";

// The frame around a website page: the light, the header and the footer. Pages sit inside it as children;
// the error screens use it too, so a wrong turn still looks like the site.
export function SiteFrame({
  children,
  flush = false,
}: {
  children: ReactNode;
  // The page fills the window and its footer sits right under it.
  flush?: boolean;
}) {
  usePublicSurface();
  // Sections settle into place as the page is scrolled (see SiteLayout.module.css). Only the website does this.
  useEffect(() => {
    const root = document.documentElement;
    root.dataset.snap = "sections";
    return () => {
      delete root.dataset.snap;
    };
  }, []);
  return (
    <div className={styles.site}>
      <Atmosphere />
      <SiteHeader />
      <main className={styles.main}>{children}</main>
      <SiteFooter flush={flush} />
      <BackToTop />
      <ScrollRestoration />
    </div>
  );
}

// The frame around every website page. It stays mounted while someone moves between pages,
// so the light in the background does not restart.
export function SiteLayout() {
  return (
    <SiteFrame>
      <Outlet />
    </SiteFrame>
  );
}
