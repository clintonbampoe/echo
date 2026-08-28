import type { ReactNode } from 'react';
import React, { useEffect, useState } from 'react';
import type { TopBarCTA } from './LayoutContext';
import { LayoutContext } from './LayoutContext';

export const LayoutProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [title, setTitle] = useState<ReactNode>('');
  const [ctas, setCtas] = useState<TopBarCTA[]>([]);
  const [searchQuery, setSearchQuery] = useState('');

  useEffect(() => {
    if (typeof title === 'string') {
      document.title = title ? `${title} | Echo` : 'Echo';
    } else {
      document.title = 'Echo';
    }
  }, [title]);


  return (
    <LayoutContext.Provider value={{ title, setTitle, ctas, setCtas, searchQuery, setSearchQuery }}>
      {children}
    </LayoutContext.Provider>
  );
};
