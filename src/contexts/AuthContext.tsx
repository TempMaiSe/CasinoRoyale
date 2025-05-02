'use client';

import { createContext, useContext, useEffect, useState, useMemo } from 'react';
import Keycloak from 'keycloak-js';
import { clientEnv } from '@/lib/env';

interface AuthContextType {
  isAuthenticated: boolean;
  isLoading: boolean;
  token: string | null;
  isAdmin: boolean;
  login: () => void;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType>({
  isAuthenticated: false,
  isLoading: true,
  token: null,
  isAdmin: false,
  login: () => {},
  logout: () => {},
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

  useEffect(() => {
    keycloak
      .init({
        onLoad: 'check-sso',
        silentCheckSsoRedirectUri: window.location.origin + '/silent-check-sso.html',
      })
      .then((authenticated) => {
        setIsAuthenticated(authenticated);
        setIsAdmin(keycloak.hasRealmRole('admin'));
        if (authenticated && keycloak.token) {
          setToken(keycloak.token);
          keycloak.onTokenExpired = () => {
            keycloak.updateToken(70).then((refreshed) => {
              if (refreshed && keycloak.token) {
                setToken(keycloak.token);
              }
            });
          };
        }
        setIsLoading(false);
      });
  }, []);

  const login = () => {
    if (!keycloak.authenticated) {
      keycloak
        .init({
          onLoad: 'login-required',
          silentCheckSsoRedirectUri: window.location.origin + '/silent-check-sso.html',
        })
        .then(() => {
          keycloak.login({
            redirectUri: window.location.origin
          });
        });
    } else {
      keycloak.login({
        redirectUri: window.location.origin
      });
    }
  };

  const logout = () => {
    keycloak.logout();
  };

  const value = useMemo(() => ({
    isAuthenticated,
    isLoading,
    token,
    isAdmin,
    login,
    logout,
  }), [isAuthenticated, isLoading, token, isAdmin]);

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => useContext(AuthContext);