import { GameApiClient } from "./api/GameApiClient";
import { SettingsApiClient } from "./api/SettingsApiClient";
import {
  getGamePreferences,
  saveGamePreferences,
  savePlayerName,
  Language,
  Difficulty,
  Topic,
  GamePreferences
} from "./gameSettings";

export interface HighScoreRecord {
  rank: string;
  playerName: string;
  score: number;
  formattedDate: string;
}

// =========================================================================
// MODULE 1: NETWORK & DATA SERVICE (High Cohesion)
// Pure data operations. Strictly NO DOM manipulation or window queries.
// =========================================================================
export class LeaderboardDataService {
  public async fetchCategoryLeaderboard(
    language: Language,
    difficulty: Difficulty,
    topic: Topic
  ): Promise<HighScoreRecord[]> {
    const targetUrl = `http://localhost:5000/api/leaderboard/${language}/${difficulty}/${topic}`;
    const response = await fetch(targetUrl, { method: "GET" });
    if (!response.ok) throw new Error(`HTTP Error: ${response.status}`);

    const rawRecords: Array<{ playerName: string; score: number; date: string }> = await response.json();
    return rawRecords.map((record, index) => ({
      rank: `#${index + 1}`,
      playerName: this.escapeHtmlSecurityShield(record.playerName),
      score: record.score,
      formattedDate: this.formatHighScoreDate(record.date)
    }));
  }

  public async fetchTopScoreValueString(language: string, difficulty: string, topic: string): Promise<string> {
    try {
      const targetUrl = `http://localhost:5000/api/leaderboard/${language}/${difficulty}/${topic}`;
      const response = await fetch(targetUrl, { method: "GET" });
      if (response.ok) {
        const entries = await response.json();
        if (entries && entries.length > 0) {
          return `${entries[0].score} (${entries[0].playerName.toUpperCase()})`;
        }
      }
    } catch { /* Fallback to safe zero state */ }
    return "0";
  }

  private escapeHtmlSecurityShield(str: string): string {
    return str
      .replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;").replace(/'/g, "&#039;");
  }

  private formatHighScoreDate(value: string): string {
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return "Unknown";
    return new Intl.DateTimeFormat(undefined, { dateStyle: "medium", timeStyle: "short" }).format(date);
  }
}
// =========================================================================
// MODULE 2: PURE DOM DISPLAY ENGINE (Separation of Concerns)
// Pure visual mutations. Strictly NO fetch, network dependencies, or API endpoints.
// =========================================================================
export class AppDisplayEngine {
  public renderLeaderboardRows(scores: HighScoreRecord[]): void {
    const tableBody = document.getElementById("leaderboard-data-rows");
    if (!tableBody) return;
    tableBody.innerHTML = "";

    if (scores.length === 0) {
      tableBody.innerHTML = `<tr><td colspan="4" style="text-align:center; padding: 20px; color:#aaaaaa;">No highscores recorded for this category combination yet. Be the first!</td></tr>`;
      return;
    }

    scores.forEach(record => {
      const row = document.createElement("tr");
      row.innerHTML = `<td>${record.rank}</td><td>${record.playerName}</td><td>${record.score}</td><td class="highscore-date">${record.formattedDate}</td>`;
      tableBody.appendChild(row);
    });
  }

  public renderLeaderboardStatusMessage(message: string, isError: boolean = false): void {
    const tableBody = document.getElementById("leaderboard-data-rows");
    if (!tableBody) return;
    tableBody.innerHTML = `<tr><td colspan="4" class="${isError ? "table-error" : "table-loading"}">${message}</td></tr>`;
  }

  public updateSettingsFormValues(prefs: GamePreferences): void {
    const selectLang = document.getElementById("set-language") as HTMLSelectElement;
    const selectDiff = document.getElementById("set-difficulty") as HTMLSelectElement;
    const selectTopic = document.getElementById("set-topic") as HTMLSelectElement;
    const inputName = document.getElementById("set-player-name") as HTMLInputElement;

    if (selectLang) selectLang.value = prefs.language;
    if (selectDiff) selectDiff.value = prefs.difficulty;
    if (selectTopic) selectTopic.value = prefs.topic;
    if (inputName) inputName.value = prefs.playerName;
  }

