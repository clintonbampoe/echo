import type { ReactNode } from 'react';
import React, { createContext, useContext, useState, useEffect } from 'react';

export interface TopBarCTA {
    type: 'search' | 'button' | 'dropdown' | 'avatar';
    label?: string;
    placeholder?: string;
    icon?: 'plus' | 'search' | 'export' | 'calendar' | 'download' | 'filter';
    variant?: 'primary' | 'secondary' | 'ghost';
    onClick?: () => void;
}

interface LayoutContextType {
    title: React.ReactNode;
    setTitle: (title: React.ReactNode) => void;
    ctas: TopBarCTA[];
    setCtas: (ctas: TopBarCTA[]) => void;
    searchQuery: string;
    setSearchQuery: (query: string) => void;
}

const LayoutContext = createContext<LayoutContextType | undefined>(undefined);

export const LayoutProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
    const [title, setTitle] = useState<React.ReactNode>('');
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

export const useLayout = () => {
    const context = useContext(LayoutContext);
    if (context === undefined) {
        throw new Error('useLayout must be used within a LayoutProvider');
    }
    return context;
};
