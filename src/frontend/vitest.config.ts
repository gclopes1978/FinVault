import { defineConfig } from 'vitest/config';
import angular from '@analogjs/vite-plugin-angular';

export default defineConfig({
  plugins: [angular({ 
    tsconfig: 'tsconfig.spec.json',
    include: ['src/**/*.ts'],
    jit: true,
  })],
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['src/test.ts'],
    include: ['src/app/**/*.spec.ts'],
  },
});