  public renderWordBlanks(pattern: string[]): void {
    const wordContainer = document.getElementById("game-word-blanks");
    if (!wordContainer) return;
    wordContainer.innerHTML = "";
    pattern.forEach(char => {
      const charSpan = document.createElement("span");
      charSpan.className = "word-letter-blank";
      charSpan.textContent = (char === "-" || char === "_") ? "" : char;
      wordContainer.appendChild(charSpan);
    });
  }

  public showOnScreenMessage(message: string, isSuccess: boolean = false): void {
    const banner = document.getElementById("game-announcement-banner");
    if (!banner) return;
    banner.textContent = message;
    banner.style.display = "block";
    banner.style.borderColor = isSuccess ? "#00ff00" : "#ff9900";
    banner.style.color = isSuccess ? "#00ff00" : "#ffff00";
  }

  public hideOnScreenMessage(): void {
    const banner = document.getElementById("game-announcement-banner");
    if (banner) banner.style.display = "none";
  }
}
// =========================================================================
// MODULE 3: APP VIEW CONTROLLER / ORCHESTRATOR (Loose Coupling) - DEEL A
// Coordinates app lifecycle state, event channels, and sub-module data flows.
// =========================================================================
class AppViewController {
  private api: GameApiClient;
  private settingsApi: SettingsApiClient;

  // Decoupled sub-module engines
  private dataService: LeaderboardDataService;
  private display: AppDisplayEngine;

  // Gameplay State Trackers
  private currentGameId: string = "";
  private score: number = 0;
  private level: number = 1;
  private lives: number = 11;
  private livesPerLevel: number = 11;
  private hints: number = 0;
  private hintCapacity: number = 0;
  private repeatGuessPenalty: boolean = false;
  private stopNavigationTimer: number | undefined;

  private highscoreLanguage: Language | undefined;
  private highscoreDifficulty: Difficulty | undefined;
  private highscoreTopic: Topic | undefined;

  private readonly highscoreLanguages: Language[] = ["english", "dutch", "indonesian"];
  private readonly highscoreDifficulties: Difficulty[] = ["beginner", "intermediate"];
  private readonly highscoreTopics: Topic[] = ["beverages", "car-parts"];

  private currentWordPattern: string[] = [];
  private guessedLetters: Set<string> = new Set();

  constructor() {
    this.api = new GameApiClient();
    this.settingsApi = new SettingsApiClient();

    // Instantiate decoupled software layers
    this.dataService = new LeaderboardDataService();
    this.display = new AppDisplayEngine();

    this.initEventListeners();
    this.initFocusTrap();
    this.updateMenuMetadata();
  }

  private applyBackendState(response: any): void {
    this.score = response.score ?? response.Score ?? 0;
    this.level = response.level ?? response.Level ?? 1;
    this.lives = response.lives ?? response.Lives ?? 0;
    this.livesPerLevel = response.livesPerLevel ?? response.LivesPerLevel ?? 11;
    this.hints = response.hints ?? response.Hints ?? 0;
    this.hintCapacity = response.hintCapacity ?? response.HintCapacity ?? 3;
    this.repeatGuessPenalty = response.repeatGuessPenalty ?? response.RepeatGuessPenalty ?? false;

    const rawRevealed = response.revealed ?? response.Revealed;
    if (rawRevealed) {
      this.currentWordPattern = rawRevealed.split("");
    }

    this.display.renderWordBlanks(this.currentWordPattern);
    void this.renderGameplayHUD();
  }

