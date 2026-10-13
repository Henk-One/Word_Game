// 1. Add this import statement at the very top of main.ts:
import { GameApiClient } from "./api/GameApiClient";

import {
  getGamePreferences,
  saveGamePreferences,
  savePlayerName,
  Language,
  Difficulty,
  Topic
} from "./gameSettings";

interface HighScoreRecord {
  playerName: string;
  score: number;
  date: string;
}

class AppViewController {
  private api: GameApiClient; // Declare the backend API client property wrapper
  // Gameplay State Trackers
  private currentGameId: string = ""; // Added to track our active GUID session row
  private score: number = 0;
  private level: number = 1;
  private lives: number = 11;
  // level based settings
  private livesPerLevel: number = 11;
  // 🎯 Declare the live active hints counter property tracker field
  private hints: number = 0;
  private hintCapacity: number = 0;
  private repeatGuessPenalty: boolean = false;
  //
  private currentWordPattern: string[] = [];
  private guessedLetters: Set<string> = new Set();

  constructor() {
    this.api = new GameApiClient(); // Instantiate the live endpoint router channel
    this.initEventListeners();
    this.initFocusTrap();
    this.updateMenuMetadata();
  }

  private initEventListeners(): void {
    // 🎮 Start Game Trigger
    document.getElementById("btn-start")?.addEventListener("click", () => {
      this.startNewGameSession();
      this.switchView("view-game-scene");
    });

    // 🔄 Live Restart Game Session Trigger
    document.getElementById("btn-game-restart")?.addEventListener("click", async () => {
      this.cleanupGameplayListeners(); // 1. Clear current hardware key registers

      // 2. Clear out the old game ID token to force a clean server lifecycle state
      this.currentGameId = "";

      // 3. Await a fresh, fully functional C# backend session allocation
      await this.startNewGameSession();
    });


    // 💡 Live Hint Resource Trigger Listener
    document.getElementById("btn-game-hint")?.addEventListener("click", () => {
      this.handleHintUsage();
    });

    // 🛑 Stop Game Session Trigger
    document.getElementById("btn-game-stop")?.addEventListener("click", async () => {
      if (this.currentGameId) {
        try {
          const result = await this.api.stop(this.currentGameId);

          if (result.qualifiesForLeaderboard) {
            this.showOnScreenMessage(`🎉 Congratulations! Your final score of ${result.finalScore} qualified for the Top 10 Leaderboard!`, true);
          } else {
            this.showOnScreenMessage(`Game Stopped. Final Score: ${result.finalScore}.`, false);
          }

          // Hold the view on-screen for 3.5 seconds so the player can read it before routing back
          setTimeout(() => {
            this.cleanupGameplayListeners();
            this.updateMenuMetadata();
            this.switchView("view-main-menu");
          }, 3500);
          return;

        } catch (error) {
          this.showOnScreenMessage("Unable to stop the game. Please try again.");
          return;
        }
      }
      this.cleanupGameplayListeners();
      this.updateMenuMetadata();
      this.switchView("view-main-menu");
    });


    document.getElementById("btn-highscores")?.addEventListener("click", () => {
      this.fetchAndRenderHighScores();
      this.switchView("view-highscores");
    });

    document.getElementById("btn-settings")?.addEventListener("click", () => {
      this.loadSettingsForm();
      this.switchView("view-settings");
    });

    // 🛑 Stop Game Session Trigger
    document.getElementById("btn-game-stop")?.addEventListener("click", () => {
      this.cleanupGameplayListeners();
      this.updateMenuMetadata();
      this.switchView("view-main-menu");
    });

    // 🔄 Restart Game Session Trigger
    document.getElementById("btn-game-restart")?.addEventListener("click", () => {
      this.cleanupGameplayListeners(); // 1. Dismount current keystroke listeners to prevent duplication ghosting
      this.startNewGameSession();      // 2. Re-trigger a fresh baseline game state session loop instantly
    });

    document.getElementById("btn-settings-cancel")?.addEventListener("click", () => {
      this.clearSettingsError();
      this.updateMenuMetadata();
      this.switchView("view-main-menu");
    });

    document.getElementById("btn-settings-save")?.addEventListener("click", () => {
      if (this.saveSettingsForm()) {
        this.updateMenuMetadata();
        this.switchView("view-main-menu");
      }
    });

    document.querySelectorAll(".btn-back, #btn-highscores-back").forEach(button => {
      button.addEventListener("click", () => {
        this.updateMenuMetadata();
        this.switchView("view-main-menu");
      });
    });
  }
  private initFocusTrap(): void {
    window.addEventListener("keydown", (event: KeyboardEvent) => {
      if (event.key !== "Tab") return;

      const activePanel = document.querySelector(".view-panel.active");
      if (!activePanel) return;

      // Ensure keys or buttons inside the active panel hold focus boundaries safely
      const focusables = Array.from(activePanel.querySelectorAll("button, select, input, .key-btn")) as HTMLElement[];
      if (focusables.length === 0) return;

      const firstElement = focusables[0];
      const lastElement = focusables[focusables.length - 1];
      const activeElement = document.activeElement as HTMLElement;

      if (event.shiftKey) {
        if (activeElement.id === firstElement.id || activeElement === firstElement) {
          event.preventDefault();
          lastElement.focus();
        }
      } else {
        if (activeElement.id === lastElement.id || activeElement === lastElement) {
          event.preventDefault();
          firstElement.focus();
        }
      }
    });
  }

