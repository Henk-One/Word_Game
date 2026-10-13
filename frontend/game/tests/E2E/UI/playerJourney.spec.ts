import { test, expect } from '@playwright/test';

test.describe('End-to-End Player Gameplay Journey', () => {

  test('Full playthrough from Main Menu to Settings to Game Victory', async ({ page }) => {
    const targetWord = 'CAR';
    const guessedLetters = new Set<string>();

    await page.route('**/start', async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          gameId: 'E2E-HTML-GAME',
          wordLength: targetWord.length,
          revealed: '---',
          fullWord: targetWord,
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

    await page.route('**/guess', async route => {
      const { letter } = route.request().postDataJSON() as { letter: string };
      guessedLetters.add(letter.toUpperCase());
      const roundComplete = [...targetWord].every(character => guessedLetters.has(character));

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          gameId: 'E2E-HTML-GAME',
          revealed: [...targetWord].map(character => guessedLetters.has(character) ? character : '-').join(''),
          fullWord: targetWord,
          lives: 11,
          score: roundComplete ? 1 : 0,
          gameOver: false,
          level: 1,
          hints: 0,
          hintCapacity: 3,
          bonusHintProgress: 0,
          solvedWords: roundComplete ? 1 : 0,
          roundNumber: 1,
          roundComplete,
          repeatGuessPenalty: false,
          livesPerLevel: 11
        })
      });
    });

    await page.route('**/stop', async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          gameId: 'E2E-HTML-GAME',
          fullWord: targetWord,
          finalScore: 1,
          qualifiesForLeaderboard: false,
          level: 1,
          solvedWords: 1
        })
      });
    });

    await page.route('**/api/leaderboard/**', async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: '[]'
      });
    });

    await page.goto('http://localhost:8080');
    await expect(page.locator('#view-main-menu')).toHaveClass(/active/);

    await page.getByRole('button', { name: 'SETTINGS', exact: true }).click();
    await page.locator('#set-difficulty').selectOption('intermediate');
    await page.locator('#set-player-name').fill('E2E_UI');
    await page.getByRole('button', { name: 'SAVE', exact: true }).click();
    await expect(page.locator('#menu-lbl-difficulty')).toHaveText('INTERMEDIATE');

    await page.getByRole('button', { name: 'START GAME', exact: true }).click();
    await expect(page.locator('#view-game-scene')).toHaveClass(/active/);
    await expect(page.locator('#game-score')).toHaveText('0');

    for (const letter of ['c', 'a', 'r']) {
      await page.locator(`#key-${letter}`).click();
    }

    await expect(page.locator('#game-score')).toHaveText('1');
    await expect(page.locator('#game-word-blanks')).toContainText('CAR');

    await page.getByRole('button', { name: 'STOP GAME', exact: true }).click();
    await expect(page.locator('#game-announcement-banner')).toContainText('Game Stopped. Final Score: 1.');
  });
});
