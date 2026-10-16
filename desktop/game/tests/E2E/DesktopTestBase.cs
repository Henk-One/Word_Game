using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;

namespace Desktop.Game.Tests;

/// <summary>
/// Shared base class for all WPF desktop E2E tests using FlaUI (UIA3).
///
/// ⚠️ IMPORTANT — Avoiding Flaky Tests:
/// WPF uses hardware-accelerated rendering and its layout/render pass runs asynchronously
/// on the dispatcher thread. FlaUI may locate an element before WPF has finished measuring,
/// arranging, and painting it — causing clicks to miss or assertions to fail intermittently.
///
/// To prevent this:
///   1. Always call <see cref="WaitUntilWindowReady"/> after obtaining the main window.
///      This waits for the window to become interactive (not busy) before proceeding.
///   2. After triggering navigation (e.g. clicking a button that changes the page), call
///      <see cref="WaitForWindowTitle"/> to wait for the title to update — this confirms
///      the new page layout has been rendered.
///   3. When asserting on dynamically loaded content (e.g. highscores fetched from an API),
///      poll with a deadline instead of asserting immediately.
///
/// Always close the application in a finally block to prevent orphan windows in CI/CD.
/// </summary>
public abstract class DesktopTestBase
{
    /// <summary>
    /// Launches the WPF desktop application using the path from the
    /// <c>WORD_GUESS_DESKTOP_EXE</c> environment variable.
    /// </summary>
    protected static Application LaunchDesktopApp()
    {
        var executablePath = Environment.GetEnvironmentVariable("WORD_GUESS_DESKTOP_EXE");
        Assert.False(string.IsNullOrWhiteSpace(executablePath), "Set WORD_GUESS_DESKTOP_EXE to the built WPF executable before running desktop E2E tests.");
        Assert.True(File.Exists(executablePath), $"Desktop executable was not found: {executablePath}");
        return Application.Launch(executablePath);
    }

    /// <summary>
    /// Obtains the main window from the launched application, waiting up to 10 seconds
    /// for it to appear.
    /// </summary>
    protected static Window GetMainWindow(Application app, UIA3Automation automation)
    {
        var window = app.GetMainWindow(automation, TimeSpan.FromSeconds(10));
        Assert.NotNull(window);
        return window;
    }

    /// <summary>
    /// Waits until the WPF window is fully rendered and interactive.
    /// FlaUI can locate automation elements before the WPF dispatcher has finished its
    /// layout pass, which causes clicks on "stale" elements. This method polls the
    /// window's clickable state to let the render cycle complete.
    /// </summary>
    protected static void WaitUntilWindowReady(Window window)
    {
        window.WaitUntilClickable(TimeSpan.FromSeconds(10));
    }

    /// <summary>
    /// Polls the window title until it matches or a 10-second deadline expires.
    /// Use after triggering navigation to confirm the new page has been rendered.
    /// </summary>
    protected static void WaitForWindowTitle(Window window, string expectedTitle)
    {
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (DateTime.UtcNow < deadline && window.Title != expectedTitle) Thread.Sleep(100);
        Assert.Equal(expectedTitle, window.Title);
    }
}

