import { test, expect } from '@playwright/test';

const languages = ['ENGLISH', 'DUTCH', 'INDONESIAN'];
const difficulties = ['BEGINNER', 'INTERMEDIATE'];
const topics = ['BEVERAGES', 'CAR-PARTS'];

function adjacentOverview(summary: string, direction: -1 | 1): string | undefined {
    const [language, difficulty, displayedTopic] = summary.split(' / ');
    const topic = displayedTopic?.replace(' ', '-');
    const currentIndex = languages.indexOf(language) * difficulties.length * topics.length
        + difficulties.indexOf(difficulty) * topics.length
        + topics.indexOf(topic);
    const nextIndex = currentIndex + direction;
    const lastIndex = languages.length * difficulties.length * topics.length - 1;

    if (nextIndex < 0 || nextIndex > lastIndex) return undefined;

    return `${languages[Math.floor(nextIndex / (difficulties.length * topics.length))]} / `
        + `${difficulties[Math.floor((nextIndex % (difficulties.length * topics.length)) / topics.length)]} / `
        + `${topics[nextIndex % topics.length].replace('-', ' ')}`;
}

test.describe('Highscores Panel Network Workflows', () => {

    test('Should successfully navigate to Highscores, fetch rows, and return to Main Menu', async ({ page }) => {
        // 1. Navigate to the local frontend server asset footprint
        await page.goto('http://localhost:8080');
        await page.waitForSelector('#btn-highscores', { state: 'visible' });

        // 2. Click the HIGHSCORES button to trigger the fetchAndRenderHighScores action loop
        await page.click('#btn-highscores');

        // 3. ✅ Use a valid Playwright matcher to wait until the loading screen is hidden
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
        await page.click('#btn-highscores-exit');
        await page.waitForSelector('#btn-start', { state: 'visible' });

        // Confirm the view panels switched and the Main Menu is active again
        const mainMenuPanel = page.locator('#view-main-menu');
        await expect(mainMenuPanel).toHaveClass(/active/);
    });

    test('navigates one highscore overview forward from the current settings', async ({ page }) => {
        await page.goto('http://localhost:8080');
        await page.click('#btn-highscores');
        await expect(page.locator('.table-loading')).toBeHidden();
        await expect(page.locator('#highscores-lbl-summary')).toBeVisible();

        const summary = await page.locator('#highscores-lbl-summary').textContent();
        const expectedNext = adjacentOverview(summary ?? '', 1);
        const nextButton = page.locator('#btn-highscores-next');

        if (expectedNext === undefined) {
            await expect(nextButton).toBeDisabled();
        } else {
            await nextButton.click();
            await expect(page.locator('#highscores-lbl-summary')).toHaveText(expectedNext);
            await expect(page.locator('#btn-highscores-previous')).toBeEnabled();
        }
    });

    test('navigates one highscore overview backward from the current settings', async ({ page }) => {
        await page.addInitScript(() => {
            localStorage.setItem('preferredLanguage', 'dutch');
            localStorage.setItem('preferredDifficulty', 'beginner');
            localStorage.setItem('preferredTopic', 'car-parts');
        });

        await page.goto('http://localhost:8080');
        await page.click('#btn-highscores');
        await expect(page.locator('.table-loading')).toBeHidden();
        await expect(page.locator('#highscores-lbl-summary')).toHaveText('DUTCH / BEGINNER / CAR PARTS');

        const summary = await page.locator('#highscores-lbl-summary').textContent();
        const expectedPrevious = adjacentOverview(summary ?? '', -1);
        const previousButton = page.locator('#btn-highscores-previous');

        if (expectedPrevious === undefined) {
            await expect(previousButton).toBeDisabled();
        } else {
            await previousButton.click();
            await expect(page.locator('#highscores-lbl-summary')).toHaveText(expectedPrevious);
            await expect(page.locator('#btn-highscores-next')).toBeEnabled();
        }
    });
});
