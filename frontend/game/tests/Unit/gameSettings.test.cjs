const assert = require("node:assert/strict");
const { afterEach, beforeEach, test } = require("node:test");

const values = new Map();
global.localStorage = {
  getItem(key) {
    return values.has(key) ? values.get(key) : null;
  },
  setItem(key, value) {
    values.set(key, String(value));
  }
};

const {
  getGamePreferences,
  getPlayerDisplayName,
  saveGamePreferences,
  savePlayerName
} = require("../../.test-dist/gameSettings.js");

beforeEach(() => values.clear());
afterEach(() => values.clear());

test("getGamePreferences returns defaults when nothing is saved", () => {
  assert.deepEqual(getGamePreferences(), {
    language: "english",
    difficulty: "beginner",
    topic: "car-parts",
    playerName: ""
  });
});

test("getGamePreferences validates saved values and trims the player name", () => {
  values.set("preferredLanguage", "dutch");
  values.set("preferredDifficulty", "intermediate");
  values.set("preferredTopic", "beverages");
  values.set("preferredPlayerName", "  Alex  ");

  assert.deepEqual(getGamePreferences(), {
    language: "dutch",
    difficulty: "intermediate",
    topic: "beverages",
    playerName: "Alex"
  });

  values.set("preferredLanguage", "unsupported");
  values.set("preferredPlayerName", "name-too-long");
  assert.deepEqual(getGamePreferences(), {
    language: "english",
    difficulty: "intermediate",
    topic: "beverages",
    playerName: ""
  });
});

test("saveGamePreferences stores preferences and trims the player name", () => {
  saveGamePreferences({
    language: "indonesian",
    difficulty: "beginner",
    topic: "beverages",
    playerName: "  Alex  "
  });

  assert.deepEqual(getGamePreferences(), {
    language: "indonesian",
    difficulty: "beginner",
    topic: "beverages",
    playerName: "Alex"
  });
});

test("savePlayerName rejects names longer than eight characters", () => {
  assert.throws(() => savePlayerName("123456789"), {
    message: "Player name must be 8 characters or fewer."
  });
});

test("getPlayerDisplayName maps blank and default player names to YOU", () => {
  assert.equal(getPlayerDisplayName("  "), "YOU");
  assert.equal(getPlayerDisplayName("player"), "YOU");
  assert.equal(getPlayerDisplayName(" Alex "), "Alex");
});
