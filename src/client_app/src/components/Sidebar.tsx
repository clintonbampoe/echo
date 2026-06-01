import React from 'react';
import echoLogo from '../assets/echo.svg';
import {
    DashboardIcon,
    FinanceIcon,
    AttendanceIcon,
    TitheIcon,
    ReportingIcon,
    ProjectsIcon,
    CalendarIcon,
    BoxIcon,
    MembersIcon
} from './Icons';
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

    const renderSidebarIcon = (iconName: string) => {
        const className = 'sidebar-icon';
        switch (iconName) {
            case 'dashboard': return <DashboardIcon className={className} />;
            case 'finance': return <FinanceIcon className={className} />;
            case 'attendance': return <AttendanceIcon className={className} />;
            case 'tithe': return <TitheIcon className={className} />;
            case 'reporting': return <ReportingIcon className={className} />;
            case 'projects': return <ProjectsIcon className={className} />;
            case 'calendar': return <CalendarIcon className={className} />;
            case 'box': return <BoxIcon className={className} />;
            case 'members': return <MembersIcon className={className} />;
            default: return null;
        }
    };

    return (
        <aside className="sidebar">
            <div className="sidebar-logo">
                <img src={echoLogo} className="logo-img" alt="Echo Logo" />
                <span className="logo-text">Echo</span>
            </div>

            <nav className="sidebar-nav">
                {menuItems.map((item) => (
                    <button
                        key={item.id}
                        onClick={() => onTabChange(item.id)}
                        className={`nav-item ${activeTab === item.id ? 'nav-item-active' : ''}`}
                    >
                        {renderSidebarIcon(item.icon)}
                        <span className={`nav-label ${activeTab === item.id ? 'nav-label-active' : ''}`}>{item.label}</span>
                    </button>
                ))}
            </nav>

            <div className="sidebar-footer">
                <div className="footer-avatar">JD</div>
                <div className="footer-user-info">
                    <span className="footer-user-name">John Doe</span>
                </div>
            </div>
        </aside>
    );
};

export default Sidebar;
