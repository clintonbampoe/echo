import type { ReactNode } from 'react';
import React, { useEffect, useMemo, useState } from 'react';
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

  const value = useMemo(
    () => ({ title, setTitle, ctas, setCtas, searchQuery, setSearchQuery }),
    [title, ctas, searchQuery]
  );

  return (
    <LayoutContext.Provider value={value}>
      {children}
    </LayoutContext.Provider>
  );
};
