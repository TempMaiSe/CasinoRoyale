import { createContext, useContext, useEffect, useState } from 'react';
import Keycloak from 'keycloak-js';
import { env } from '@/lib/env';

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
  url: env.keycloak.url,
  realm: env.keycloak.realm,
  clientId: env.keycloak.clientId,
});

export function AuthProvider({ children }: { children: React.ReactNode }) {
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
        if (authenticated) {
          setToken(keycloak.token);
          keycloak.onTokenExpired = () => {
            keycloak.updateToken(70).then((refreshed) => {
              if (refreshed) {
                setToken(keycloak.token);
              }
            });
          };
        }
        setIsLoading(false);
      });
  }, []);

  const login = () => {
    keycloak.login();
  };

  const logout = () => {
    keycloak.logout();
  };

  return (
    <AuthContext.Provider
      value={{
        isAuthenticated,
        isLoading,
        token,
        isAdmin,
        login,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => useContext(AuthContext);