  public switchView(viewId: string): void {
    document.querySelectorAll(".view-panel").forEach(panel => {
      panel.classList.remove("active");
    });

    const targetPanel = document.getElementById(viewId);
    if (targetPanel) {
      targetPanel.classList.add("active");

      const firstFocusable = targetPanel.querySelector("button, select, input, .key-btn") as HTMLElement;
      if (firstFocusable) {
        firstFocusable.focus();
      }
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

    // 🎯 REPAIR HUD HOOKS: Synchronize sub-panel meta summaries cleanly
    const summaryString = `${prefs.language} / ${prefs.difficulty} / ${prefs.topic.replace(/-/g, " ")}`.toUpperCase();

    const gameSummary = document.getElementById("game-lbl-summary");
    const scoreSummary = document.getElementById("highscores-lbl-summary");

    if (gameSummary) gameSummary.textContent = summaryString;
    if (scoreSummary) scoreSummary.textContent = summaryString;
  }

  // 📝 Settings View Logic
  private loadSettingsForm(): void {
    const prefs = getGamePreferences();
    const selectLang = document.getElementById("set-language") as HTMLSelectElement;
    const selectDiff = document.getElementById("set-difficulty") as HTMLSelectElement;
    const selectTopic = document.getElementById("set-topic") as HTMLSelectElement;
    const inputName = document.getElementById("set-player-name") as HTMLInputElement;

    if (selectLang) selectLang.value = prefs.language;
    if (selectDiff) selectDiff.value = prefs.difficulty;
    if (selectTopic) selectTopic.value = prefs.topic;
    if (inputName) inputName.value = prefs.playerName;

    this.clearSettingsError();
  }

  private saveSettingsForm(): boolean {
    const selectLang = document.getElementById("set-language") as HTMLSelectElement;
    const selectDiff = document.getElementById("set-difficulty") as HTMLSelectElement;
    const selectTopic = document.getElementById("set-topic") as HTMLSelectElement;
    const inputName = document.getElementById("set-player-name") as HTMLInputElement;
    const errorDisplay = document.getElementById("settings-error-text");

    const trimmedName = inputName?.value.trim() || "";

    if (trimmedName.length === 0) {
      if (errorDisplay) errorDisplay.textContent = "Player name cannot be empty.";
      inputName?.focus();
      return false;
    }

    saveGamePreferences({
      language: selectLang.value as Language,
      difficulty: selectDiff.value as Difficulty,
      topic: selectTopic.value as Topic,
      playerName: trimmedName
    });

    savePlayerName(trimmedName);
    return true;
  }

  private clearSettingsError(): void {
    const errorDisplay = document.getElementById("settings-error-text");
    if (errorDisplay) errorDisplay.textContent = "";
  }

  // 🏆 Leaderboard Fetching Logic
  private async fetchAndRenderHighScores(): Promise<void> {
    const tableBody = document.getElementById("leaderboard-data-rows");
    if (!tableBody) return;

    tableBody.innerHTML = `<tr><td colspan="3" class="table-loading">Loading records from server...</td></tr>`;

    const prefs = getGamePreferences();
    const targetUrl = `http://localhost:5000/api/leaderboard/${prefs.language}/${prefs.difficulty}/${prefs.topic}`;

    try {
      const response = await fetch(targetUrl, { method: "GET" });
      if (!response.ok) throw new Error(`HTTP Error: ${response.status}`);

      const highscores: HighScoreRecord[] = await response.json();
      tableBody.innerHTML = "";

      if (highscores.length === 0) {
        tableBody.innerHTML = `<tr><td colspan="3" style="text-align:center; padding: 20px; color:#aaaaaa;">No highscores recorded for this category combination yet. Be the first!</td></tr>`;
        return;
      }

      highscores.forEach((record, index) => {
        const row = document.createElement("tr");
        row.innerHTML = `
          <td>#${index + 1}</td>
          <td>${this.escapeHtml(record.playerName)}</td>
          <td>${record.score}</td>
        `;
        tableBody.appendChild(row);
      });

    } catch {
      tableBody.innerHTML = `<tr><td colspan="3" class="table-error">⚠️ Error fetching leaderboards. Make sure Leaderboard.Api is running on port 5000.</td></tr>`;
    }
  }

  // 🎮 Core Gameplay Implementation - Live Service Integration
  private async startNewGameSession(): Promise<void> {
    this.guessedLetters.clear();

    // 🎯 STEP 2 FIXED: Hide the announcement banner at the start of every fresh game session
    this.hideOnScreenMessage();

    const prefs = getGamePreferences(); // Load your preferred configurations

    try {
      // 1. Call your live Game.Api backend to establish a fresh session row
      const response = await this.api.start(
        prefs.language,
        prefs.difficulty,
        prefs.topic,
        prefs.playerName || "PLAYER"
      );

      // 2. Safely capture the server-allocated GUID string token
      this.currentGameId = response.gameId || (response as any).GameId;

      // 3. Populate our gameplay state parameters using the backend payload
      this.applyBackendState(response);

      // 🎯 INITIALIZE TIMING SEQUENCE: Render layouts first to populate the DOM elements
      this.generateVirtualKeyboard();
      this.attachPhysicalKeyboardListener();

    } catch {
      const wordContainer = document.getElementById("game-word-blanks");
      if (wordContainer) {
        wordContainer.innerHTML = `<span style="color:#ff4444; font-size:18px;">⚠️ Game Server unreachable on port 5151.</span>`;
      }
    }
  }


  private applyBackendState(response: any): void {
    this.score = response.score ?? response.Score ?? 0;
    this.level = response.level ?? response.Level ?? 1;
    this.lives = response.lives ?? response.Lives ?? 0;
    this.livesPerLevel = response.livesPerLevel ?? response.LivesPerLevel ?? 11;

    // 🎯 FIXED: Support both camelCase and PascalCase JSON serialization models cleanly
    this.hints = response.hints ?? response.Hints ?? 0;
    this.hintCapacity = response.hintCapacity ?? response.HintCapacity ?? 3;

    this.repeatGuessPenalty = response.repeatGuessPenalty ?? response.RepeatGuessPenalty ?? false;

    if (response.revealed) {
      this.currentWordPattern = response.revealed.split("");
    } else if (response.Revealed) {
      this.currentWordPattern = response.Revealed.split("");
    }

    this.renderWordBlanks();
    this.renderGameplayHUD();
  }

  private async renderGameplayHUD(): Promise<void> {
    const lblScore = document.getElementById("game-score");
    const lblLevel = document.getElementById("game-level");
    const lblHighscore = document.getElementById("game-highscore");
    const containerLives = document.getElementById("game-lives-container");
    const containerHints = document.getElementById("game-hints-container");
    const tooltipRules = document.getElementById("level-rules-tooltip");

    if (lblScore) lblScore.textContent = this.score.toString();
    if (lblLevel) lblLevel.textContent = this.level.toString();

    // 🏆 LIVE HIGHSCORE HUD BINDING: Pull top entry from Leaderboard.Api asynchronously
    if (lblHighscore) {
      try {
        const prefs = getGamePreferences();
        const targetUrl = `http://localhost:5000/api/leaderboard/${prefs.language}/${prefs.difficulty}/${prefs.topic}`;
        const response = await fetch(targetUrl, { method: "GET" });

        if (response.ok) {
          const entries = await response.json();
          if (entries && entries.length > 0) {
            lblHighscore.textContent = `${entries[0].score} (${entries[0].playerName.toUpperCase()})`;
          } else {
            lblHighscore.textContent = "0";
          }
        }
      } catch (err) {
        lblHighscore.textContent = "0";
      }
    }

    if (containerLives) {
      containerLives.textContent = "❤️".repeat(this.lives) + "🖤".repeat(Math.max(0, this.livesPerLevel - this.lives));
    }

    if (containerHints) {
      containerHints.innerHTML = `
        Hint: ${"💡".repeat(this.hints)}<span style="opacity: 0.25;">${"💡".repeat(Math.max(0, this.hintCapacity - this.hints))}</span>
      `;
    }

    if (tooltipRules) {
      tooltipRules.innerHTML = `
        <strong>📋 LEVEL ${this.level} CONSTRAINTS:</strong><br>
        • Allowed Mistakes: ${this.livesPerLevel} Hearts<br>
        • Hint Resources: ${this.hintCapacity} Capacity<br>
        • Duplicate Penalty: ${this.repeatGuessPenalty ? "ACTIVE ⚠️" : "DISABLED"}
      `;
    }
  }


  private renderWordBlanks(): void {
    const wordContainer = document.getElementById("game-word-blanks");
    if (!wordContainer) return;

    wordContainer.innerHTML = "";
    this.currentWordPattern.forEach(char => {
      const charSpan = document.createElement("span");
      charSpan.className = "word-letter-blank";

      // 🎯 FIXED: Check for the backend hyphen mask character. 
      // If it's hidden, leave the inner text blank so only the clean white CSS bottom border displays!
      charSpan.textContent = (char === "-" || char === "_") ? "" : char;

      wordContainer.appendChild(charSpan);
    });
  }


  private generateVirtualKeyboard(): void {
    const keyboardContainer = document.getElementById("game-virtual-keyboard");
    if (!keyboardContainer) return;

    keyboardContainer.innerHTML = "";
    const alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".split("");

    alphabet.forEach(letter => {
      const button = document.createElement("button");
      button.type = "button";
      button.className = "key-btn";
      button.id = `key-${letter.toLowerCase()}`;
      button.textContent = letter;

      button.addEventListener("click", () => this.handleLetterGuess(letter.toLowerCase()));
      keyboardContainer.appendChild(button);
    });
  }

  private handlePhysicalKeyDown = (event: KeyboardEvent): void => {
    const key = event.key.toLowerCase();
    // Validate that the keystroke is a single character letter between standard boundaries
    if (/^[a-z]$/.test(key)) {
      this.handleLetterGuess(key);
    }
  };

  private attachPhysicalKeyboardListener(): void {
    window.addEventListener("keydown", this.handlePhysicalKeyDown);
  }

  private cleanupGameplayListeners(): void {
    window.removeEventListener("keydown", this.handlePhysicalKeyDown);
  }
  // 🎮 Live Letter Guess Integration
  private async handleLetterGuess(letter: string): Promise<void> {
    if (this.guessedLetters.has(letter) || this.lives <= 0) return;

    this.guessedLetters.add(letter);

    const virtualKey = document.getElementById(`key-${letter}`) as HTMLButtonElement;
    if (virtualKey) {
      virtualKey.disabled = true;
    }

    try {
      const response = await this.api.guess(this.currentGameId, letter);

      if (virtualKey) {
        const isCorrect = response.revealed.toUpperCase().includes(letter.toUpperCase());
        virtualKey.classList.add(isCorrect ? "key-correct" : "key-incorrect");
      }

      // 🎯 FIX: Apply the server state variables IMMEDIATELY so hint increments show on screen
      this.applyBackendState(response);

      if (response.roundComplete) {
        // Wait 2 seconds for the player to celebrate, then clear keyboard and fetch next word
        setTimeout(() => this.loadNextRound(), 2000);
        return;
      }

      if (response.gameOver) {
        const targetFullWord = response.fullWord || "";

        if (targetFullWord) {
          // Convert the text string into clean individual character strings
          this.currentWordPattern = targetFullWord.split("");

          // Force an immediate visual DOM refresh loop to render the green letters on screen
          this.renderWordBlanks();
        }

        // Display the on-screen announcement banner notification cleanly underneath
        this.showOnScreenMessage(`❌ Game Over! The word was: ${targetFullWord}.`, false);

        Array.from(document.querySelectorAll('.key-btn')).forEach(btn => (btn as HTMLButtonElement).disabled = true);
        this.cleanupGameplayListeners();

        setTimeout(() => {
          this.switchView("view-main-menu");
        }, 3500);
        return;
      }

    } catch {
      this.showOnScreenMessage("Unable to submit your guess. Please try again.");
    }
  }


  // 💡 Live Hint Resource Handler
  private async handleHintUsage(): Promise<void> {
    if (this.lives <= 0 || !this.currentGameId) return;

    try {
      // Submit hint request payload to C# Game.Api endpoint over port 5151
      const response = await this.api.hint(this.currentGameId);

      // Update our HUD elements and word display underlines instantly with the server state
      this.applyBackendState(response);

      // Handle structural boundary checks if the hint completed the word
      if ((response as any).roundComplete) {
        setTimeout(() => this.loadNextRound(), 2000);
      }
    } catch {
      this.showOnScreenMessage("Unable to use a hint. Please try again.");
    }
  }

  private async loadNextRound(): Promise<void> {
    try {
      const response = await this.api.nextRound(this.currentGameId);
      this.guessedLetters.clear();
      this.applyBackendState(response);
      this.generateVirtualKeyboard(); // Refresh with 26 clean grey keys
    } catch {
      this.showOnScreenMessage("Unable to load the next word. Please try again.");
    }
  }

  private showOnScreenMessage(message: string, isSuccess: boolean = false): void {
    const banner = document.getElementById("game-announcement-banner");
    if (!banner) return;

    banner.textContent = message;
    banner.style.display = "block";

    // Apply distinct color properties based on status rules
    banner.style.borderColor = isSuccess ? "#00ff00" : "#ff9900";
    banner.style.color = isSuccess ? "#00ff00" : "#ffff00";
  }

  private hideOnScreenMessage(): void {
    const banner = document.getElementById("game-announcement-banner");
    if (banner) {
      banner.style.display = "none";
    }
  }



  private escapeHtml(str: string): string {
    return str
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;")
      .replace(/'/g, "&#039;");
  }
}
window.addEventListener("DOMContentLoaded", () => {
  (window as any).appController = new AppViewController();
});
