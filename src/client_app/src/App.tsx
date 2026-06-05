import React, { useEffect, useState } from 'react'
import Dashboard from './components/Dashboard'
import Finance from './components/Finance'
import Login from './components/Login'
import Sidebar from './components/Sidebar'
import Topbar from './components/Topbar'
import Attendance from './components/Attendance'
import Tithe from './components/Tithe'
import { LayoutProvider, useLayout } from './context/LayoutContext'
import { AuthProvider, useAuth } from './context/AuthContext'
import './styles/App.css'

const PlaceholderTab: React.FC<{ name: string }> = ({ name }) => {
    const { setTitle, setCtas } = useLayout();

    useEffect(() => {
        setTitle(name.charAt(0).toUpperCase() + name.slice(1));
        setCtas([
            { type: 'search', placeholder: `Search ${name}...` },
            { type: 'avatar' }
        ]);
    }, [name, setTitle, setCtas]);

    return (
        <div className="placeholder-container">
            <h3 className="placeholder-title">{name} content coming soon...</h3>
        </div>
    );
};

const AppContent: React.FC = () => {
    const { isAuthenticated, isLoading: isAuthLoading } = useAuth();
    const [activeTab, setActiveTab] = useState('dashboard');

    if (isAuthLoading) {
        return (
            <div className="loading-screen">
                <p>Loading...</p>
            </div>
        );
    }

    if (!isAuthenticated) {
        return (
            <main>
                <Login />
            </main>
        );
    }

    return (
        <LayoutProvider>
            <div className="app-layout">
                <Sidebar activeTab={activeTab} onTabChange={setActiveTab} />
                <div className="main-content">
                    <Topbar />
                    <div className="page-content">
                        {activeTab === 'dashboard' && <Dashboard />}
                        {activeTab === 'finance' && <Finance />}
                        {activeTab === 'attendance' && <Attendance />}
                        {activeTab === 'tithe' && <Tithe />}
                        {activeTab !== 'dashboard' && activeTab !== 'finance' && activeTab !== 'attendance' && activeTab !== 'tithe' && <PlaceholderTab name={activeTab} />}
                    </div>
                </div>
            </div>
        </LayoutProvider>
    );
};

function App() {
    return (
        <AuthProvider>
            <AppContent />
        </AuthProvider>
    );
}

export default App;



