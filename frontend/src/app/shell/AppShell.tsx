import { useState } from "react";
import { Outlet } from "react-router-dom";
import { Drawer } from "@/ui";
import { SidebarBody } from "./SidebarBody";
import styles from "./AppShell.module.css";
import { TopBar } from "./Topbar";

// The frame around every signed-in page: a sidebar on wide screens, and on phones a top bar
// with a slide-in menu that holds the same sidebar. The page itself is drawn where <Outlet /> is.
export function AppShell() {
  const [menuOpen, setMenuOpen] = useState(false);
  const close = () => setMenuOpen(false);
  return (
    <div className={styles.app}>
      <aside className={styles.side} aria-label="Sidebar">
        <SidebarBody />
      </aside>
      <div className={styles.panel}>
        <TopBar onMenu={() => setMenuOpen(true)} />
        <main className={styles.main}>
          <Outlet />
        </main>
      </div>
      <Drawer open={menuOpen} onClose={close} label="Menu">
        <div className={styles.drawerBody}>
          <SidebarBody onNavigate={close} />
        </div>
      </Drawer>
    </div>
  );
}
