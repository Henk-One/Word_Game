import { Language, Difficulty, Topic, GamePreferences } from "../gameSettings";
import { HighScoreRecord } from "../main"; // Haalt de interface rechtstreeks uit de hoofdmodule

export class AppDisplayEngine {
    /**
     * Updates only the visual leaderboard rows inside the HTML table.
     * Satisfies: "Everything concerning the screen should be separate from everything concerning the data."
     */
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
            row.innerHTML = `
        <td>${record.rank}</td>
        <td>${record.playerName}</td>
        <td>${record.score}</td>
        <td class="highscore-date">${record.formattedDate}</td>
      `;
            tableBody.appendChild(row);
        });
    }

    /**
     * Sets the visual loading or error state messages in the table DOM.
     */
    public renderLeaderboardStatusMessage(message: string, isError: boolean = false): void {
        const tableBody = document.getElementById("leaderboard-data-rows");
        if (!tableBody) return;

        const className = isError ? "table-error" : "table-loading";
        tableBody.innerHTML = `<tr><td colspan="4" class="${className}">${message}</td></tr>`;
    }

    /**
     * Syncs the settings form fields to match the active preference data models.
     */
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

    /**
     * Renders the game over word underscores or letters onto the screen.
     */
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

    /**
     * Shows or updates the floating banner notification element cleanly.
     */
    public showOnScreenMessage(message: string, isSuccess: boolean = false): void {
        const banner = document.getElementById("game-announcement-banner");
        if (!banner) return;

        banner.textContent = message;
        banner.style.display = "block";
        banner.style.borderColor = isSuccess ? "#00ff00" : "#ff9900";
        banner.style.color = isSuccess ? "#00ff00" : "#ffff00";
    }

    /**
     * Hides the game announcement banner from view.
     */
    public hideOnScreenMessage(): void {
        const banner = document.getElementById("game-announcement-banner");
        if (banner) banner.style.display = "none";
    }
}
