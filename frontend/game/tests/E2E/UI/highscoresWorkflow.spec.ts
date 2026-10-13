import { test, expect } from '@playwright/test';

test.describe('Highscores Panel Network Workflows', () => {

    test('Should successfully navigate to Highscores, fetch rows, and return to Main Menu', async ({ page }) => {
        // 1. Navigate to the local frontend server asset footprint
        await page.goto('http://localhost:8080');
        await page.waitForSelector('#btn-highscores', { state: 'visible' });

        // 2. Click the HIGHSCORES button to trigger the fetchAndRenderHighScores action loop
        await page.click('#btn-highscores');

        // 3. ✅ FIXED: Use a valid Playwright matcher to wait until the loading screen is hidden
        const loadingRow = page.locator('.table-loading');
        await expect(loadingRow).toBeHidden({ timeout: 5000 });

        // 4. Assert on the data grid rows
        const dataRows = page.locator('#leaderboard-data-rows tr');
        const errorRow = page.locator('.table-error');

        // Check if a database connection error occurred or if records mapped down cleanly
        if (await errorRow.count() > 0) {
            throw new Error("E2E Validation Failed: Leaderboard table returned a connectivity error state.");
        }

        // Verify that the container table holds valid row nodes
        const rowCount = await dataRows.count();
        expect(rowCount).toBeGreaterThanOrEqual(1);

        // 5. Click the BACK button to return to the main dashboard menu layout boundaries
        await page.click('#btn-highscores-back');
        await page.waitForSelector('#btn-start', { state: 'visible' });

        // Confirm the view panels switched and the Main Menu is active again
        const mainMenuPanel = page.locator('#view-main-menu');
        await expect(mainMenuPanel).toHaveClass(/active/);
    });
});
