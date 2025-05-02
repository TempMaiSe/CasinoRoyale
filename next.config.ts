import type { NextConfig } from 'next'
import withPWA from 'next-pwa'

const config: NextConfig = {
  env: {
    // Aspire service URLs
    API_URL: process.env.services__api__https__0,
    // Other environment variables
    KEYCLOAK_URL: process.env.KEYCLOAK_URL,
    KEYCLOAK_REALM: process.env.KEYCLOAK_REALM,
    KEYCLOAK_CLIENT_ID: process.env.KEYCLOAK_CLIENT_ID,
  },
}

export default withPWA({
  dest: 'public',
  disable: process.env.NODE_ENV === 'development',
  register: true,
})(config)
