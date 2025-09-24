using System.Windows;
using SteamStartMenu.Services;
using SteamStartMenu.Views;

namespace SteamStartMenu;

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