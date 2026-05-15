import React from 'react';

interface SidebarProps {
  activeTab: string;
  onTabChange: (tab: string) => void;
}

const Sidebar: React.FC<SidebarProps> = ({ activeTab, onTabChange }) => {
  const menuItems = [
    { id: 'dashboard', label: 'Dashboard', icon: 'dashboard' },
    { id: 'finance', label: 'Finance', icon: 'finance' },
    { id: 'attendance', label: 'Attendance', icon: 'attendance' },
    { id: 'tithe', label: 'Tithe', icon: 'tithe' },
    { id: 'reporting', label: 'Reporting', icon: 'reporting' },
    { id: 'projects', label: 'Projects', icon: 'projects' },
    { id: 'events', label: 'Events', icon: 'events' },
    { id: 'assets', label: 'Assets', icon: 'assets' },
    { id: 'members', label: 'Members', icon: 'members' },
  ];

  return (
    <aside style={styles.sidebar}>
      <div style={styles.logoContainer}>
        <div style={styles.logoBox}></div>
        <span style={styles.logoText}>Echo</span>
      </div>

      <nav style={styles.nav}>
        {menuItems.map((item) => (
          <button
            key={item.id}
            onClick={() => onTabChange(item.id)}
            style={{
              ...styles.navItem,
              ...(activeTab === item.id ? styles.navItemActive : {}),
            }}
          >
            <div style={{
              ...styles.iconBox,
              ...(activeTab === item.id ? styles.iconBoxActive : {}),
            }}></div>
            <span style={{
              ...styles.navLabel,
              ...(activeTab === item.id ? styles.navLabelActive : {}),
            }}>{item.label}</span>
          </button>
        ))}
      </nav>

      <div style={styles.userProfile}>
        <div style={styles.avatar}></div>
        <div style={styles.userInfo}>
          <span style={styles.userName}>John Doe</span>
        </div>
      </div>
    </aside>
  );
};

const styles: { [key: string]: React.CSSProperties } = {
  sidebar: {
    width: 'var(--sidebar-width)',
    height: '100vh',
    backgroundColor: 'var(--bg-sidebar)',
    borderRight: '1px solid var(--border)',
    display: 'flex',
    flexDirection: 'column',
    position: 'fixed',
    left: 0,
    top: 0,
    zIndex: 100,
  },
  logoContainer: {
    padding: '32px 24px',
    display: 'flex',
    alignItems: 'center',
    gap: '12px',
  },
  logoBox: {
    width: '32px',
    height: '32px',
    backgroundColor: 'black',
    borderRadius: '6px',
  },
  logoText: {
    fontSize: '20px',
    fontWeight: '700',
    color: 'black',
  },
  nav: {
    flex: 1,
    padding: '0 16px',
    display: 'flex',
    flexDirection: 'column',
    gap: '4px',
  },
  navItem: {
    display: 'flex',
    alignItems: 'center',
    gap: '12px',
    padding: '12px 16px',
    borderRadius: '10px',
    border: 'none',
    background: 'transparent',
    textAlign: 'left',
    width: '100%',
    color: 'var(--text-muted)',
    transition: 'all 0.2s ease',
  },
  navItemActive: {
    backgroundColor: '#575757',
    color: 'white',
  },
  iconBox: {
    width: '18px',
    height: '18px',
    backgroundColor: '#d1d1d6',
    borderRadius: '4px',
  },
  iconBoxActive: {
    backgroundColor: 'white',
    opacity: 0.9,
  },
  navLabel: {
    fontSize: '14px',
    fontWeight: '500',
  },
  navLabelActive: {
    color: 'white',
  },
  userProfile: {
    padding: '24px',
    borderTop: '1px solid var(--border)',
    display: 'flex',
    alignItems: 'center',
    gap: '12px',
  },
  avatar: {
    width: '36px',
    height: '36px',
    backgroundColor: '#e5e5ea',
    borderRadius: '50%',
  },
  userInfo: {
    display: 'flex',
    flexDirection: 'column',
  },
  userName: {
    fontSize: '15px',
    fontWeight: '600',
    color: 'black',
  },
};

export default Sidebar;
