import React, { createContext, useContext, useState, useEffect } from 'react';
import type { ReactNode } from 'react';
// Prepared for actual API integration
// import { apiFetch } from '../services/api';

interface User {
  id: string;
  email: string;
  name?: string;
  token?: string;
}

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    // Check for stored auth state on mount
    const storedUser = localStorage.getItem('user');
    if (storedUser) {
      try {
        setUser(JSON.parse(storedUser));
      } catch (e) {
        localStorage.removeItem('user');
      }
    }
    setIsLoading(false);
  }, []);

  const login = async (email: string, _password: string) => {
    setIsLoading(true);
    try {
      // Prepared for actual API call
      // const response = await apiFetch('/auth/login', {
      //   method: 'POST',
      //   body: JSON.stringify({ email, password }),
      // });
      // setUser(response.user);
      // localStorage.setItem('user', JSON.stringify(response.user));
      
      // Simulating API delay for now
      console.log('Attempting login for:', email);
      await new Promise(resolve => setTimeout(resolve, 1000));
      
      const mockUser: User = { 
        id: '1', 
        email, 
        name: email.split('@')[0],
        token: 'mock-jwt-token'
      };
      
      setUser(mockUser);
      localStorage.setItem('user', JSON.stringify(mockUser));
    } catch (error) {
      console.error('Login failed:', error);
      throw error;
    } finally {
      setIsLoading(false);
    }
  };


  const logout = () => {
    setUser(null);
    localStorage.removeItem('user');
  };

  const value = {
    user,
    isAuthenticated: !!user,
    isLoading,
    login,
    logout
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
