import type { ReactNode } from 'react';
import React, { createContext, useContext, useState } from 'react';

export interface TopBarCTA {
    type: 'search' | 'button' | 'dropdown' | 'avatar';
    label?: string;
    placeholder?: string;
    icon?: 'plus' | 'search' | 'export' | 'calendar' | 'download' | 'filter';
    variant?: 'primary' | 'secondary' | 'ghost';
    onClick?: () => void;
}

interface LayoutContextType {
    title: string;
    setTitle: (title: string) => void;
    ctas: TopBarCTA[];
    setCtas: (ctas: TopBarCTA[]) => void;
}

const LayoutContext = createContext<LayoutContextType | undefined>(undefined);

export const LayoutProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
    const [title, setTitle] = useState('');
    const [ctas, setCtas] = useState<TopBarCTA[]>([]);

    return (
        <LayoutContext.Provider value={{ title, setTitle, ctas, setCtas }}>
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
