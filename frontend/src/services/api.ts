const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5025/api';

let isRefreshing = false;
let failedQueue: Array<{
  resolve: (value?: unknown) => void;
  reject: (reason?: unknown) => void;
}> = [];

const processQueue = (error: unknown) => {
  failedQueue.forEach(prom => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve();
    }
  });
  failedQueue = [];
};

export const apiFetch = async (endpoint: string, options: RequestInit = {}): Promise<any> => {
  const userStr = localStorage.getItem('user');
  let token = '';
  let refreshToken = '';

  if (userStr) {
    try {
      const user = JSON.parse(userStr);
      token = user?.token || '';
      refreshToken = user?.refreshToken || '';
    } catch {
      // ignore
    }
  }

  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
    ...((options.headers as Record<string, string>) || {}),
  };

  const response = await fetch(`${API_BASE_URL}${endpoint}`, {
    ...options,
    headers,
  });

  if (response.status === 401) {
    // If we have a refresh token and this is not already an auth endpoint request, try refreshing
    if (refreshToken && !endpoint.includes('/auth/v1/sessions/')) {
      if (!isRefreshing) {
        isRefreshing = true;

        try {
          const refreshRes = await fetch(`${API_BASE_URL}/auth/v1/sessions/refresh`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ refreshToken }),
          });

          if (refreshRes.ok) {
            let data = await refreshRes.json();
            if (data && typeof data === 'object' && 'data' in data) {
              data = data.data;
            }

            const currentUser = userStr ? JSON.parse(userStr) : {};
            const updatedUser = {
              ...currentUser,
              token: data.accessToken,
              refreshToken: data.refreshToken,
            };
            localStorage.setItem('user', JSON.stringify(updatedUser));
            isRefreshing = false;
            processQueue(null);

            // Retry original request with new token
            return apiFetch(endpoint, {
              ...options,
              headers: {
                ...options.headers,
                Authorization: `Bearer ${data.accessToken}`,
              },
            });
          }
        } catch {
          // Refresh failed
        }

        isRefreshing = false;
        processQueue(new Error('Session expired'));
      } else {
        // Wait for refresh to finish then retry
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        }).then(() => apiFetch(endpoint, options));
      }
    }

    localStorage.removeItem('user');
    window.dispatchEvent(new Event('auth:unauthorized'));
    throw new Error('Unauthorized');
  }

  if (!response.ok) {
    const errorData = await response.json().catch(() => ({}));
    let errorMessage = errorData.detail || errorData.message || errorData.title;

    if (!errorMessage && errorData.errors) {
      errorMessage = Object.values(errorData.errors).flat().join(' ');
    }

    throw new Error(errorMessage || 'Something went wrong');
  }

  const contentType = response.headers.get('content-type');
  if (contentType && contentType.includes('application/json')) {
    const json = await response.json();
    if (json && typeof json === 'object') {
      if ('data' in json) return json.data;
      if ('resource' in json) return json.resource;
    }
    return json;
  }

  const text = await response.text();
  return text ? { message: text } : null;
};
