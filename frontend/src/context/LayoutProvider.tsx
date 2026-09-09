import React, { useState } from 'react';
import type { ReactNode } from 'react';
import { LayoutContext } from './LayoutContext';
import type { TopBarCTA } from './LayoutContext';

export const LayoutProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [title, setTitle] = useState<ReactNode>('');
  const [ctas, setCtas] = useState<TopBarCTA[]>([]);
  const [searchQuery, setSearchQuery] = useState('');

  return (
    <LayoutContext.Provider value={{ title, setTitle, ctas, setCtas, searchQuery, setSearchQuery }}>
      {children}
    </LayoutContext.Provider>
  );
};