  private async renderGameplayHUD(): Promise<void> {
    const lblScore = document.getElementById("game-score");
    const lblLevel = document.getElementById("game-level");
    const lblHighscore = document.getElementById("game-highscore");
    const containerLives = document.getElementById("game-lives-container");
    const containerHints = document.getElementById("game-hints-container");
    const hintButton = document.getElementById("btn-game-hint") as HTMLButtonElement | null;
    const tooltipRules = document.getElementById("level-rules-tooltip");

    if (lblScore) lblScore.textContent = this.score.toString();
    if (lblLevel) lblLevel.textContent = this.level.toString();

    if (lblHighscore) {
      const prefs = getGamePreferences();
      lblHighscore.textContent = await this.dataService.fetchTopScoreValueString(prefs.language, prefs.difficulty, prefs.topic);
    }

    if (containerLives) {
      containerLives.textContent = "❤️".repeat(this.lives) + "🖤".repeat(Math.max(0, this.livesPerLevel - this.lives));
    }

    if (containerHints) {
      containerHints.innerHTML = `Hint: ${"🪙".repeat(this.hints)}<span style="opacity: 0.25;">${"🪙".repeat(Math.max(0, this.hintCapacity - this.hints))}</span>`;
    }

    if (hintButton) hintButton.disabled = this.hints <= 0 || this.lives <= 0;

    if (tooltipRules) {
      tooltipRules.innerHTML = `<strong>LEVEL ${this.level} CONSTRAINTS:</strong><br>Allowed Mistakes: ${this.livesPerLevel} Hearts<br>Hint Resources: ${this.hintCapacity} Capacity<br>Duplicate Penalty: ${this.repeatGuessPenalty ? "ACTIVE" : "DISABLED"}`;
    }
  }


  private initEventListeners(): void {
    document.getElementById("btn-start")?.addEventListener("click", () => {
      void this.startNewGameSession();
      this.switchView("view-game-scene");
    });



    document.getElementById("btn-game-restart")?.addEventListener("click", async () => {
      this.cleanupGameplayListeners();
      this.currentGameId = "";
      await this.startNewGameSession();
    });

    document.getElementById("btn-game-hint")?.addEventListener("click", () => {
      void this.handleHintUsage();
    });

    document.getElementById("btn-game-stop")?.addEventListener("click", async () => {
      if (this.currentGameId) {
        try {
          const result = await this.api.stop(this.currentGameId);
          if (result.qualifiesForLeaderboard) {
            this.display.showOnScreenMessage(`Congratulations! Your final score of ${result.finalScore} qualified for the Top 10 Leaderboard!`, true);
          } else {
            this.display.showOnScreenMessage(`Game Stopped. Final Score: ${result.finalScore}.`, false);
          }

          this.stopNavigationTimer = window.setTimeout(() => {
            this.stopNavigationTimer = undefined;
            this.cleanupGameplayListeners();
            this.updateMenuMetadata();
            this.switchView("view-main-menu");
          }, 3500);
          return;
        } catch {
          this.display.showOnScreenMessage("Unable to stop the game. Please try again.");
          return;
        }
      }
      this.cleanupGameplayListeners();
      this.updateMenuMetadata();
      this.switchView("view-main-menu");
    });

    document.getElementById("btn-highscores")?.addEventListener("click", () => {
      const prefs = getGamePreferences();
      this.highscoreLanguage = prefs.language;
      this.highscoreDifficulty = prefs.difficulty;
      this.highscoreTopic = prefs.topic;
      void this.fetchAndRenderHighScores();
      this.switchView("view-highscores");
    });

    document.getElementById("btn-highscores-previous")?.addEventListener("click", () => {
      this.navigateHighscores(-1);
    });

    document.getElementById("btn-highscores-next")?.addEventListener("click", () => {
      this.navigateHighscores(1);
    });

    document.getElementById("btn-settings")?.addEventListener("click", async () => {
      await this.loadSettingsForm();
      this.switchView("view-settings");
    });

    document.getElementById("btn-settings-cancel")?.addEventListener("click", () => {
      this.clearSettingsError();
      this.updateMenuMetadata();
      this.switchView("view-main-menu");
    });

    document.getElementById("btn-settings-save")?.addEventListener("click", () => {
      void this.saveSettingsForm();
    });

    document.querySelectorAll(".btn-back, #btn-highscores-back").forEach(button => {
      button.addEventListener("click", () => {
        this.updateMenuMetadata();
        this.switchView("view-main-menu");
      });
    });
  }

