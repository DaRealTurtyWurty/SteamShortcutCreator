using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using SteamShortcutCreator.Models;
using SteamShortcutCreator.Services;

namespace SteamShortcutCreator.Views;

public partial class MainWindow
{
    private SteamApp? _selectedApp;
    private readonly DispatcherTimer _statusTimer;

    public MainWindow()
    {
        InitializeComponent();

        CreateShortcutButton.IsEnabled = false;
        
        _statusTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        _statusTimer.Tick += (_, __) =>
        {
            StatusText.Visibility = Visibility.Collapsed;
            _statusTimer.Stop();
        };
        
        this.PreviewKeyDown += MainWindow_PreviewKeyDown;

        var steamPath = SteamUtils.GetSteamPath();
        if (string.IsNullOrEmpty(steamPath))
        {
            ShowStatus("Steam not found. Please install Steam first.");
            return;
        }
        
        List<SteamApp> apps = [];

        var appInfo = SteamUtils.ReadAppInfo(steamPath);
        var filteredApps = appInfo.Apps
            .Where(appInfoApp => appInfoApp.Data["common"] is not null)
            .Where(appInfoApp =>
                appInfoApp.Data["common"]["type"].ToString(CultureInfo.CurrentCulture) == "Game")
            .Where(appInfoApp => appInfoApp.Data["common"]["clienticon"] is not null);
        
        foreach (var appId in SteamUtils.GetSteamLibraries(steamPath).SelectMany(lib => lib.Apps))
        {
            apps.AddRange(from infoApp in filteredApps 
                where infoApp.AppID == appId 
                let common = infoApp.Data["common"]
                let name = common["name"].ToString(CultureInfo.CurrentCulture)
                let icon = common["clienticon"].ToString(CultureInfo.CurrentCulture)
                select new SteamApp(appId, name, Path.Combine(steamPath, "steam", "games", $"{icon}.ico")));
        }
        
        AppListBox.ItemsSource = apps;
        ShowStatus($"Loaded {apps.Count} games.");
    }
    
    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    private async Task LoadSteamAppsAsync()
    {
        await Task.CompletedTask;
        // var steamPath = SteamUtils.GetSteamPath();
        // if (string.IsNullOrEmpty(steamPath))
        // {
        //     MessageBox.Show("Steam not found. Please install Steam first.");
        //     return;
        // }
        //
        // var apps = new List<SteamApp>();
        // foreach (var steamLibrary in SteamUtils.GetSteamLibraries(steamPath))
        // {
        //     await AddAppsAsync(apps, steamLibrary);
        // }
        //
        // AppListBox.ItemsSource = apps;
    }

    private static async Task AddAppsAsync(List<SteamApp> apps, SteamUtils.SteamLibrary steamLibrary)
    {
        await Task.CompletedTask;
        // var appTasks = steamLibrary.Apps.Select(async appId => await SteamApp.CreateAsync(appId));
        // var createdApps = await Task.WhenAll(appTasks);
        // apps.AddRange(createdApps);
    }

    private void AppListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (AppListBox.SelectedItem is SteamApp app)
        {
            _selectedApp = app;
            SelectedGameLabel.Text = $"Selected: {app.Name} ({app.AppId})";
            CreateShortcutButton.IsEnabled = true;
            ShowStatus("Press Enter, double-click, or click Create Shortcut.");
        }
        else
        {
            _selectedApp = null;
            SelectedGameLabel.Text = "Pick a game from the list.";
            CreateShortcutButton.IsEnabled = false;
        }
    }
    
    private void AppListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (CreateShortcutButton.IsEnabled)
            ConfirmSelection_Click(sender, e);
    }
    private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && CreateShortcutButton.IsEnabled)
        {
            ConfirmSelection_Click(sender, e);
            e.Handled = true;
        }
        else if (e.Key == Key.Escape && AppListBox.SelectedItem != null)
        {
            AppListBox.SelectedItem = null;
            e.Handled = true;
        }
    }

    private void DeselectSelectedApp()
    {
        AppListBox.SelectedItem = null;
        _selectedApp = null;
        SelectedGameLabel.Text = "Pick a game from the list.";
        CreateShortcutButton.IsEnabled = false;
    }

    private void ConfirmSelection_Click(object? sender, RoutedEventArgs e)
    {
        if (_selectedApp is null)
        {
            ShowStatus("Please select a game first.");
            return;
        }

        var steamPath = SteamUtils.GetSteamPath();
        if (string.IsNullOrEmpty(steamPath))
        {
            ShowStatus("Steam not found. Please install Steam first.");
            return;
        }

        try
        {
            var shortcutPath = SteamUtils.CreateShortcut(_selectedApp.Value);
            ShowStatus($"Shortcut created: {shortcutPath}");
            DeselectSelectedApp();
        }
        catch (Exception ex)
        {
            ShowStatus($"Failed to create shortcut: {ex.Message}");
        }
    }
    
    private void ShowStatus(string message)
    {
        StatusText.Text = message;
        StatusText.Visibility = Visibility.Visible;
        _statusTimer.Stop();
        _statusTimer.Start();
    }
}