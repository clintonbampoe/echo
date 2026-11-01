import { useState, type ReactNode } from "react";
import { Outlet } from "react-router-dom";
import { MenuPage } from "./MenuPage";
import { NavProgress } from "./NavProgress";
import { PullToRefresh } from "./PullToRefresh";
import { Sidebar } from "./Sidebar";
import { TopBar } from "./TopBar";
import { VerifyEmailBanner } from "./VerifyEmailBanner";
import { useLongPressMenu } from "./useLongPressMenu";
import styles from "./AppShell.module.css";

// The frame around every signed-in page: a sidebar on wide screens; on tablets and phones a top bar whose
// menu button opens the menu page. The page itself is drawn where {children ?? <Outlet />} is.
export function AppShell({ children }: { children?: ReactNode }) {
  const [menuOpen, setMenuOpen] = useState(false);
  useLongPressMenu();
  return (
    <div className={styles.app}>
      <NavProgress />
      <Sidebar />
      <div className={styles.panel}>
        <TopBar onMenu={() => setMenuOpen(true)} />
        <main className={styles.main}>
          <VerifyEmailBanner />
          {children ?? <Outlet />}
        </main>
      </div>
      <PullToRefresh />
      <MenuPage open={menuOpen} onClose={() => setMenuOpen(false)} />
    </div>
  );
}
