import { test, expect } from '@playwright/test';

test('displays the current player highscore in the next game', async ({ page, request }) => {
  const playerName = 'E2EWEB';

  try {
    // Make the test repeatable even after an interrupted previous run.
    await request.delete(`http://localhost:5000/api/admin/purge-player?playerName=${encodeURIComponent(playerName)}`);
    await request.post('http://localhost:5000/api/leaderboard/submit', {
      data: {
        playerName,
        score: 999,
        language: 'english',
        difficulty: 'beginner',
        topic: 'beverages'
      }
    });

    await page.addInitScript(() => {
      localStorage.setItem('preferredLanguage', 'english');
      localStorage.setItem('preferredDifficulty', 'beginner');
      localStorage.setItem('preferredTopic', 'beverages');
      localStorage.setItem('preferredPlayerName', '  E2EWEB  ');
    });

    await page.goto('http://localhost:8080');
    await page.click('#btn-start');
    await expect(page.locator('#game-highscore')).toHaveText('999 (E2EWEB)', { timeout: 10000 });
  } finally {
    await request.delete(`http://localhost:5000/api/admin/purge-player?playerName=${encodeURIComponent(playerName)}`);
  }
});
