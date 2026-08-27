import React from 'react';
import { NavLink } from 'react-router-dom';
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
    MembersIcon,
    ContributionsIcon,
    LogoutIcon
} from './Icons';
import { useAuth } from '../context/AuthContext';
import '../styles/Sidebar.css';

const Sidebar: React.FC = () => {
    const { logout } = useAuth();

    const menuItems = [
        { id: 'dashboard', label: 'Dashboard', icon: 'dashboard' },
        { id: 'finance', label: 'Finance', icon: 'finance' },
        { id: 'attendance', label: 'Attendance', icon: 'attendance' },
        { id: 'tithe', label: 'Tithe', icon: 'tithe' },
        { id: 'reporting', label: 'Reporting', icon: 'reporting' },
        { id: 'projects', label: 'Projects', icon: 'projects' },
        { id: 'contributions', label: 'Contributions', icon: 'contributions' },
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
            case 'contributions': return <ContributionsIcon className={className} />;
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
                    <NavLink
                        key={item.id}
                        to={`/${item.id}`}
                        className={({ isActive }) => `nav-item ${isActive ? 'nav-item-active' : ''}`}
                    >
                        {({ isActive }) => (
                            <>
                                {renderSidebarIcon(item.icon)}
                                <span className={`nav-label ${isActive ? 'nav-label-active' : ''}`}>{item.label}</span>
                            </>
                        )}
                    </NavLink>
                ))}
            </nav>

            <div className="sidebar-footer">
                <div className="footer-avatar">JD</div>
                <div className="footer-user-info" style={{ flex: 1 }}>
                    <span className="footer-user-name">John Doe</span>
                </div>
                <button 
                  onClick={logout} 
                  className="logout-btn" 
                  title="Log out"
                >
                    <LogoutIcon size={18} />
                </button>
            </div>
        </aside>
    );
};

export default Sidebar;
