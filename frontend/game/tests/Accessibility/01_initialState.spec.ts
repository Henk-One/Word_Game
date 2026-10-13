import { test, expect } from '@playwright/test';

test('The first main-menu action can receive focus', async ({ page }) => {
    await page.goto('http://localhost:8080');
    await page.waitForSelector('#btn-start', { state: 'visible' });

    const startButton = page.getByRole('button', { name: 'START GAME', exact: true });
    await startButton.focus();
    await expect(startButton).toBeFocused();
});