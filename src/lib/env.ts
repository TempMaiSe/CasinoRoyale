export const env = {
  apiUrl: process.env['services__api__https__0'] ?? process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5000',
  keycloak: {
    url: process.env.NEXT_PUBLIC_KEYCLOAK_URL ?? 'http://localhost:8080',
    realm: process.env.NEXT_PUBLIC_KEYCLOAK_REALM ?? 'casino-royale',
    clientId: process.env.NEXT_PUBLIC_KEYCLOAK_CLIENT_ID ?? 'casino-royale-web'
  }
};