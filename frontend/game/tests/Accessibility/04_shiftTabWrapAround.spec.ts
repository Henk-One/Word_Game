import { test, expect } from '@playwright/test';

test('Pressing Shift+Tab reverses focus navigation accurately', async ({ page }) => {
    await page.goto('http://localhost:8080');
    await page.waitForSelector('#btn-start', { state: 'visible' });

    const startButton = page.getByRole('button', { name: 'START GAME', exact: true });
    await startButton.focus();
    await page.keyboard.press('Shift+Tab');

    const settingsButton = page.getByRole('button', { name: 'SETTINGS', exact: true });
    await expect(settingsButton).toBeFocused();
});