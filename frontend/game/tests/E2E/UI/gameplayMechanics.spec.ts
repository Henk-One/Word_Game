import { expect, test, type Page } from '@playwright/test';

test.describe('Gameplay mechanics', () => {
    async function mockGameResponse(
        page: Page,
        endpoint: 'start' | 'guess' | 'hint',
        payload: Record<string, unknown>
    ): Promise<void> {
        await page.route(`**/api/game/${endpoint}**`, async route => {
            await route.fulfill({
                status: 200,
                contentType: 'application/json',
                body: JSON.stringify(payload)
            });
        });
    }


    test.beforeEach(async ({ page }) => {
        // Set up standard starting baseline mock for game initiation
        await mockGameResponse(page, 'start', {
            gameId: "E2E-TEST-GUID-12345",
            revealed: "---",
            fullWord: "CAR",
            lives: 11,
            score: 0,
            level: 1,
            hints: 0,
            hintCapacity: 3,
            repeatGuessPenalty: false,
            livesPerLevel: 11
        });

        await page.goto('http://localhost:8080');
        await page.waitForSelector('#btn-start', { state: 'visible' });
        await page.click('#btn-start');
        await page.waitForSelector('#view-game-scene', { state: 'visible' });
    });

    test('resets game state and keyboard buttons on restart', async ({ page }) => {
        await page.click('#key-c');
        await page.click('#key-z');

        await page.click('#btn-game-restart');

        await expect(page.locator('#game-score')).toHaveText('0');
        await expect(page.locator('#game-level')).toHaveText('1');
        await expect(page.locator('#game-lives-container')).toHaveText('❤️❤️❤️❤️❤️❤️❤️❤️❤️❤️❤️');

        await expect(page.locator('#key-c')).toBeEnabled();
        await expect(page.locator('#key-z')).toBeEnabled();
    });

    test('shows a hint awarded for completing a word', async ({ page }) => {
        await mockGameResponse(page, 'guess', {
            gameId: "E2E-TEST-GUID-12345",
            revealed: "CAR",
            fullWord: "CAR",
            lives: 11,
            score: 1,
            gameOver: false,
            level: 1,
            hints: 1,
            hintCapacity: 5,
            roundComplete: true,
            repeatGuessPenalty: false,
            livesPerLevel: 11
        });

        await page.click('#key-r');

        await expect(page.locator('#game-score')).toHaveText('1');
        await expect(page.locator('#game-hints-container')).toContainText('💡');
    });

    test('reveals the answer when the game ends', async ({ page }) => {
        await mockGameResponse(page, 'guess', {
            gameId: "E2E-TEST-GUID-12345",
            revealed: "--R",
            fullWord: "CAR",
            lives: 0,
            score: 0,
            gameOver: true,
            level: 1,
            hints: 0,
            hintCapacity: 3,
            roundComplete: false,
            repeatGuessPenalty: false,
            livesPerLevel: 11
        });

        await page.click('#key-x');

        const blanks = page.locator('#game-word-blanks .word-letter-blank');
        await expect(blanks.nth(0)).toHaveText('C');
        await expect(blanks.nth(1)).toHaveText('A');
        await expect(blanks.nth(2)).toHaveText('R');

        await expect(page.locator('#game-announcement-banner')).toBeVisible();
        await expect(page.locator('#game-announcement-banner')).toContainText('CAR');
    });

    test('updates lives, level, score, and hints from game responses', async ({ page }) => {
        await mockGameResponse(page, 'guess', {
            gameId: "E2E-TEST-GUID-12345",
            revealed: "---",
            fullWord: "CAR",
            lives: 10,
            score: 0,
            gameOver: false,
            level: 1,
            hints: 0,
            hintCapacity: 3,
            roundComplete: false,
            repeatGuessPenalty: false,
            livesPerLevel: 11
        });
        await page.click('#key-x');
        await expect(page.locator('#game-lives-container')).toHaveText('❤️❤️❤️❤️❤️❤️❤️❤️❤️❤️🖤');

        // Restart returns the next session's level from the server.
        await mockGameResponse(page, 'start', {
            gameId: "E2E-TEST-GUID-12345",
            revealed: "----",
            fullWord: "GEAR",
            lives: 11,
            score: 3,
            level: 2,
            hints: 2,
            hintCapacity: 3,
            repeatGuessPenalty: true,
            livesPerLevel: 11
        });

        await page.click('#btn-game-restart');
        await expect(page.locator('#game-level')).toHaveText('2');

        await mockGameResponse(page, 'hint', {
            gameId: "E2E-TEST-GUID-12345",
            revealed: "G---",
            fullWord: "GEAR",
            lives: 11,
            score: 3,
            level: 2,
            hints: 1,
            hintCapacity: 3,
            roundComplete: false
        });
        await page.click('#btn-game-hint');
        await expect(page.locator('#game-hints-container')).toContainText('💡');

        await mockGameResponse(page, 'hint', {
            gameId: "E2E-TEST-GUID-12345",
            revealed: "GEAR",
            fullWord: "GEAR",
            lives: 11,
            score: 4,
            level: 2,
            hints: 0,
            hintCapacity: 3,
            roundComplete: true
        });
        await page.click('#btn-game-hint');
        await expect(page.locator('#game-score')).toHaveText('4');
    });
});
