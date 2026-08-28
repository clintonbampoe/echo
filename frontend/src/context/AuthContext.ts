import { createContext } from 'react';

// Interfaces decoupled here to keep the provider and components clean
export interface User {
  id: string;
  email: string;
  name?: string;
  token?: string;
}

export interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
}

export const AuthContext = createContext<AuthContextType | undefined>(undefined);
