import { test, expect } from '@playwright/test';

test.describe('Settings Panel Configuration Workflows', () => {

    test.beforeEach(async ({ page }) => {
        // 1. Navigate to the local server asset viewport
        await page.goto('http://localhost:8080');
        await page.waitForSelector('#btn-settings', { state: 'visible' });

        // 2. Click settings to pull up the HTML options form panel
        await page.click('#btn-settings');
        await page.waitForSelector('#set-difficulty', { state: 'visible' });
    });

    test('Should successfully modify difficulty dropdown to BEGINNER and save changes', async ({ page }) => {
        const difficultySelect = page.locator('#set-difficulty');
        await difficultySelect.selectOption('beginner');
        await expect(difficultySelect).toHaveValue('beginner');

        await page.fill('#set-player-name', 'SET_BEG');
        await page.click('#btn-settings-save');

        const menuDifficultyLabel = page.locator('#menu-lbl-difficulty');
        await expect(menuDifficultyLabel).toHaveText('BEGINNER');
    });

    test('Should successfully modify difficulty dropdown to INTERMEDIATE and save changes', async ({ page }) => {
        const difficultySelect = page.locator('#set-difficulty');
        await difficultySelect.selectOption('intermediate');
        await expect(difficultySelect).toHaveValue('intermediate');

        await page.fill('#set-player-name', 'SET_INT');
        await page.click('#btn-settings-save');

        const menuDifficultyLabel = page.locator('#menu-lbl-difficulty');
        await expect(menuDifficultyLabel).toHaveText('INTERMEDIATE');
    });

    // 🔄 ADDED: Cancel button rollback state validation test
    test('Should discard unsaved configuration changes when hitting the CANCEL button', async ({ page }) => {
        const difficultySelect = page.locator('#set-difficulty');

        // 1. First, explicitly ensure our starting baseline state is committed to BEGINNER
        await difficultySelect.selectOption('beginner');
        await page.fill('#set-player-name', 'CNCL_TST');
        await page.click('#btn-settings-save');

        // 2. Re-open the settings menu to execute the modification test
        await page.click('#btn-settings');
        await page.waitForSelector('#set-difficulty', { state: 'visible' });

        // 3. Change dropdown to INTERMEDIATE but do NOT save
        await difficultySelect.selectOption('intermediate');
        await expect(difficultySelect).toHaveValue('intermediate');

        // 4. Click the CANCEL button to discard the changes and route back
        await page.click('#btn-settings-cancel');

        // 5. Re-open the settings panel a third time to inspect data persistence
        await page.click('#btn-settings');
        await page.waitForSelector('#set-difficulty', { state: 'visible' });

        // 6. Assert that the layout has rolled back to BEGINNER
        await expect(difficultySelect).toHaveValue('beginner');
    });
});

test.describe('Settings Panel LocalStorage Edge Cases', () => {
    test('Should display trimmed player name in settings form when localStorage contains untrimmed player name', async ({ page }) => {
        await page.addInitScript(() => {
            localStorage.setItem('preferredPlayerName', '  Alex  ');
        });

        await page.goto('http://localhost:8080');
        await page.waitForSelector('#btn-settings', { state: 'visible' });
        await page.click('#btn-settings');
        await page.waitForSelector('#set-player-name', { state: 'visible' });

        const inputName = page.locator('#set-player-name');
        await expect(inputName).toHaveValue('Alex');
    });
});

