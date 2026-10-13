export type Language = "english" | "dutch" | "indonesian";
export type Difficulty = "beginner" | "intermediate";
export type Topic = "car-parts" | "beverages";

export interface GamePreferences {
  language: Language;
  difficulty: Difficulty;
  topic: Topic;
  playerName: string;
}

const defaultPreferences: GamePreferences = {
  language: "english",
  difficulty: "beginner",
  topic: "car-parts",
  playerName: ""
};

export function getGamePreferences(): GamePreferences {
  const language = localStorage.getItem("preferredLanguage");
  const difficulty = localStorage.getItem("preferredDifficulty");
  const topic = localStorage.getItem("preferredTopic");
  const playerName = localStorage.getItem("preferredPlayerName");
  const savedPlayerName = playerName === "PLAYER" ? null : playerName;

  return {
    language: isLanguage(language) ? language : defaultPreferences.language,
    difficulty: isDifficulty(difficulty) ? difficulty : defaultPreferences.difficulty,
    topic: isTopic(topic) ? topic : defaultPreferences.topic,
    playerName: isValidPlayerName(savedPlayerName)
      ? savedPlayerName.trim()
      : defaultPreferences.playerName
  };
}

export function saveGamePreferences(preferences: GamePreferences): void {
  localStorage.setItem("preferredLanguage", preferences.language);
  localStorage.setItem("preferredDifficulty", preferences.difficulty);
  localStorage.setItem("preferredTopic", preferences.topic);
  savePlayerName(preferences.playerName);
}

export function savePlayerName(playerName: string): void {
  if (!isValidPlayerName(playerName)) {
    throw new Error("Player name must be 8 characters or fewer.");
  }

  localStorage.setItem("preferredPlayerName", playerName.trim());
}

export function getPlayerDisplayName(playerName: string): string {
  const trimmed = playerName.trim();
  return !trimmed || trimmed.toUpperCase() === "PLAYER" ? "YOU" : trimmed;
}

export function isLanguage(value: unknown): value is Language {
  return value === "english" || value === "dutch" || value === "indonesian";
}

export function isDifficulty(value: unknown): value is Difficulty {
  return value === "beginner" || value === "intermediate";
}

export function isTopic(value: unknown): value is Topic {
  return value === "car-parts" || value === "beverages";
}

export function isValidPlayerName(value: unknown): value is string {
  if (typeof value !== "string") {
    return false;
  }

  const trimmed = value.trim();
  return trimmed.length <= 8;
}
