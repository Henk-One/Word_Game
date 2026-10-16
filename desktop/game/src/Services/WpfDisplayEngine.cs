using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Desktop.Game.Services;

// =========================================================================
// MODULE 2: PURE WPF DISPLAY ENGINE (Separation of Concerns) - DEEL 1
// Pure visual layouts and UI mutations. Strictly NO API clients or network calls.
// =========================================================================
public class WpfDisplayEngine
{
    private readonly Grid _rootGrid;
    private readonly TextBlock _pageTitle;
    private readonly Border _menuMetadata;
    private readonly StackPanel _mainMenuView;

    public WpfDisplayEngine(Grid rootGrid, TextBlock pageTitle, Border menuMetadata, StackPanel mainMenuView)
    {
        _rootGrid = rootGrid ?? throw new ArgumentNullException(nameof(rootGrid));
        _pageTitle = pageTitle ?? throw new ArgumentNullException(nameof(pageTitle));
        _menuMetadata = menuMetadata ?? throw new ArgumentNullException(nameof(menuMetadata));
        _mainMenuView = mainMenuView ?? throw new ArgumentNullException(nameof(mainMenuView));
    }

    /// <summary>
    /// Swaps the active view panel cleanly and updates window titles.
    /// </summary>
    public void ShowPage(string title, FrameworkElement content)
    {
        _pageTitle.Text = title;
        _pageTitle.FontSize = title == "Highscores" ? 30 : 36;
        _menuMetadata.Visibility = Visibility.Collapsed;
        _mainMenuView.Visibility = Visibility.Collapsed;

        // FIX: Werk de daadwerkelijke WPF Window titel bij zodat FlaUI tests slagen
        var parentWindow = Window.GetWindow(_rootGrid);
        if (parentWindow != null)
        {
            parentWindow.Title = title;
        }

        // Clean out legacy contextual pages from layout thread memory
        foreach (var child in _rootGrid.Children.OfType<FrameworkElement>()
            .Where(child => child != _pageTitle && child != _menuMetadata && child != _mainMenuView).ToList())
        {
            _rootGrid.Children.Remove(child);
        }

        Grid.SetRow(content, 1);
        _rootGrid.Children.Add(content);
    }

    /// <summary>
    /// Restores view states back to original main entry parameters.
    /// </summary>
    public void ShowMainMenu()
    {
        const string mainMenuTitle = "Word Guessing - LearningPath";
        _pageTitle.Text = "Word Guessing";
        _menuMetadata.Visibility = Visibility.Visible;
        _mainMenuView.Visibility = Visibility.Visible;

        // FIX: Herstel de originele venstertitel voor de hoofdmenu-tests
        var parentWindow = Window.GetWindow(_rootGrid);
        if (parentWindow != null)
        {
            parentWindow.Title = mainMenuTitle;
        }

        foreach (var child in _rootGrid.Children.OfType<FrameworkElement>()
            .Where(child => child != _pageTitle && child != _menuMetadata && child != _mainMenuView).ToList())
        {
            _rootGrid.Children.Remove(child);
        }
    }


    /// <summary>
    /// Dynamic sub-renderer helper that generates cells for the leaderboard table.
    /// </summary>
    public void AddTableCell(Grid grid, string text, int row, int column, bool header = false, int columnSpan = 1)
    {
        var cell = new TextBlock
        {
            Text = text,
            Foreground = header ? Brushes.LightGray : Brushes.White,
            FontWeight = header ? FontWeights.Bold : FontWeights.Normal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(4, 0, 4, 0),
            TextTrimming = TextTrimming.CharacterEllipsis
        };
        Grid.SetRow(cell, row);
        Grid.SetColumn(cell, column);
        Grid.SetColumnSpan(cell, columnSpan);

        var border = new Border
        {
            BorderBrush = new SolidColorBrush(Colors.DimGray),
            BorderThickness = new Thickness(1),
            Child = cell
        };
        Grid.SetRow(border, row);
        Grid.SetColumn(border, column);
        Grid.SetColumnSpan(border, columnSpan);
        grid.Children.Add(border);
    }
    /// <summary>
    /// Bouwt de complete highscore-pagina met navigatieknoppen en vult de datarijen.
    /// Voldoet aan Separation of Concerns: bevat geen enkele netwerk- of API-kennis.
    /// </summary>
    public StackPanel BuildHighscoresPage(
        List<ViewHighScoreRecord> scores,
        string mode,
        bool canGoBack,
        bool canGoNext,
        RoutedEventHandler backClick,
        RoutedEventHandler nextClick,
        RoutedEventHandler exitClick)
    {
        var content = new StackPanel { Width = 620 };
        content.Children.Add(new TextBlock
        {
            Text = mode,
            Foreground = Brushes.LightGray,
            FontSize = 14,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, -10, 0, 15)
        });

        var table = new Border
        {
            MinHeight = 360,
            Padding = new Thickness(16),
            Background = new SolidColorBrush(Colors.Black),
            BorderBrush = new SolidColorBrush(Colors.Gray),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(0, 0, 0, 20)
        };

        var tableGrid = new Grid();
        foreach (var width in new[] { new GridLength(60), new GridLength(1, GridUnitType.Star), new GridLength(90), new GridLength(180) })
            tableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = width });

        for (var row = 0; row < 11; row++)
            tableGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });

        var headers = new[] { "Rank", "Player", "Score", "Date" };
        for (var column = 0; column < headers.Length; column++)
            AddTableCell(tableGrid, headers[column], 0, column, true);

        if (scores.Count == 0)
        {
            AddTableCell(tableGrid, "No highscores yet.", 1, 0, false, 4);
        }
        else
        {
            for (int i = 0; i < scores.Count; i++)
            {
                int r = i + 1;
                if (r > 10) break;
                AddTableCell(tableGrid, scores[i].Rank, r, 0);
                AddTableCell(tableGrid, scores[i].PlayerName, r, 1);
                AddTableCell(tableGrid, scores[i].Score.ToString(), r, 2);
                AddTableCell(tableGrid, scores[i].FormattedDate, r, 3);
            }
        }

        table.Child = tableGrid;
        content.Children.Add(table);

        var navigation = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
        var btnBack = new Button { Content = "BACK", IsEnabled = canGoBack, Width = 120, Margin = new Thickness(5) };
        btnBack.Click += backClick;

        var btnExit = new Button { Content = "EXIT", Padding = new Thickness(10), Width = 180, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(5) };
        System.Windows.Automation.AutomationProperties.SetAutomationId(btnExit, "btn-back");
        btnExit.Click += exitClick;

        var btnNext = new Button { Content = "NEXT", IsEnabled = canGoNext, Width = 120, Margin = new Thickness(5) };
        btnNext.Click += nextClick;

        navigation.Children.Add(btnBack);
        navigation.Children.Add(btnExit);
        navigation.Children.Add(btnNext);
        content.Children.Add(navigation);

        return content;
    }
}
