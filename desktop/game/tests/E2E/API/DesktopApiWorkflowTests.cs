using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using System.Net.Http.Json;

namespace Desktop.Game.Tests;

public class DesktopApiWorkflowTests : DesktopTestBase
{
    [Fact]
    public async Task New_highscore_is_displayed_with_player_name_in_next_game()
    {
        using var httpClient = new HttpClient();
        const string playerName = "E2E_HNK";
        var isolatedSettingsPath = Path.Combine(Path.GetTempPath(), $"wordguess-desktop-e2e-{Guid.NewGuid():N}.json");
        Environment.SetEnvironmentVariable("WORD_GUESS_DESKTOP_SETTINGS_PATH", isolatedSettingsPath);
        Application? app = null;
        try
        {
            var submitResponse = await httpClient.PostAsJsonAsync(
                "http://localhost:5000/api/leaderboard/submit",
                new { playerName, score = 9999, language = "english", difficulty = "beginner", topic = "beverages" });
            Assert.True(submitResponse.IsSuccessStatusCode, "Leaderboard.Api must be running.");

            app = LaunchDesktopApp();
            using var automation = new UIA3Automation();
            var window = GetMainWindow(app, automation);
            WaitUntilWindowReady(window);

            window.FindFirstDescendant(cf => cf.ByAutomationId("btn-settings"))!.AsButton().Invoke();
            WaitForWindowTitle(window, "Settings");
            window.FindFirstDescendant(cf => cf.ByAutomationId("settings-language"))!.AsComboBox().Select("english");
            window.FindFirstDescendant(cf => cf.ByAutomationId("settings-difficulty"))!.AsComboBox().Select("beginner");
            window.FindFirstDescendant(cf => cf.ByAutomationId("settings-topic"))!.AsComboBox().Select("beverages");
            var playerNameTextBox = window.FindFirstDescendant(cf => cf.ByAutomationId("settings-player-name"))!.AsTextBox();
            playerNameTextBox.Patterns.Value.Pattern.SetValue(playerName);
            Assert.Equal(playerName, playerNameTextBox.Text);
            window.FindFirstDescendant(cf => cf.ByAutomationId("btn-settings-save"))!.AsButton().Invoke();
            WaitForWindowTitle(window, "Word Guessing - LearningPath");

            window.FindFirstDescendant(cf => cf.ByAutomationId("btn-start"))!.AsButton().Invoke();
            WaitForWindowTitle(window, "Guess the Word");
            var highscore = window.FindFirstDescendant(cf => cf.ByAutomationId("game-highscore"));
            Assert.NotNull(highscore);
            var deadline = DateTime.UtcNow.AddSeconds(10);
            while (DateTime.UtcNow < deadline && highscore!.Name != "Highscore: 9999 (E2E_HNK)") Thread.Sleep(100);
            Assert.Equal("Highscore: 9999 (E2E_HNK)", highscore!.Name);
        }
        finally
        {
            app?.Close();
            await httpClient.DeleteAsync($"http://localhost:5000/api/admin/purge-player?playerName={Uri.EscapeDataString(playerName)}");
            Environment.SetEnvironmentVariable("WORD_GUESS_DESKTOP_SETTINGS_PATH", null);
            if (File.Exists(isolatedSettingsPath)) File.Delete(isolatedSettingsPath);
        }
    }
    [Fact]
    public void Start_game_button_opens_game_page_when_game_api_is_running()
    {
        Application? app = null;
        try
        {
            app = LaunchDesktopApp();
            using var automation = new UIA3Automation();
            var window = GetMainWindow(app, automation);
            WaitUntilWindowReady(window);
            window.FindFirstDescendant(cf => cf.ByAutomationId("btn-start"))!.AsButton().Invoke();
            WaitForWindowTitle(window, "Guess the Word");
        }
        finally
        {
            app?.Close();
        }
    }

    [Fact]
    public void Highscores_button_opens_highscores_page_when_leaderboard_api_is_running()
    {
        Application? app = null;
        try
        {
            app = LaunchDesktopApp();
            using var automation = new UIA3Automation();
            var window = GetMainWindow(app, automation);
            WaitUntilWindowReady(window);
            window.FindFirstDescendant(cf => cf.ByAutomationId("btn-highscores"))!.AsButton().Invoke();
            WaitForWindowTitle(window, "Highscores");
            Assert.NotNull(window.FindFirstDescendant(cf => cf.ByAutomationId("btn-back")));
        }
        finally
        {
            app?.Close();
        }
    }
}
