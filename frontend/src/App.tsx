import React, { useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import Dashboard from './components/Dashboard';
import Finance from './components/Finance';
import Login from './components/auth/Login';
import SignUp from './components/auth/SignUp';
import ResetPassword from './components/auth/ResetPassword';
import Sidebar from './components/Sidebar';
import Topbar from './components/Topbar';
import Attendance from './components/Attendance';
import Tithe from './components/Tithe';
import Projects from './components/Projects';
import Contributions from './components/Contributions';
import Events from './components/Events';
import Members from './components/Members';
import { LayoutProvider, useLayout } from './context/LayoutContext';
import { AuthProvider, useAuth } from './context/AuthContext';
import './styles/App.css';

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

const ProtectedLayout: React.FC = () => {
    const { isAuthenticated } = useAuth();
    if (!isAuthenticated) return <Navigate to="/login" replace />;

    return (
        <LayoutProvider>
            <div className="app-layout">
                <Sidebar />
                <div className="main-content">
                    <Topbar />
                    <div className="page-content">
                        <Routes>
                            <Route path="/" element={<Navigate to="/dashboard" replace />} />
                            <Route path="/dashboard" element={<Dashboard />} />
                            <Route path="/finance" element={<Finance />} />
                            <Route path="/attendance" element={<Attendance />} />
                            <Route path="/tithe" element={<Tithe />} />
                            <Route path="/projects" element={<Projects />} />
                            <Route path="/contributions" element={<Contributions />} />
                            <Route path="/events" element={<Events />} />
                            <Route path="/members" element={<Members />} />
                            <Route path="/reporting" element={<PlaceholderTab name="Reporting" />} />
                            <Route path="/assets" element={<PlaceholderTab name="Assets" />} />
                            <Route path="*" element={<PlaceholderTab name="Not Found" />} />
                        </Routes>
                    </div>
                </div>
            </div>
        </LayoutProvider>
    );
};

const AppContent: React.FC = () => {
    const { isLoading: isAuthLoading } = useAuth();

    if (isAuthLoading) {
        return (
            <div className="loading-screen">
                <p>Loading...</p>
            </div>
        );
    }

    return (
        <Routes>
            <Route path="/login" element={<main><Login /></main>} />
            <Route path="/signup" element={<main><SignUp /></main>} />
            <Route path="/reset-password" element={<main><ResetPassword /></main>} />
            <Route path="/*" element={<ProtectedLayout />} />
        </Routes>
    );
};

function App() {
    return (
        <Router>
            <AuthProvider>
                <AppContent />
            </AuthProvider>
        </Router>
    );
}

export default App;
