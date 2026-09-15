import { apiFetch } from './api';

export interface TokenPair {
  accessToken: string;
  accessTokenExpiresAt: string;
  refreshToken: string;
  refreshTokenExpiresAt: string;
}

export interface RegisterCongregationPayload {
  congregationDto: {
    name: string;
    orgType: 'Church' | 'Mosque' | 'Temple' | 'Other';
    phoneNumber: string;
    emailAddress: string;
    region: string;
    city: string;
    town: string;
    gpsAddress: string;
    postalAddress?: string;
    websiteUrl?: string;
  };
  userDto: {
    firstName: string;
    lastName: string;
    otherNames?: string;
    emailAddress: string;
    password: string;
  };
}

export const authService = {
  login: async (email: string, password: string): Promise<TokenPair> => {
    return apiFetch('/auth/v1/sessions/login', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    });
  },

  refresh: async (refreshToken: string): Promise<TokenPair> => {
    return apiFetch('/auth/v1/sessions/refresh', {
      method: 'POST',
      body: JSON.stringify({ refreshToken }),
    });
  },

  logout: async (refreshToken?: string): Promise<void> => {
    if (!refreshToken) return;
    try {
      await apiFetch('/auth/v1/sessions/revoke', {
        method: 'POST',
        body: JSON.stringify({ refreshToken }),
      });
    } catch {
      // Ignore network/auth errors during logout
    }
  },

  registerCongregation: async (data: RegisterCongregationPayload): Promise<void> => {
    return apiFetch('/auth/v1/register/congregation', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  },
};
