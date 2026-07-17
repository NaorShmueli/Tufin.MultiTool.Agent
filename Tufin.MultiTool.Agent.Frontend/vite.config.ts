import tailwindcss from '@tailwindcss/vite';
import react from '@vitejs/plugin-react';
import { defineConfig } from 'vite';

export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    port: 5173,
    proxy: {
      '/task': 'http://localhost:8080',
      '/tasks': 'http://localhost:8080',
      '/health': 'http://localhost:8080'
    }
  }
});
