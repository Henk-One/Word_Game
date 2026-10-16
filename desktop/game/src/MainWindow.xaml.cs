using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using Desktop.Game.Api;
using Desktop.Game.Models;
using Desktop.Game.Services;

namespace Desktop.Game;

public partial class MainWindow : Window
{
    private readonly GameApiClient gameApiClient;
    private readonly SettingsApiClient settingsApiClient;

    // Decoupled enterprise architecture services
    private readonly DesktopLeaderboardService _leaderboardService;
    private readonly WpfDisplayEngine _displayEngine;

    private static readonly string SettingsPath = Environment.GetEnvironmentVariable("WORD_GUESS_DESKTOP_SETTINGS_PATH")
        ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LearningPath", "desktop-settings.json");

    private readonly string[] highscoreLanguages = ["english", "dutch", "indonesian"];
    private readonly string[] highscoreDifficulties = ["beginner", "intermediate"];
    private readonly string[] highscoreTopics = ["beverages", "car-parts"];

    private int highscoreIndex = 1;
    private string? currentGameId;
    private TextBlock? gameStatus;
    private TextBlock? gameWord;
    private TextBlock? gameScore;
    private TextBlock? gameHighscore;
    private TextBlock? gameLives;
    private TextBlock? gameHints;
    private TextBlock? gameRules;
    private Button? gameHintButton;
    private TextBlock? gameLevel;
    private WrapPanel? gameKeyboard;

