import { createContext } from 'react';

export interface User {
  id: string;
  email: string;
  name?: string;
  token: string;
  refreshToken?: string;
  role?: string;
  congregationId?: string;
}

export interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
}

export const AuthContext = createContext<AuthContextType | undefined>(undefined);
