'use client';

import { createContext, useContext, useEffect, useState, useMemo } from 'react';
import Keycloak from 'keycloak-js';
import { clientEnv } from '@/lib/env';

interface AuthContextType {
  isAuthenticated: boolean;
  isLoading: boolean;
  token: string | null;
  isAdmin: boolean;
  error: Error | null;
  login: () => Promise<void>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType>({
  isAuthenticated: false,
  isLoading: true,
  token: null,
  isAdmin: false,
  error: null,
  login: async () => {},
  logout: async () => {},
});

const keycloak = new Keycloak({
  url: clientEnv.keycloak.url,
  realm: clientEnv.keycloak.realm,
  clientId: clientEnv.keycloak.clientId,
});

type AuthProviderProps = Readonly<{
  children: React.ReactNode;
}>;

export function AuthProvider({ children }: AuthProviderProps) {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  const [token, setToken] = useState<string | null>(null);
  const [isAdmin, setIsAdmin] = useState(false);
  const [error, setError] = useState<Error | null>(null);

  const updateToken = async (minValidity: number = 70) => {
    try {
      const refreshed = await keycloak.updateToken(minValidity);
      if (refreshed && keycloak.token) {
        setToken(keycloak.token);
      }
    } catch (err) {
      setError(err instanceof Error ? err : new Error('Failed to refresh token'));
      // Token refresh failed, user needs to login again
      setIsAuthenticated(false);
      setToken(null);
    }
  };

  useEffect(() => {
    const initKeycloak = async () => {
      try {
        const authenticated = await keycloak.init({
          onLoad: 'check-sso',
          silentCheckSsoRedirectUri: `${window.location.origin}/silent-check-sso.html`,
          pkceMethod: 'S256', // Modern PKCE for enhanced security
          checkLoginIframe: false, // Disable problematic iframe checks
        });

        setIsAuthenticated(authenticated);

        if (authenticated && keycloak.token) {
          setToken(keycloak.token);
          setIsAdmin(keycloak.hasRealmRole('admin'));
          
          // Set up token refresh
          keycloak.onTokenExpired = () => {
            updateToken();
          };

          // Schedule periodic token refresh
          const refreshInterval = setInterval(() => {
            updateToken();
          }, 60000); // Check token every minute

          return () => {
            clearInterval(refreshInterval);
          };
        }
      } catch (err) {
        setError(err instanceof Error ? err : new Error('Failed to initialize Keycloak'));
      } finally {
        setIsLoading(false);
      }
    };

    initKeycloak();
  }, []);

  const login = async () => {
    try {
      setError(null);
      if (!keycloak.authenticated) {
        await keycloak.init({
          onLoad: 'login-required',
          silentCheckSsoRedirectUri: `${window.location.origin}/silent-check-sso.html`,
          pkceMethod: 'S256',
          checkLoginIframe: false,
        });
      }
      await keycloak.login({
        redirectUri: window.location.origin,
        scope: 'openid profile email',
      });
    } catch (err) {
      setError(err instanceof Error ? err : new Error('Login failed'));
    }
  };

  const logout = async () => {
    try {
      setError(null);
      await keycloak.logout({
        redirectUri: window.location.origin
      });
      setIsAuthenticated(false);
      setToken(null);
      setIsAdmin(false);
    } catch (err) {
      setError(err instanceof Error ? err : new Error('Logout failed'));
    }
  };

  const value = useMemo(() => ({
    isAuthenticated,
    isLoading,
    token,
    isAdmin,
    error,
    login,
    logout,
  }), [isAuthenticated, isLoading, token, isAdmin, error]);

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => useContext(AuthContext);