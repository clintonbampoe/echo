import { useEffect, useState } from 'react'
import Dashboard from './components/Dashboard'
import Login from './components/Login'
import Sidebar from './components/Sidebar'
import Topbar from './components/Topbar'
import { LayoutProvider, useLayout } from './context/LayoutContext'
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

function App() {
    const [isLoggedIn, setIsLoggedIn] = useState(false)
    const [activeTab, setActiveTab] = useState('dashboard')

    if (!isLoggedIn) {
        return (
            <main>
                <Login onLogin={() => setIsLoggedIn(true)} />
            </main>
        )
    }

    return (
        <LayoutProvider>
            <div className="app-layout">
                <Sidebar activeTab={activeTab} onTabChange={setActiveTab} />
                <div className="main-content">
                    <Topbar />
                    <div className="page-content">
                        {activeTab === 'dashboard' && <Dashboard />}
                        {activeTab !== 'dashboard' && <PlaceholderTab name={activeTab} />}
                    </div>
                </div>
            </div>
        </LayoutProvider>
    )
}

export default App