  // Orchestrated Highscore Flow - Satisfies Loose Coupling rule
  private async fetchAndRenderHighScores(): Promise<void> {
    this.display.renderLeaderboardStatusMessage("Loading records from server...");

    const language = this.highscoreLanguage ?? getGamePreferences().language;
    const difficulty = this.highscoreDifficulty ?? getGamePreferences().difficulty;
    const topic = this.highscoreTopic ?? getGamePreferences().topic;

    const summary = document.getElementById("highscores-lbl-summary");
    if (summary) {
      summary.textContent = `${language.toUpperCase()} / ${difficulty.toUpperCase()} / ${topic.toUpperCase().replace("-", " ")}`;
    }
    this.updateHighscoreNavigation();

    try {
      // 1. READ through decoupled service mapping zone
      const transformedScores = await this.dataService.fetchCategoryLeaderboard(language, difficulty, topic);
      // 2. SHOW through the pure display engine channel
      this.display.renderLeaderboardRows(transformedScores);
    } catch {
      this.display.renderLeaderboardStatusMessage("Error fetching leaderboards. Make sure Leaderboard.Api is running on port 5000.", true);
    }
  }

  private async startNewGameSession(): Promise<void> {
    this.guessedLetters.clear();
    this.display.hideOnScreenMessage();

    const prefs = getGamePreferences();
    try {
      const response = await this.api.start(prefs.language, prefs.difficulty, prefs.topic, prefs.playerName || "PLAYER");
      this.currentGameId = response.gameId || (response as any).GameId;
      this.applyBackendState(response);

      this.generateVirtualKeyboard();
      this.attachPhysicalKeyboardListener();
    } catch {
      const wordContainer = document.getElementById("game-word-blanks");
      if (wordContainer) {
        wordContainer.innerHTML = `<span style="color:#ff4444; font-size:18px;">Game Server unreachable on port 5151.</span>`;
      }
    }
  }


  private navigateHighscores(direction: -1 | 1): void {
    const languageIndex = this.highscoreLanguages.indexOf(this.highscoreLanguage!);
    const difficultyIndex = this.highscoreDifficulties.indexOf(this.highscoreDifficulty!);
    const topicIndex = this.highscoreTopics.indexOf(this.highscoreTopic!);
    const total = this.highscoreDifficulties.length * this.highscoreTopics.length;
    const currentIndex = languageIndex * total + difficultyIndex * this.highscoreTopics.length + topicIndex;
    const nextIndex = currentIndex + direction;

    if (nextIndex < 0 || nextIndex >= this.highscoreLanguages.length * total) return;

    this.highscoreLanguage = this.highscoreLanguages[Math.floor(nextIndex / total)];
    const remainder = nextIndex % total;
    this.highscoreDifficulty = this.highscoreDifficulties[Math.floor(remainder / this.highscoreTopics.length)];
    this.highscoreTopic = this.highscoreTopics[remainder % this.highscoreTopics.length];

    void this.fetchAndRenderHighScores();
  }

