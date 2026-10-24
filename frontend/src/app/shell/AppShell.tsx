import { useState } from "react";
import { Outlet } from "react-router-dom";
import { MenuPage } from "./MenuPage";
import { Sidebar } from "./Sidebar";
import { TopBar } from "./TopBar";
import styles from "./AppShell.module.css";

// The frame around every signed-in page: a sidebar on wide screens; on tablets and phones a top bar whose
// menu button opens the menu page. The page itself is drawn where <Outlet /> is.
export function AppShell() {
  const [menuOpen, setMenuOpen] = useState(false);
  return (
    <div className={styles.app}>
      <Sidebar />
      <div className={styles.panel}>
        <TopBar onMenu={() => setMenuOpen(true)} />
        <main className={styles.main}>
          <Outlet />
        </main>
      </div>
      <MenuPage open={menuOpen} onClose={() => setMenuOpen(false)} />
    </div>
  );
}
