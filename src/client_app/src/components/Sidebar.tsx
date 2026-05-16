import React from 'react';
import '../styles/Sidebar.css';

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
        { id: 'events', label: 'Events', icon: 'calendar' },
        { id: 'assets', label: 'Assets', icon: 'box' },
        { id: 'members', label: 'Members', icon: 'members' },
    ];

    return (
        <aside className="sidebar">
            <div className="sidebar-logo">
                <div className="logo-box"></div>
                <span className="logo-text">Echo</span>
            </div>

            <nav className="sidebar-nav">
                {menuItems.map((item) => (
                    <button
                        key={item.id}
                        onClick={() => onTabChange(item.id)}
                        className={`nav-item ${activeTab === item.id ? 'nav-item-active' : ''}`}
                    >
                        <div className={`icon-box ${activeTab === item.id ? 'icon-box-active' : ''}`}></div>
                        <span className={`nav-label ${activeTab === item.id ? 'nav-label-active' : ''}`}>{item.label}</span>
                    </button>
                ))}
            </nav>

            <div className="sidebar-footer">
                <div className="footer-avatar"></div>
                <div className="footer-user-info">
                    <span className="footer-user-name">John Doe</span>
                </div>
            </div>
        </aside>
    );
};

export default Sidebar;