    public MainWindow()
    {
        InitializeComponent();
        var httpClient = new HttpClient();
        gameApiClient = new GameApiClient(httpClient);
        settingsApiClient = new SettingsApiClient(httpClient);

        // Instantiate the decoupled services
        var leaderboardApiClient = new LeaderboardApiClient(httpClient);
        _leaderboardService = new DesktopLeaderboardService(leaderboardApiClient);
        _displayEngine = new WpfDisplayEngine(RootGrid, PageTitle, MenuMetadata, MainMenuView);
    }

    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var preferences = LoadPreferences();
            var game = await gameApiClient.StartAsync(new(preferences.Language, preferences.Difficulty, preferences.Topic, preferences.PlayerName));
            currentGameId = game.GameId;
            _displayEngine.ShowPage("Guess the Word", BuildGamePage(game));
        }
        catch (Exception ex)
        {
            _displayEngine.ShowPage("Game unavailable", BuildMessagePage($"Could not start the game: {ex.Message}"));
        }
    }

    private async void HighscoresButton_Click(object sender, RoutedEventArgs e)
    {
        var preferences = LoadPreferences();
        highscoreIndex = _leaderboardService.ComputeHighscoreIndex(preferences.Language, preferences.Difficulty, preferences.Topic);
        await ShowHighscoresPage();
    }

    private async void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var preferences = LoadPreferences();
        try
        {
            var serverSettings = await settingsApiClient.GetAsync(preferences.PlayerName);
            preferences = preferences with { Language = serverSettings.Language, Difficulty = serverSettings.Difficulty, Topic = serverSettings.Topic };
            SavePreferences(preferences);
        }
        catch { /* Keep the locally cached settings when the API is unavailable. */ }

        var content = new StackPanel { Width = 360 };
        content.Children.Add(new TextBlock { Text = "Language", Foreground = System.Windows.Media.Brushes.White });

        var language = new ComboBox { ItemsSource = highscoreLanguages, SelectedItem = preferences.Language, Margin = new Thickness(0, 6, 0, 12) };
        AutomationProperties.SetAutomationId(language, "settings-language");
        content.Children.Add(language);

        content.Children.Add(new TextBlock { Text = "Difficulty", Foreground = System.Windows.Media.Brushes.White });
        var difficulty = new ComboBox { ItemsSource = highscoreDifficulties, SelectedItem = preferences.Difficulty, Margin = new Thickness(0, 6, 0, 12) };
        AutomationProperties.SetAutomationId(difficulty, "settings-difficulty");
        content.Children.Add(difficulty);

        content.Children.Add(new TextBlock { Text = "Topic", Foreground = System.Windows.Media.Brushes.White });
        var topic = new ComboBox { ItemsSource = highscoreTopics, SelectedItem = preferences.Topic, Margin = new Thickness(0, 6, 0, 20) };
        AutomationProperties.SetAutomationId(topic, "settings-topic");
        content.Children.Add(topic);

        content.Children.Add(new TextBlock { Text = "Player name", Foreground = System.Windows.Media.Brushes.White });
        var playerName = new TextBox { Text = preferences.PlayerName, MaxLength = 8, Margin = new Thickness(0, 6, 0, 20) };
        AutomationProperties.SetAutomationId(playerName, "settings-player-name");
        content.Children.Add(playerName);

        var save = new Button { Content = "SAVE", Width = 180, Padding = new Thickness(10), HorizontalAlignment = HorizontalAlignment.Center };
        AutomationProperties.SetAutomationId(save, "btn-settings-save");
        save.Click += async (_, _) =>
        {
            var updated = new DesktopPreferences(
                language.SelectedItem?.ToString() ?? "english",
                difficulty.SelectedItem?.ToString() ?? "beginner",
                topic.SelectedItem?.ToString() ?? "car-parts",
                playerName.Text.Trim());
            SavePreferences(updated);
            try
            {
                var saved = await settingsApiClient.SaveAsync(updated.PlayerName, new(updated.Language, updated.Difficulty, updated.Topic));
                SavePreferences(updated with { Language = saved.Language, Difficulty = saved.Difficulty, Topic = saved.Topic });
            }
            catch { /* Local persistence remains available if the API is offline. */ }
            _displayEngine.ShowMainMenu();
        };
        content.Children.Add(save);
        content.Children.Add(CreateBackButton());

        _displayEngine.ShowPage("Settings", content);
    }

    private FrameworkElement BuildGamePage(StartGameResponse? game)
    {
        var content = new StackPanel { Width = 680 };
        var preferences = LoadPreferences();
        content.Children.Add(new TextBlock
        {
            Text = $"Mode: {preferences.Language.ToUpperInvariant()} / {preferences.Difficulty.ToUpperInvariant()} / {preferences.Topic.ToUpperInvariant()}",
            Foreground = System.Windows.Media.Brushes.LightGray,
            FontSize = 14,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, -10, 0, 15)
        });
        gameStatus = new TextBlock { Foreground = System.Windows.Media.Brushes.LightGray, FontSize = 16, Text = "Guess a letter" };
        gameWord = new TextBlock { Foreground = System.Windows.Media.Brushes.Lime, FontSize = 34, FontFamily = new System.Windows.Media.FontFamily("Consolas"), HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 20, 0, 20) };
        gameScore = new TextBlock { Foreground = System.Windows.Media.Brushes.White, FontSize = 16 };
        gameHighscore = new TextBlock { Foreground = System.Windows.Media.Brushes.White, FontSize = 16, HorizontalAlignment = HorizontalAlignment.Right };
        AutomationProperties.SetAutomationId(gameHighscore, "game-highscore");
        gameLives = new TextBlock { Foreground = System.Windows.Media.Brushes.IndianRed, FontSize = 16 };
        gameHints = new TextBlock { Foreground = System.Windows.Media.Brushes.Gold, FontSize = 16 };
        gameRules = new TextBlock { Foreground = System.Windows.Media.Brushes.LightGray, FontSize = 13, TextWrapping = TextWrapping.Wrap, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 8, 0, 8) };
        gameLevel = new TextBlock { Foreground = System.Windows.Media.Brushes.White, FontSize = 16 };
        var stats = new Grid { Margin = new Thickness(0, 8, 0, 0) };
        stats.ColumnDefinitions.Add(new ColumnDefinition());
        stats.ColumnDefinitions.Add(new ColumnDefinition());
        stats.ColumnDefinitions.Add(new ColumnDefinition());
        stats.RowDefinitions.Add(new RowDefinition());
        stats.RowDefinitions.Add(new RowDefinition());
        Grid.SetColumn(gameScore, 0);
        Grid.SetColumn(gameHighscore, 2);
        Grid.SetRow(gameLives, 1);
        Grid.SetColumn(gameLives, 0);
        Grid.SetRow(gameLevel, 1);
        Grid.SetColumn(gameLevel, 1);
        gameLevel.HorizontalAlignment = HorizontalAlignment.Center;
        Grid.SetRow(gameHints, 1);
        Grid.SetColumn(gameHints, 2);
        gameHints.HorizontalAlignment = HorizontalAlignment.Right;
        stats.Children.Add(gameScore);
        stats.Children.Add(gameHighscore);
        stats.Children.Add(gameLives);
        stats.Children.Add(gameLevel);
        stats.Children.Add(gameHints);
        content.Children.Add(stats);
        content.Children.Add(gameRules);
        content.Children.Add(gameStatus);
        content.Children.Add(gameWord);

        gameKeyboard = new WrapPanel { HorizontalAlignment = HorizontalAlignment.Center, Width = 620, Margin = new Thickness(0, 20, 0, 15) };
        foreach (var letter in "ABCDEFGHIJKLMNOPQRSTUVWXYZ")
        {
            var button = new Button { Content = letter.ToString(), Width = 58, Height = 42, Margin = new Thickness(3) };
            button.Click += async (_, _) => await SubmitGuess(letter, button);
            gameKeyboard.Children.Add(button);
        }
        content.Children.Add(gameKeyboard);

        var actions = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
        var hint = new Button { Content = "HINT", Width = 110, Margin = new Thickness(4), Foreground = System.Windows.Media.Brushes.Gold, BorderBrush = System.Windows.Media.Brushes.Gold };
        gameHintButton = hint;
        hint.Click += async (_, _) => await UseHint(hint);
        var restart = new Button { Content = "RESTART", Width = 110, Margin = new Thickness(4), Foreground = System.Windows.Media.Brushes.LimeGreen, BorderBrush = System.Windows.Media.Brushes.LimeGreen };
        restart.Click += async (_, _) => await RestartGame();
        var stop = new Button { Content = "STOP", Width = 110, Margin = new Thickness(4), Foreground = System.Windows.Media.Brushes.IndianRed, BorderBrush = System.Windows.Media.Brushes.IndianRed };
        stop.Click += async (_, _) => await StopGame();
        actions.Children.Add(hint);
        actions.Children.Add(restart);
        actions.Children.Add(stop);
        content.Children.Add(actions);
        RenderGameState(game);
        return content;
    }

    private async Task SubmitGuess(char letter, Button button)
    {
        if (string.IsNullOrWhiteSpace(currentGameId)) return;
        try
        {
            var previousReveal = gameWord?.Text ?? "";
            var state = await gameApiClient.GuessAsync(new(currentGameId, letter.ToString()));

            // Calculate the status directly from the backend response.
            bool isCorrect = state is not null && state.Revealed != previousReveal;

            // Keep the button fully clickable for the Level 2 repeat penalty.
            // Change only the styling to make the contrast clear.
            if (isCorrect)
            {
                // Give a correct letter a vivid green background with white text.
                button.Background = System.Windows.Media.Brushes.DarkGreen;
                button.Foreground = System.Windows.Media.Brushes.White;
                button.BorderBrush = System.Windows.Media.Brushes.Lime;
            }
            else
            {
                // Give an incorrect letter a dark red background with muted gray text.
                button.Background = System.Windows.Media.Brushes.DarkRed;
                button.Foreground = System.Windows.Media.Brushes.LightGray;
                button.BorderBrush = System.Windows.Media.Brushes.Red;
            }

            // Preserve the letter exactly as requested; do not replace it with a check mark or X.
            button.Content = letter.ToString();

            RenderGameState(state);

            if (state?.RoundComplete == true && state.GameOver == false)
            {
                if (gameStatus is not null) gameStatus.Text = "Word solved! Starting the next round...";
                await Task.Delay(1200);
                await LoadNextRound();
            }
        }
        catch (Exception ex) { if (gameStatus is not null) gameStatus.Text = $"Guess failed: {ex.Message}"; }
    }

    private async Task LoadNextRound()
    {
        if (string.IsNullOrWhiteSpace(currentGameId)) return;
        try
        {
            var state = await gameApiClient.NextRoundAsync(currentGameId);
            RenderGameState(state);
            if (gameKeyboard is not null)
            {
                foreach (var child in gameKeyboard.Children.OfType<Button>())
                {
                    // Restore the buttons to their initial state for the new round.
                    child.IsEnabled = true;
                    child.IsHitTestVisible = true;
                    child.ClearValue(Control.ForegroundProperty);
                    child.ClearValue(Control.BackgroundProperty);
                    child.ClearValue(Control.BorderBrushProperty);
                }
            }
            if (gameStatus is not null) gameStatus.Text = "Guess a letter";
        }
        catch (Exception ex) { if (gameStatus is not null) gameStatus.Text = $"Next round failed: {ex.Message}"; }
    }



    private async Task UseHint(Button button)
    {
        if (string.IsNullOrWhiteSpace(currentGameId)) return;
        try
        {
            var state = await gameApiClient.HintAsync(currentGameId);
            RenderGameState(state);

            // If no hints remain, use dark gray instead of gold to maintain readable contrast.
            if (state?.Hints <= 0)
            {
                button.IsEnabled = false;
                button.Foreground = System.Windows.Media.Brushes.DimGray;
                button.BorderBrush = System.Windows.Media.Brushes.DimGray;
            }
        }
        catch (Exception ex) { if (gameStatus is not null) gameStatus.Text = $"Hint failed: {ex.Message}"; }
    }




    private async Task RestartGame()
    {
        try
        {
            var preferences = LoadPreferences();
            var game = await gameApiClient.StartAsync(new(preferences.Language, preferences.Difficulty, preferences.Topic, preferences.PlayerName));
            currentGameId = game.GameId;
            _displayEngine.ShowPage("Guess the Word", BuildGamePage(game));
        }
        catch (Exception ex) { if (gameStatus is not null) gameStatus.Text = $"Restart failed: {ex.Message}"; }
    }

    private async Task StopGame()
    {
        if (!string.IsNullOrWhiteSpace(currentGameId))
        {
            try { await gameApiClient.StopAsync(currentGameId); } catch { }
        }
        currentGameId = null;
        _displayEngine.ShowMainMenu();
    }
    private void RenderGameState(StartGameResponse? state)
    {
        if (state is null) return;
        if (gameWord is not null) gameWord.Text = state.Revealed;
        if (gameScore is not null) gameScore.Text = $"Score: {state.Score}";
        if (gameHighscore is not null) _ = LoadHighscore();
        if (gameLevel is not null) gameLevel.Text = $"Level: {state.Level}";
        // Update the hint button in each RenderGameState overload.
        if (gameHintButton is not null)
        {
            bool hasHints = state.Hints > 0 && state.Lives > 0;
            gameHintButton.IsEnabled = hasHints;

            if (hasHints)
            {
                // Restore the original gold/yellow styling on a dark background.
                gameHintButton.Foreground = System.Windows.Media.Brushes.Gold;
                gameHintButton.BorderBrush = System.Windows.Media.Brushes.Gold;
            }
            else
            {
                // Ensure high contrast against the white Windows disabled-button background.
                gameHintButton.Foreground = System.Windows.Media.Brushes.DimGray;
                gameHintButton.BorderBrush = System.Windows.Media.Brushes.DimGray;
            }
        }

        if (gameRules is not null) gameRules.Text = BuildLevelRules(state.Level, state.LivesPerLevel, state.HintCapacity, state.RepeatGuessPenalty);

        if (gameLives is not null)
        {
            gameLives.FontSize = 18;
            // Pass the sixth parameter, "♡", correctly.
            RenderIconTrack(gameLives, "Lives: ", "♥", state.Lives, state.LivesPerLevel, "♡");
        }
        if (gameHints is not null)
        {
            gameHints.FontSize = 18;
            // Pass the sixth parameter, "○", correctly.
            RenderIconTrack(gameHints, "Hints: ", "✦", state.Hints, state.HintCapacity, "○");
        }
    }

    private void RenderGameState(GuessResponse? state)
    {
        if (state is null) return;
        if (gameWord is not null) gameWord.Text = state.Revealed;
        if (gameScore is not null) gameScore.Text = $"Score: {state.Score}";
        if (gameLevel is not null) gameLevel.Text = $"Level: {state.Level}";
        // Update the hint button in each RenderGameState overload.
        if (gameHintButton is not null)
        {
            bool hasHints = state.Hints > 0 && state.Lives > 0;
            gameHintButton.IsEnabled = hasHints;

            if (hasHints)
            {
                // Restore the original gold/yellow styling on a dark background.
                gameHintButton.Foreground = System.Windows.Media.Brushes.Gold;
                gameHintButton.BorderBrush = System.Windows.Media.Brushes.Gold;
            }
            else
            {
                // Ensure high contrast against the white Windows disabled-button background.
                gameHintButton.Foreground = System.Windows.Media.Brushes.DimGray;
                gameHintButton.BorderBrush = System.Windows.Media.Brushes.DimGray;
            }
        }

        if (gameRules is not null) gameRules.Text = BuildLevelRules(state.Level, state.LivesPerLevel, state.HintCapacity, state.RepeatGuessPenalty);
        if (gameStatus is not null && state.GameOver) gameStatus.Text = $"Game over. The word was {state.FullWord}.";

        if (gameLives is not null)
        {
            gameLives.FontSize = 18;
            // Pass the sixth parameter, "♡", correctly.
            RenderIconTrack(gameLives, "Lives: ", "♥", state.Lives, state.LivesPerLevel, "♡");
        }
        if (gameHints is not null)
        {
            gameHints.FontSize = 18;
            // Pass the sixth parameter, "○", correctly.
            RenderIconTrack(gameHints, "Hints: ", "✦", state.Hints, state.HintCapacity, "○");
        }
    }
    private void RenderGameState(HintResponse? state)
    {
        if (state is null) return;
        if (gameWord is not null) gameWord.Text = state.Revealed;
        if (gameScore is not null) gameScore.Text = $"Score: {state.Score}";
        if (gameLevel is not null) gameLevel.Text = $"Level: {state.Level}";
        // Update the hint button in each RenderGameState overload.
        if (gameHintButton is not null)
        {
            bool hasHints = state.Hints > 0 && state.Lives > 0;
            gameHintButton.IsEnabled = hasHints;

            if (hasHints)
            {
                // Restore the original gold/yellow styling on a dark background.
                gameHintButton.Foreground = System.Windows.Media.Brushes.Gold;
                gameHintButton.BorderBrush = System.Windows.Media.Brushes.Gold;
            }
            else
            {
                // Ensure high contrast against the white Windows disabled-button background.
                gameHintButton.Foreground = System.Windows.Media.Brushes.DimGray;
                gameHintButton.BorderBrush = System.Windows.Media.Brushes.DimGray;
            }
        }

        if (gameRules is not null) gameRules.Text = BuildLevelRules(state.Level, 11, state.HintCapacity, false);

        if (gameLives is not null)
        {
            gameLives.FontSize = 18;
            // Pass the sixth parameter, "♡", correctly.
            RenderIconTrack(gameLives, "Lives: ", "♥", state.Lives, 11, "♡");
        }
        if (gameHints is not null)
        {
            gameHints.FontSize = 18;
            // Pass the sixth parameter, "○", correctly.
            RenderIconTrack(gameHints, "Hints: ", "✦", state.Hints, state.HintCapacity, "○");
        }
    }

    private static string BuildLevelRules(int level, int livesPerLevel, int hintCapacity, bool repeatPenalty)
    {
        var repeatText = repeatPenalty ? "repeated guesses lose a life" : "repeated guesses are free";
        return $"Level {level} rules: {livesPerLevel} lives  up to {hintCapacity} hints  {repeatText}";
    }

    private static void RenderIconTrack(TextBlock targetBlock, string prefix, string icon, int enabled, int capacity, string inactiveIcon)
    {
        if (targetBlock == null) return;
        targetBlock.Inlines.Clear();
        targetBlock.Inlines.Add(new System.Windows.Documents.Run(prefix));

        enabled = Math.Max(0, enabled);
        capacity = Math.Max(enabled, capacity);

        // 1. Render active/available icons in fully opaque colors.
        if (enabled > 0)
        {
            var activeRun = new System.Windows.Documents.Run(string.Concat(Enumerable.Repeat(icon, enabled)));

            // Use bright red for lives and bright gold/yellow for hints.
            activeRun.Foreground = prefix.StartsWith("Lives")
                ? System.Windows.Media.Brushes.Crimson
                : System.Windows.Media.Brushes.Gold;

            targetBlock.Inlines.Add(activeRun);
        }

        // 2. Render inactive/lost capacity with a dim, translucent appearance.
        int disabledCount = capacity - enabled;
        if (disabledCount > 0)
        {
            var disabledRun = new System.Windows.Documents.Run(string.Concat(Enumerable.Repeat(inactiveIcon, disabledCount)))
            {
                // Use a subtle gray with low opacity (alpha 60 of 255).
                Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(60, 128, 128, 128))
            };
            targetBlock.Inlines.Add(disabledRun);
        }
    }



    private async Task LoadHighscore()
    {
        if (gameHighscore is null) return;
        try
        {
            var preferences = LoadPreferences();
            var scores = await _leaderboardService.GetCleanLeaderboardAsync(preferences.Language, preferences.Difficulty, preferences.Topic);
            gameHighscore.Text = scores.Count == 0
                ? "Highscore: 0"
                : $"Highscore: {scores[0].Score} ({scores[0].PlayerName.ToUpperInvariant()})";
        }
        catch { gameHighscore.Text = "Highscore: unavailable"; }
    }

    private async Task ShowHighscoresPage()
    {
        var modeText = _leaderboardService.GetMetadataString(highscoreIndex);
        var totalCategories = _leaderboardService.GetTotalCategoryCount();

        List<ViewHighScoreRecord> cleanScores;
        try
        {
            var targetCategory = modeText.Replace("Mode: ", "").Split('/');
            var lang = targetCategory[0].Trim().ToLowerInvariant();
            var diff = targetCategory[1].Trim().ToLowerInvariant();
            var topic = targetCategory[2].Trim().ToLowerInvariant().Replace(" ", "-");

            cleanScores = await _leaderboardService.GetCleanLeaderboardAsync(lang, diff, topic);
        }
        catch (Exception ex)
        {
            cleanScores = [new ViewHighScoreRecord("ERROR", "SYSTEM", 0, ex.Message)];
        }

        var viewPanel = _displayEngine.BuildHighscoresPage(
            cleanScores,
            modeText,
            canGoBack: highscoreIndex > 0,
            canGoNext: highscoreIndex < totalCategories - 1,
            backClick: async (_, _) => { highscoreIndex--; await ShowHighscoresPage(); },
            nextClick: async (_, _) => { highscoreIndex++; await ShowHighscoresPage(); },
            exitClick: (_, _) => _displayEngine.ShowMainMenu()
        );

        _displayEngine.ShowPage("Highscores", viewPanel);
    }

    private static DesktopPreferences LoadPreferences()
    {
        try
        {
            if (File.Exists(SettingsPath))
                return JsonSerializer.Deserialize<DesktopPreferences>(File.ReadAllText(SettingsPath)) ?? new DesktopPreferences();
        }
        catch { }
        return new DesktopPreferences();
    }

    private static void SavePreferences(DesktopPreferences preferences)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
        File.WriteAllText(SettingsPath, JsonSerializer.Serialize(preferences, new JsonSerializerOptions { WriteIndented = true }));
    }

    private StackPanel BuildMessagePage(string message)
    {
        var content = new StackPanel { Width = 560 };
        content.Children.Add(new TextBlock { Text = message, Foreground = System.Windows.Media.Brushes.White, FontSize = 18, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 20) });
        content.Children.Add(CreateBackButton());
        return content;
    }

    private Button CreateBackButton(string caption = "BACK")
    {
        var button = new Button { Content = caption, Padding = new Thickness(10), Width = 180, HorizontalAlignment = HorizontalAlignment.Center };
        AutomationProperties.SetAutomationId(button, "btn-back");
        button.Click += (_, _) => _displayEngine.ShowMainMenu();
        return button;
    }

    private sealed record DesktopPreferences(string Language = "english", string Difficulty = "beginner", string Topic = "car-parts", string PlayerName = "PLAYER");
}
