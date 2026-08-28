import { createContext } from 'react';
import type { ReactNode } from 'react';

export interface TopBarCTA {
  type: 'search' | 'button' | 'dropdown' | 'avatar';
  label?: string;
  placeholder?: string;
  icon?: 'plus' | 'search' | 'export' | 'calendar' | 'download' | 'filter';
  variant?: 'primary' | 'secondary' | 'ghost';
  onClick?: () => void;
}

export interface LayoutContextType {
  title: ReactNode;
  setTitle: (title: ReactNode) => void;
  ctas: TopBarCTA[];
  setCtas: (ctas: TopBarCTA[]) => void;
  searchQuery: string;
  setSearchQuery: (query: string) => void;
}

export const LayoutContext = createContext<LayoutContextType | undefined>(undefined);
