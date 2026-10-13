import { defineConfig, devices } from '@playwright/test';

const isCI = !!(globalThis as typeof globalThis & {
    process?: { env?: { CI?: string } };
}).process?.env?.CI;

export default defineConfig({
    testDir: './tests',
    // 🔍 FIXED: Explicitly allow both lowercase and uppercase folder matches cleanly
    testMatch: ['**/*.spec.ts', '**/*.test.ts'],
    fullyParallel: false,
    forbidOnly: isCI,
    retries: isCI ? 2 : 0,
    workers: 1,
    reporter: 'line',
    use: {
        baseURL: 'http://localhost:8080',
        trace: 'on-first-retry',
    },
    projects: [
        {
            name: 'chromium',
            use: { ...devices['Desktop Chrome'] },
        },
    ],
});
