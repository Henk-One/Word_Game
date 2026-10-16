import { expect, test } from '@playwright/test';

test.describe('End-to-End Player Gameplay Journey', () => {
  test('starts a live game, shows the game UI, and stops it', async ({ page }) => {
    await page.goto('http://localhost:8080');
    await expect(page.locator('#view-main-menu')).toHaveClass(/active/);
    await page.getByRole('button', { name: 'START GAME', exact: true }).click();
    await expect(page.locator('#view-game-scene')).toHaveClass(/active/);
    await expect(page.locator('#game-score')).toHaveText('0');
    await expect(page.locator('#game-word-blanks .word-letter-blank').first()).toBeVisible();
    await page.locator('#key-z').click();
    await expect(page.locator('#key-z')).toBeDisabled();
    await page.getByRole('button', { name: 'STOP GAME', exact: true }).click();
    await expect(page.locator('#game-announcement-banner')).toContainText(/Game Stopped|final score/i);
    await expect(page.locator('#view-main-menu')).toHaveClass(/active/, { timeout: 5000 });
  });
});
