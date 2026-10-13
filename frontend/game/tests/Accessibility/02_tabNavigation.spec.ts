import { test, expect } from '@playwright/test';

test('Pressing Tab key should smoothly cycle focus down to HIGHSCORES', async ({ page }) => {
    await page.goto('http://localhost:8080');
    await page.waitForSelector('#btn-start', { state: 'visible' });

    const startButton = page.getByRole('button', { name: 'START GAME', exact: true });
    await startButton.focus();
    await page.keyboard.press('Tab');

    const highscoresButton = page.getByRole('button', { name: 'HIGHSCORES', exact: true });
    await expect(highscoresButton).toBeFocused();
});