import { expect, test } from '@playwright/test';

test.describe('Gameplay mechanics', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('http://localhost:8080');
    await page.click('#btn-start');
    await expect(page.locator('#view-game-scene')).toHaveClass(/active/);
  });

  test('disables a guessed keyboard button and updates the game state', async ({ page }) => {
    const key = page.locator('#key-z');
    await expect(key).toBeEnabled();
    await key.click();
    await expect(key).toBeDisabled();
  });

  test('resets the game state and keyboard buttons on restart', async ({ page }) => {
    await page.locator('#key-z').click();
    await page.click('#btn-game-restart');
    await expect(page.locator('#game-score')).toHaveText('0');
    await expect(page.locator('#game-level')).toHaveText('1');
    await expect(page.locator('#key-z')).toBeEnabled();
  });

  test('disables the hint button after live hints are exhausted', async ({ page }) => {
    const hintButton = page.locator('#btn-game-hint');
    for (let attempt = 0; attempt < 10 && await hintButton.isEnabled(); attempt++) {
      // The API response can disable the button between isEnabled() and click().
      // Treat that race as the expected exhausted-hints state.
      await hintButton.click({ timeout: 1000 }).catch(() => undefined);
      await page.waitForTimeout(150);
    }
    await expect(hintButton).toBeDisabled();
  });

  test('stops the active game and returns to the main menu', async ({ page }) => {
    await page.click('#btn-game-stop');
    await expect(page.locator('#game-announcement-banner')).toContainText(/Game Stopped|final score/i);
    await expect(page.locator('#view-main-menu')).toHaveClass(/active/, { timeout: 5000 });
  });
});
