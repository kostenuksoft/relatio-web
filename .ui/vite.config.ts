import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'
import path from 'path'
import { fileURLToPath } from 'url'

const __dirname = path.dirname(fileURLToPath(import.meta.url))

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')
  const get = (key: string, fallback: string) =>
    process.env[key] ?? env[key] ?? fallback

  return {
    plugins: [react(), tailwindcss()],
    resolve: {
      alias: { '@': path.resolve(__dirname, './src') },
    },
    server: {
      host: '0.0.0.0',
      port: 5173,
      proxy: {
        '/api/auth': {
          target: get('IDENTITY_URL', 'http://localhost:5212'),
          changeOrigin: true,
          configure: (proxy) => {
            proxy.on('error', (err, req) => console.error('[proxy] auth error', req.url, err.message))
            proxy.on('proxyRes', (res, req) => console.log('[proxy] auth', res.statusCode, req.url))
          },
        },
        '/api/customers': {
          target: get('CUSTOMERS_URL', 'http://localhost:5211'),
          changeOrigin: true,
          configure: (proxy) => {
            proxy.on('error', (err, req) => console.error('[proxy] customers error', req.url, err.message))
            proxy.on('proxyRes', (res, req) => console.log('[proxy] customers', res.statusCode, req.url))
          },
        },
        '/api/contacts': {
          target: get('CONTACTS_URL', 'http://localhost:5213'),
          changeOrigin: true,
          configure: (proxy) => {
            proxy.on('error', (err, req) => console.error('[proxy] contacts error', req.url, err.message))
            proxy.on('proxyRes', (res, req) => console.log('[proxy] contacts', res.statusCode, req.url))
          },
        },
        '/api/deals': {
          target: get('SALES_URL', 'http://localhost:5215'),
          changeOrigin: true,
          configure: (proxy) => {
            proxy.on('error', (err, req) => console.error('[proxy] deals error', req.url, err.message))
            proxy.on('proxyRes', (res, req) => console.log('[proxy] deals', res.statusCode, req.url))
          },
        },
      },
    },
  }
})
