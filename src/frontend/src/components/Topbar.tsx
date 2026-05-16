import React from 'react';
import { useLayout } from '../context/LayoutContext';
import '../styles/Topbar.css';

const Topbar: React.FC = () => {
  const { title, ctas } = useLayout();
  const renderIcon = (icon?: string) => {
    switch (icon) {
      case 'plus':
        return <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round"><line x1="12" y1="5" x2="12" y2="19"></line><line x1="5" y1="12" x2="19" y2="12"></line></svg>;
      case 'search':
        return <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><circle cx="11" cy="11" r="8"></circle><line x1="21" y1="21" x2="16.65" y2="16.65"></line></svg>;
      case 'export':
      case 'download':
        return <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path><polyline points="7 10 12 15 17 10"></polyline><line x1="12" y1="15" x2="12" y2="3"></line></svg>;
      case 'calendar':
        return <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><rect x="3" y="4" width="18" height="18" rx="2" ry="2"></rect><line x1="16" y1="2" x2="16" y2="6"></line><line x1="8" y1="2" x2="8" y2="6"></line><line x1="3" y1="10" x2="21" y2="10"></line></svg>;
      case 'filter':
        return <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><polygon points="22 3 2 3 10 12.46 10 19 14 21 14 12.46 22 3"></polygon></svg>;
      default:
        return null;
    }
  };

  return (
    <header className="topbar">
      <h2 className="topbar-title">{title}</h2>
      
      <div className="right-section">
        {ctas.map((cta, index) => {
          if (cta.type === 'search') {
            return (
              <div key={index} className="search-container">
                <div className="search-icon">{renderIcon('search')}</div>
                <input type="text" placeholder={cta.placeholder || "Search..."} className="search-input" />
              </div>
            );
          }
          
          if (cta.type === 'avatar') {
            return <div key={index} className="user-circle"></div>;
          }

          if (cta.type === 'button' || cta.type === 'dropdown') {
            const isPrimary = cta.variant === 'primary';
            const isDropdown = cta.type === 'dropdown';
            
            return (
              <button 
                key={index} 
                onClick={cta.onClick}
                className={`button-base ${isPrimary ? 'button-primary' : 'button-secondary'}`}
              >
                {renderIcon(cta.icon)}
                {cta.label}
                {isDropdown && (
                  <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round" style={{ marginLeft: '4px' }}>
                    <path d="M6 9l6 6 6-6"></path>
                  </svg>
                )}
              </button>
            );
          }
          
          return null;
        })}
      </div>
    </header>
  );
};

export default Topbar;
