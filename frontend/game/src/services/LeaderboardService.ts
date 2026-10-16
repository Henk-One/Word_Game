import { GameApiClient } from "../api/GameApiClient";
import { SettingsApiClient } from "../api/SettingsApiClient";
import { Language, Difficulty, Topic, GamePreferences, getGamePreferences } from "../gameSettings";

export interface HighScoreRecord {
    rank: string;
    playerName: string;
    score: number;
    formattedDate: string;
}

// =========================================================================
// MODULE 1: DATA & TELEMETRY LAYER (High Cohesion & Separation of Concerns)
// Encompasses raw network reads, HTML shielding, and representation formatting.
// =========================================================================

export class LeaderboardDataService {
    /**
     * Reads highscore records from the server and transforms them into view-ready models.
     * Satisfies: "Everything concerning the screen should be separate from the database/network."
     */
    public async fetchCategoryLeaderboard(
        language: Language,
        difficulty: Difficulty,
        topic: Topic
    ): Promise<HighScoreRecord[]> {
        const targetUrl = `http://localhost:5000/api/leaderboard/${language}/${difficulty}/${topic}`;

        const response = await fetch(targetUrl, { method: "GET" });
        if (!response.ok) {
            throw new Error(`HTTP Error: ${response.status}`);
        }

        const rawRecords: Array<{ playerName: string; score: number; date: string }> = await response.json();

        // Maps raw server JSON payload data cleanly into filtered layout representations
        return rawRecords.map((record, index) => ({
            rank: `#${index + 1}`,
            playerName: this.escapeHtmlSecurityShield(record.playerName),
            score: record.score,
            formattedDate: this.formatHighScoreDate(record.date)
        }));
    }

    /**
     * Asynchronously fetches the top score entry value directly from Leaderboard.Api
     */
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
        } catch { /* Fail silently and fall back to safe structural default string value */ }
        return "0";
    }

    /**
     * Utility helper to secure input strings against injection vulnerabilities.
     * Kept inside data module to maintain high cohesion.
     */
    private escapeHtmlSecurityShield(str: string): string {
        return str
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }

    /**
     * Localizes highscore timestamp dates securely within data boundary zones.
     */
    private formatHighScoreDate(value: string): string {
        const date = new Date(value);
        if (Number.isNaN(date.getTime())) return "Unknown";

        return new Intl.DateTimeFormat(undefined, {
            dateStyle: "medium",
            timeStyle: "short"
        }).format(date);
    }
}
