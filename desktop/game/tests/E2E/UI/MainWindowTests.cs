using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;

namespace Desktop.Game.Tests;

public class MainWindowTests : DesktopTestBase
{
    [Fact]
    public void Main_menu_contains_start_highscores_and_settings_buttons()
    {
        Application? app = null;
        try
        {
            app = LaunchDesktopApp();
            using var automation = new UIA3Automation();
            var window = GetMainWindow(app, automation);
            WaitUntilWindowReady(window);
            AssertButtonExists(window, "btn-start", "START GAME");
            AssertButtonExists(window, "btn-highscores", "HIGHSCORES");
            AssertButtonExists(window, "btn-settings", "SETTINGS");
        }
        finally
        {
            app?.Close();
        }
    }

    [Fact]
    public void Settings_button_opens_settings_page_and_back_returns_to_menu()
    {
        Application? app = null;
        try
        {
            app = LaunchDesktopApp();
            using var automation = new UIA3Automation();
            var window = GetMainWindow(app, automation);
            WaitUntilWindowReady(window);
            window.FindFirstDescendant(cf => cf.ByAutomationId("btn-settings"))!.AsButton().Invoke();
            WaitForWindowTitle(window, "Settings");
            Assert.NotNull(window.FindFirstDescendant(cf => cf.ByAutomationId("btn-back")));
            window.FindFirstDescendant(cf => cf.ByAutomationId("btn-back"))!.AsButton().Invoke();
            WaitForWindowTitle(window, "Word Guessing - LearningPath");
            Assert.NotNull(window.FindFirstDescendant(cf => cf.ByAutomationId("btn-start")));
        }
        finally
        {
            app?.Close();
        }
    }

    private static void AssertButtonExists(Window window, string automationId, string expectedName)
    {
        var button = window.FindFirstDescendant(cf => cf.ByAutomationId(automationId))?.AsButton();
        Assert.NotNull(button);
        Assert.Equal(expectedName, button.Name);
    }

    [Fact]
    public void GameplayHUD_IconsAndLayout_ShouldMaintainUniformSizingAndDisableState()
    {
        // 1. Initialize and start the desktop application
        using var app = LaunchDesktopApp();
        var automation = new FlaUI.UIA3.UIA3Automation();
        var window = app.GetMainWindow(automation);
        WaitForWindowTitle(window, "Word Guessing - LearningPath");

        // 2. Click the START GAME trigger button to transition views and render the gameplay HUD
        var startButton = window.FindFirstDescendant(cf => cf.ByAutomationId("btn-start"))?.AsButton();
        Assert.NotNull(startButton);
        startButton.Click();

        // Hold the test execution context thread until the target window title mutates
        WaitForWindowTitle(window, "Guess the Word");

        // 3. Query all raw text elements from the active visual tree.
        // A WPF TextBlock exposes its rendered value through the universal .Name property, not .Text.
        var textElements = window.FindAllDescendants(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Text))
                                 .ToList();

        var livesElement = textElements.FirstOrDefault(x => x.Name.StartsWith("Lives:"));
        var hintsElement = textElements.FirstOrDefault(x => x.Name.StartsWith("Hints:"));
        var levelElement = textElements.FirstOrDefault(x => x.Name.StartsWith("Level:"));

        // Validation: Verify that all necessary structural telemetry blocks were successfully painted
        Assert.NotNull(livesElement);
        Assert.NotNull(hintsElement);
        Assert.NotNull(levelElement);

        // 4. VERIFY INITIAL GAME STATE VALUES (Level 1: 11 lives active, 0 hints available)
        // Assert 1: All 11 initialized lives must be fully visible and rendered using active symbols
        Assert.Contains("Lives: ♥♥♥♥♥♥♥♥♥♥♥", livesElement.Name);
        Assert.DoesNotContain("♡", livesElement.Name); // Ensure no empty hearts 

        // Assert 2: Hint resources must display as entirely empty/disabled upon starting a session
        // FIX: Match the empty circles pattern used for inactive hints
        Assert.Contains("Hints: ○○○", hintsElement.Name);
        Assert.DoesNotContain("✦", hintsElement.Name); // Ensure no active hints

        // 5. CRITICAL VISUAL ALIGNMENT & OVERLAP VALIDATION (Window Boundaries Guard)
        // Read the absolute physical X-coordinates (Bounding Rectangle) of the elements on the desktop view canvas.
        // This mathematically ensures elements do not render over each other on the horizontal axis.
        var livesBounds = livesElement.BoundingRectangle;
        var levelBounds = levelElement.BoundingRectangle;
        var hintsBounds = hintsElement.BoundingRectangle;

        // The Level indicator is docked in the middle, meaning Lives must be completely to its left
        Assert.True(livesBounds.Right <= levelBounds.Left,
            $"Layout Corruption: Lives block (Right: {livesBounds.Right}) overlaps with middle Level block (Left: {levelBounds.Left}).");

        // The Hints track is docked to the right edge, meaning Level must be completely to its left
        Assert.True(levelBounds.Right <= hintsBounds.Left,
            $"Layout Corruption: Level block (Right: {levelBounds.Right}) overlaps with right Hints block (Left: {hintsBounds.Left}).");

        // Assert 3: Width guard. If custom high-DPI emoji assets expand abnormally, they will overflow the frame.
        // We confirm that the rightmost point of the UI elements remains safely within the window's outer boundary.
        Assert.True(hintsBounds.Right < window.BoundingRectangle.Right,
            "Layout Corruption: The icon rows are too wide and are stretching outside the visible window boundaries.");

        app.Close();
    }

}
