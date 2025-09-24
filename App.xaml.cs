using System.Windows;
using SteamShortcutCreator.Services;
using SteamShortcutCreator.Views;

namespace SteamShortcutCreator;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var steamPath = SteamUtils.GetSteamPath();
        Window window;
        if (SteamUtils.IsValidSteamFolder(steamPath))
        {
            window = new MainWindow();
        }
        else
        {
            SteamUtils.SetSteamPath(null);
            window = new SteamSelectWindow();
        }

        window.Show();
    }
}