import { test, expect } from '@playwright/test';

test('Pressing Tab sequentially wraps around the focus menu correctly', async ({ page }) => {
    // 1. Navigate to the local server
    await page.goto('http://localhost:8080');
    await page.waitForSelector('#btn-start', { state: 'visible' });

    // Focus the menu button without activating it; clicking it switches to the game panel.
    const startButton = page.getByRole('button', { name: 'START GAME' });
    await startButton.focus();

    await expect(startButton).toBeFocused();

    // 2. Tab down to HIGHSCORES and verify it has focus
    await page.keyboard.press('Tab');
    await page.waitForTimeout(50); // Give the browser thread room to process the keystroke
    const highscoresButton = page.getByRole('button', { name: 'HIGHSCORES' });
    await expect(highscoresButton).toBeFocused();

    // 3. Tab down to SETTINGS and verify it has focus
    await page.keyboard.press('Tab');
    await page.waitForTimeout(50);
    const settingsButton = page.getByRole('button', { name: 'SETTINGS' });
    await expect(settingsButton).toBeFocused();

    // 4. This Tab triggers our custom Focus Trap wrap-around loop back to the top element
    await page.keyboard.press('Tab');
    await page.waitForTimeout(50);

    // 5. Assert that focus successfully looped back to the top
    await expect(startButton).toBeFocused();
});