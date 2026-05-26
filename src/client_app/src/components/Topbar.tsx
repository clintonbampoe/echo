import React from 'react';
import { useLayout } from '../context/LayoutContext';
import { PlusIcon, SearchIcon, ExportIcon, CalendarIcon, FilterIcon } from './Icons';
import '../styles/Topbar.css';

const Topbar: React.FC = () => {
  const { title, ctas, searchQuery, setSearchQuery } = useLayout();
  const renderIcon = (icon?: string) => {
    const size = 16;
    switch (icon) {
      case 'plus':
        return <PlusIcon size={size} />;
      case 'search':
        return <SearchIcon size={size} />;
      case 'export':
      case 'download':
        return <ExportIcon size={size} />;
      case 'calendar':
        return <CalendarIcon size={size} />;
      case 'filter':
        return <FilterIcon size={size} />;
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
                <input 
                  type="text" 
                  placeholder={cta.placeholder || "Search..."} 
                  className="search-input" 
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                />
              </div>
            );
          }
          
          if (cta.type === 'avatar') {
            return <div key={index} className="user-circle">JD</div>;
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
