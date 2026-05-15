import { useState, useEffect } from 'react'
import Login from './components/Login'
import Sidebar from './components/Sidebar'
import Topbar from './components/Topbar'
import Dashboard from './components/Dashboard'
import { LayoutProvider, useLayout } from './context/LayoutContext'

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
    <div style={{ padding: '40px', textAlign: 'center', color: 'var(--text-muted)' }}>
      <h3>{name.charAt(0).toUpperCase() + name.slice(1)} content coming soon...</h3>
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
      <div style={styles.appLayout}>
        <Sidebar activeTab={activeTab} onTabChange={setActiveTab} />
        <div style={styles.mainContent}>
          <Topbar />
          <div style={styles.pageContent}>
            {activeTab === 'dashboard' && <Dashboard />}
            {activeTab !== 'dashboard' && <PlaceholderTab name={activeTab} />}
          </div>
        </div>
      </div>
    </LayoutProvider>
  )
}

const styles: { [key: string]: React.CSSProperties } = {
  appLayout: {
    display: 'flex',
    minHeight: '100vh',
    width: '100%',
  },
  mainContent: {
    flex: 1,
    marginLeft: 'var(--sidebar-width)',
    display: 'flex',
    flexDirection: 'column',
    backgroundColor: 'var(--bg-main)',
  },
  pageContent: {
    flex: 1,
  },
};

export default App

