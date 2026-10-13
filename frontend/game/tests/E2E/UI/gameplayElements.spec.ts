import { test, expect } from '@playwright/test';

test.describe('Gameplay Arena Core Elements Verification', () => {

    test.beforeEach(async ({ page }) => {
        await page.route('**/start', async route => {
            await route.fulfill({
                status: 200,
                contentType: 'application/json',
                body: JSON.stringify({
                    gameId: 'E2E-GAMEPLAY-ELEMENTS',
                    wordLength: 3,
                    revealed: '---',
                    fullWord: 'CAR',
                    lives: 11,
                    score: 0,
                    level: 1,
                    hints: 0,
                    hintCapacity: 3,
                    bonusHintProgress: 0,
                    solvedWords: 0,
                    roundNumber: 1,
                    repeatGuessPenalty: false,
                    livesPerLevel: 11
                })
            });
        });

        await page.goto('http://localhost:8080');
        await page.waitForSelector('#btn-start', { state: 'visible' });

        await page.click('#btn-start');
        await page.waitForSelector('#view-game-scene', { state: 'visible' });
    });

    test('Should verify all textual and structural game layout components are present', async ({ page }) => {
        // Check main screen heading title
        const gameplayHeading = page.locator('#view-game-scene h1');
        await expect(gameplayHeading).toBeVisible();

        // 📝 ✅ FIXED: Use type-safe toContainText matchers to verify category preference combinations
        const metaSummary = page.locator('#game-lbl-summary');
        await expect(metaSummary).toBeVisible();
        await expect(metaSummary).toContainText('ENGLISH', { ignoreCase: true });
        await expect(metaSummary).toContainText('BEGINNER', { ignoreCase: true });
        await expect(metaSummary).toContainText('CAR PARTS', { ignoreCase: true });

        // Verify Head-Up Display (HUD) Label Metrics are visible
        await expect(page.locator('#game-score')).toBeVisible();
        await expect(page.locator('#game-level')).toBeVisible();
        await expect(page.locator('#game-hints-container')).toBeVisible();
        await expect(page.locator('#game-lives-container')).toBeVisible();

        // Verify Hidden Word Placeholder Blanks are present
        const blanks = page.locator('#game-word-blanks .word-letter-blank');
        const totalBlanksCount = await blanks.count();
        expect(totalBlanksCount).toBeGreaterThan(0);

        // Hidden letters are rendered as empty spans with visible underline placeholders.
        const firstBlankText = await blanks.first().textContent();
        expect(firstBlankText).toBe('');

        // Verify the Full Alphabet Keyboard Buttons are present (A through Z)
        const alphabet = "abcdefghijklmnopqrstuvwxyz".split("");
        for (const letter of alphabet) {
            const keyButton = page.locator(`#key-${letter}`);
            await expect(keyButton).toBeVisible();
            await expect(keyButton).toHaveText(letter.toUpperCase());
        }

        // Verify Control Buttons are present and labeled accurately
        const restartButton = page.locator('#btn-game-restart');
        const stopButton = page.locator('#btn-game-stop');

        await expect(restartButton).toBeVisible();
        await expect(restartButton).toHaveText('RESTART GAME');

        await expect(stopButton).toBeVisible();
        await expect(stopButton).toHaveText('STOP GAME');
    });
});
