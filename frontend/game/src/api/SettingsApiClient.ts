import { Difficulty, GamePreferences, isDifficulty, isLanguage, isTopic, Language, Topic } from "../gameSettings";

export class SettingsApiClient {
  private readonly baseUrl = `${process.env.GAME_API_URL.replace(/\/game$/, "")}/settings`;

  async get(playerName: string): Promise<GamePreferences> {
    const response = await fetch(`${this.baseUrl}/${encodeURIComponent(playerName)}`);
    if (!response.ok) throw new Error(`Failed to load settings (${response.status})`);
    return this.map(await response.json(), playerName);
  }

  async save(preferences: GamePreferences): Promise<GamePreferences> {
    const response = await fetch(`${this.baseUrl}/${encodeURIComponent(preferences.playerName)}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ language: preferences.language, difficulty: preferences.difficulty, topic: preferences.topic })
    });
    if (!response.ok) throw new Error(`Failed to save settings (${response.status})`);
    return this.map(await response.json(), preferences.playerName);
  }

  private map(value: any, fallbackPlayerName: string): GamePreferences {
    return {
      language: isLanguage(value?.language) ? value.language as Language : "english",
      difficulty: isDifficulty(value?.difficulty) ? value.difficulty as Difficulty : "beginner",
      topic: isTopic(value?.topic) ? value.topic as Topic : "car-parts",
      playerName: typeof value?.playerName === "string" ? value.playerName : fallbackPlayerName
    };
  }
}
