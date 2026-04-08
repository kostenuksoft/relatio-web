import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')
  return {
    plugins: [react(), tailwindcss()],
    server: {
      host: '0.0.0.0',
      port: 5173,
      proxy: {
        '/api/auth': {
          target: env['IDENTITY_URL'] ?? 'http://localhost:5212',
          changeOrigin: true,
        },
        '/api/customers': {
          target: env['CUSTOMERS_URL'] ?? 'http://localhost:5211',
          changeOrigin: true,
        },
        '/api/contacts': {
          target: env['CONTACTS_URL'] ?? 'http://localhost:5213',
          changeOrigin: true,
        },
        '/api/deals': {
          target: env['SALES_URL'] ?? 'http://localhost:5215',
          changeOrigin: true,
        },
      },
    },
  }
})
