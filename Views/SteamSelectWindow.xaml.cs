using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using SteamShortcutCreator.Services;

namespace SteamShortcutCreator.Views;

public partial class SteamSelectWindow
{
    private string? _steamPath;

    public SteamSelectWindow()
    {
        InitializeComponent();
    }

    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Select your Steam folder",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
        };

        if (!dialog.ShowDialog().GetValueOrDefault(false))
            return;

        if (SteamUtils.IsValidSteamFolder(dialog.FolderName))
        {
            SteamPathTextBox.Text = dialog.FolderName;
            ShowStatus("Valid Steam folder selected.", success: true);
        }
        else
        {
            ShowStatus("The selected folder is not a valid Steam installation.", success: false);
        }
    }

    private void ConfirmButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(SteamPathTextBox.Text))
        {
            ShowStatus("Please select a Steam folder first.", success: false);
            return;
        }

        if (!SteamUtils.IsValidSteamFolder(SteamPathTextBox.Text))
        {
            ShowStatus("Invalid Steam folder. Please try again.", success: false);
            return;
        }

        _steamPath = SteamPathTextBox.Text;
        SteamUtils.SetSteamPath(_steamPath);

        new MainWindow().Show();
        Close();
    }

    private void ShowStatus(string message, bool success)
    {
        StatusText.Text = message;
        StatusText.Foreground = success
            ? new SolidColorBrush(Colors.LightGreen)
            : new SolidColorBrush(Colors.IndianRed);

        StatusText.Visibility = Visibility.Visible;
    }
}