  private updateHighscoreNavigation(): void {
    const languageIndex = this.highscoreLanguages.indexOf(this.highscoreLanguage!);
    const difficultyIndex = this.highscoreDifficulties.indexOf(this.highscoreDifficulty!);
    const topicIndex = this.highscoreTopics.indexOf(this.highscoreTopic!);
    const total = this.highscoreDifficulties.length * this.highscoreTopics.length;
    const currentIndex = languageIndex * total + difficultyIndex * this.highscoreTopics.length + topicIndex;
    const lastIndex = this.highscoreLanguages.length * total - 1;

    const previous = document.getElementById("btn-highscores-previous") as HTMLButtonElement | null;
    const next = document.getElementById("btn-highscores-next") as HTMLButtonElement | null;
    if (previous) previous.disabled = currentIndex <= 0;
    if (next) next.disabled = currentIndex >= lastIndex;
  }
  private async handleLetterGuess(letter: string): Promise<void> {
    if (this.guessedLetters.has(letter) || this.lives <= 0) return;
    this.guessedLetters.add(letter);

    const virtualKey = document.getElementById(`key-${letter}`) as HTMLButtonElement;
    if (virtualKey) virtualKey.disabled = true;

    try {
      const response = await this.api.guess(this.currentGameId, letter);
      if (virtualKey) {
        const isCorrect = response.revealed.toUpperCase().includes(letter.toUpperCase());
        virtualKey.classList.add(isCorrect ? "key-correct" : "key-incorrect");
      }

      this.applyBackendState(response);

      if (response.roundComplete) {
        setTimeout(() => void this.loadNextRound(), 2000);
        return;
      }

      if (response.gameOver) {
        const targetFullWord = response.fullWord || "";
        try { await this.api.finish(this.currentGameId); } catch {
          this.display.showOnScreenMessage("Game over, but the final score could not be saved.");
        }

        if (targetFullWord) {
          this.currentWordPattern = targetFullWord.split("");
          this.display.renderWordBlanks(this.currentWordPattern);
        }

        this.display.showOnScreenMessage(`Game Over! The word was: ${targetFullWord}.`, false);
        Array.from(document.querySelectorAll('.key-btn')).forEach(btn => (btn as HTMLButtonElement).disabled = true);
        this.cleanupGameplayListeners();

        setTimeout(() => this.switchView("view-main-menu"), 3500);
        return;
      }
    } catch {
      this.display.showOnScreenMessage("Unable to submit your guess. Please try again.");
    }
  }

  private async handleHintUsage(): Promise<void> {
    if (this.lives <= 0 || !this.currentGameId) return;
    try {
      const response = await this.api.hint(this.currentGameId);
      this.applyBackendState(response);
      if ((response as any).roundComplete) {
        setTimeout(() => void this.loadNextRound(), 2000);
      }
    } catch {
      this.display.showOnScreenMessage("Unable to use a hint. Please try again.");
    }
  }

  private async loadNextRound(): Promise<void> {
    try {
      const response = await this.api.nextRound(this.currentGameId);
      this.guessedLetters.clear();
      this.applyBackendState(response);
      this.generateVirtualKeyboard();
    } catch {
      this.display.showOnScreenMessage("Unable to load the next word. Please try again.");
    }
  }

  private async loadSettingsForm(): Promise<void> {
    let prefs = getGamePreferences();
    try {
      const serverPrefs = await this.settingsApi.get(prefs.playerName || "PLAYER");
      prefs = { ...prefs, ...serverPrefs };
      saveGamePreferences(prefs);
    } catch { /* Local fallback logic */ }
    this.display.updateSettingsFormValues(prefs);
    this.clearSettingsError();
  }

  private async saveSettingsForm(): Promise<void> {
    const selectLang = document.getElementById("set-language") as HTMLSelectElement;
    const selectDiff = document.getElementById("set-difficulty") as HTMLSelectElement;
    const selectTopic = document.getElementById("set-topic") as HTMLSelectElement;
    const inputName = document.getElementById("set-player-name") as HTMLInputElement;
    const errorDisplay = document.getElementById("settings-error-text");

    const trimmedName = inputName?.value.trim() || "";
    if (trimmedName.length === 0) {
      if (errorDisplay) errorDisplay.textContent = "Player name cannot be empty.";
      inputName?.focus();
      return;
    }

    const updatedPrefs = {
      language: selectLang.value as Language,
      difficulty: selectDiff.value as Difficulty,
      topic: selectTopic.value as Topic,
      playerName: trimmedName
    };

    saveGamePreferences(updatedPrefs);
    savePlayerName(trimmedName);

    try {
      const saved = await this.settingsApi.save(updatedPrefs);
      saveGamePreferences(saved);
    } catch {
      if (errorDisplay) errorDisplay.textContent = "Settings saved locally, but the server could not be reached.";
    }
    this.updateMenuMetadata();
    this.switchView("view-main-menu");
  }

