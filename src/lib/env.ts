// This is a server-side module by default (no 'use client' directive)
const serverEnv = {
  apiUrl: process.env['services__api__https__0'] ?? process.env.API_URL ?? 'http://localhost:5000',
  keycloak: {
    url: process.env.KEYCLOAK_URL ?? 'http://localhost:8080',
    realm: process.env.KEYCLOAK_REALM ?? 'casino-royale',
    clientId: process.env.KEYCLOAK_CLIENT_ID ?? 'casino-royale-web'
  }
} as const;

// Only expose what the client needs - these will be baked into the JS bundle at build time
export const clientEnv = {
  apiUrl: serverEnv.apiUrl,
  keycloak: {
    url: serverEnv.keycloak.url,
    realm: serverEnv.keycloak.realm,
    clientId: serverEnv.keycloak.clientId
  }
} as const;

// Server-side only exports
export const getServerEnv = () => serverEnv;