// frontend/game/src/api/GameApiClient.ts

export interface StartGameResponse {
  gameId: string;
  wordLength: number;
  revealed: string;
  lives: number;
  fullWord: string;
  score: number;
  level: number;
  hints: number;
  hintCapacity: number;
  bonusHintProgress: number;
  solvedWords: number;
  roundNumber: number;
  repeatGuessPenalty: boolean;
  livesPerLevel: number;
}

export interface GuessResponse {
  gameId: string;
  revealed: string;
  fullWord: string;
  lives: number;
  score: number;
  gameOver: boolean;
  level: number;
  hints: number;
  hintCapacity: number;
  bonusHintProgress: number;
  solvedWords: number;
  roundNumber: number;
  roundComplete: boolean;
  repeatGuessPenalty: boolean;
  livesPerLevel: number;
}

export interface HintResponse {
  gameId: string;
  revealed: string;
  fullWord: string;
  lives: number;
  score: number;
  level: number;
  hints: number;
  hintCapacity: number;
  bonusHintProgress: number;
  solvedWords: number;
  roundNumber: number;
  roundComplete: boolean;
}

export interface FinishGameResponse {
  gameId: string;
  fullWord: string;
  finalScore: number;
  qualifiesForLeaderboard: boolean;
  level: number;
  solvedWords: number;
}

export class GameApiClient {
  private baseUrl = process.env.GAME_API_URL;

  async start(language: string, difficulty: string, topic: string, playerName: string): Promise<StartGameResponse> {
    const response = await fetch(`${this.baseUrl}/start`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        language,
        difficulty,
        topic,
        playerName
      })
    });

    if (!response.ok) {
      throw new Error("Failed to start game");
    }

    return response.json();
  }

  async nextRound(gameId: string): Promise<StartGameResponse> {
  const response = await fetch(
    `${this.baseUrl}/next-round?gameId=${gameId}`,
    {
      method: "POST"
    }
  );

  if (!response.ok) {
    throw new Error("Failed to start next round");
  }

  return await response.json();
}

  async guess(gameId: string, letter: string): Promise<GuessResponse> {
    const response = await fetch(`${this.baseUrl}/guess`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        gameId,
        letter
      })
    });

    if (!response.ok) {
      throw new Error("Failed to submit guess");
    }

    return response.json();
  }

  async hint(gameId: string): Promise<HintResponse> {
    const response = await fetch(`${this.baseUrl}/hint`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ gameId })
    });

    if (!response.ok) {
      throw new Error("Failed to use hint");
    }

    return response.json();
  }

  async stop(gameId: string): Promise<FinishGameResponse> {
  const response = await fetch(`${this.baseUrl}/stop`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ gameId })
  });

  if (!response.ok) {
    throw new Error("Failed to stop game");
  }
  return response.json();
}

async finish(gameId: string): Promise<FinishGameResponse> {
  const response = await fetch(`${this.baseUrl}/finish`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ gameId })
  });

  if (!response.ok) {
    throw new Error("Failed to naturally finish game");
  }
  return response.json();
}

  
}