  private generateVirtualKeyboard(): void {
    const keyboardContainer = document.getElementById("game-virtual-keyboard");
    if (!keyboardContainer) return;
    keyboardContainer.innerHTML = "";
    "ABCDEFGHIJKLMNOPQRSTUVWXYZ".split("").forEach(letter => {
      const button = document.createElement("button");
      button.type = "button";
      button.className = "key-btn";
      button.id = `key-${letter.toLowerCase()}`;
      button.textContent = letter;
      button.addEventListener("click", () => void this.handleLetterGuess(letter.toLowerCase()));
      keyboardContainer.appendChild(button);
    });
  }

  private handlePhysicalKeyDown = (event: KeyboardEvent): void => {
    const key = event.key.toLowerCase();
    if (/^[a-z]\$/.test(key)) void this.handleLetterGuess(key);
  };

  private attachPhysicalKeyboardListener(): void {
    window.addEventListener("keydown", this.handlePhysicalKeyDown);
  }

  private cleanupGameplayListeners(): void {
    window.removeEventListener("keydown", this.handlePhysicalKeyDown);
  }

  private initFocusTrap(): void {
    window.addEventListener("keydown", (event: KeyboardEvent) => {
      if (event.key !== "Tab") return;
      const activePanel = document.querySelector(".view-panel.active");
      if (!activePanel) return;
      const focusables = Array.from(activePanel.querySelectorAll("button, select, input, .key-btn")) as HTMLElement[];
      if (focusables.length === 0) return;

      const firstElement = focusables[0];
      const lastElement = focusables[focusables.length - 1];
      const activeElement = document.activeElement as HTMLElement;

      if (event.shiftKey) {
        if (activeElement === firstElement) { event.preventDefault(); lastElement.focus(); }
      } else {
        if (activeElement === lastElement) { event.preventDefault(); firstElement.focus(); }
      }
    });
  }

  public switchView(viewId: string): void {
    if (this.stopNavigationTimer !== undefined) {
      window.clearTimeout(this.stopNavigationTimer);
      this.stopNavigationTimer = undefined;
    }
    document.querySelectorAll(".view-panel").forEach(p => p.classList.remove("active"));
    const targetPanel = document.getElementById(viewId);
    if (targetPanel) {
      targetPanel.classList.add("active");
      const firstFocusable = targetPanel.querySelector("button, select, input, .key-btn") as HTMLElement;
      if (firstFocusable) firstFocusable.focus();
    }
  }

  private updateMenuMetadata(): void {
    const prefs = getGamePreferences();
    const lblLang = document.getElementById("menu-lbl-language");
    const lblDiff = document.getElementById("menu-lbl-difficulty");
    const lblTopic = document.getElementById("menu-lbl-topic");

    if (lblLang) lblLang.textContent = prefs.language.toUpperCase();
    if (lblDiff) lblDiff.textContent = prefs.difficulty.toUpperCase();
    if (lblTopic) lblTopic.textContent = prefs.topic.replace(/-/g, " ").toUpperCase();

    const summaryString = `${prefs.language} / ${prefs.difficulty} / ${prefs.topic.replace(/-/g, " ")}`.toUpperCase();
    const gameSummary = document.getElementById("game-lbl-summary");
    const scoreSummary = document.getElementById("highscores-lbl-summary");

    if (gameSummary) gameSummary.textContent = summaryString;
    if (scoreSummary) scoreSummary.textContent = summaryString;
  }

  private clearSettingsError(): void {
    const errorDisplay = document.getElementById("settings-error-text");
    if (errorDisplay) errorDisplay.textContent = "";
  }
}

window.addEventListener("DOMContentLoaded", () => {
  (window as any).appController = new AppViewController();
});
