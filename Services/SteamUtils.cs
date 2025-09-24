using System.IO;
using Gameloop.Vdf;
using Microsoft.Win32;
using SteamShortcutCreator.Models;

namespace SteamShortcutCreator.Services;

public static class SteamUtils
{
    private const string RegistryPath = @"HKEY_CURRENT_USER\Software\SteamShortcutCreator";
    private const string SteamPathKey = "SteamPath";

    public static string? GetSteamPath()
    {
        return Registry.GetValue(RegistryPath, SteamPathKey, null) as string;
    }

    public static void SetSteamPath(string? steamPath)
    {
        if (string.IsNullOrEmpty(steamPath))
        {
            Registry.CurrentUser.DeleteSubKeyTree(RegistryPath, false);
        }
        else
        {
            Registry.SetValue(RegistryPath, SteamPathKey, steamPath);
        }
    }

    public static bool IsValidSteamFolder(string? steamPath)
    {
        if (string.IsNullOrEmpty(steamPath)) return false;

        var libraryFoldersPath = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
        return File.Exists(libraryFoldersPath);
    }
    
    public static AppInfo ReadAppInfo(string steamPath)
    {
        var appInfoPath = Path.Combine(steamPath, "appcache", "appinfo.vdf");
        var appInfo = new AppInfo();
        appInfo.Read(appInfoPath);
        return appInfo;
    }

    public static string CreateShortcut(SteamApp steamApp)
    {
        var startMenuPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs", "Steam");
        Directory.CreateDirectory(startMenuPath);
        
        var shortcutPath = Path.Combine(startMenuPath, $"{steamApp.Name}.url");
        
        File.WriteAllLines(shortcutPath, [
            "[{000214A0-0000-0000-C000-000000000046}]",
            "Prop3=19,0",
            "[InternetShortcut]",
            "IDList=",
            "IconIndex=0",
            $"URL=steam://rungameid/{steamApp.AppId}",
            $@"IconFile={steamApp.Icon}"
        ]);
        
        return shortcutPath;
    }

    public static List<SteamLibrary> GetSteamLibraries(string steamPath)
    {
        var libraryFoldersPath = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
        var libraryFolders = new List<SteamLibrary>();

        dynamic volvo = VdfConvert.Deserialize(File.ReadAllText(libraryFoldersPath)).Value;
        foreach (var entry in volvo)
        {
            var val = entry.Value;

            string path = val.path.ToString();
            string label = val.label.ToString();
            long contentId = long.Parse(val.contentid.ToString());
            long totalSize = long.Parse(val.totalsize.ToString());
            long updateCleanBytesTally = long.Parse(val.update_clean_bytes_tally.ToString());
            long timeLastUpdateVerified = long.Parse(val.time_last_update_verified.ToString());

            var library = new SteamLibrary
            {
                Path = path,
                Label = label,
                ContentId = contentId,
                TotalSize = totalSize,
                UpdateCleanBytesTally = updateCleanBytesTally,
                TimeLastUpdateVerified = timeLastUpdateVerified,
                Apps = []
            };

            if (val.apps != null)
            {
                foreach (var app in val.apps)
                {
                    library.Apps.Add(int.Parse(app.Key.ToString()));
                }
            }

            libraryFolders.Add(library);
        }

        return libraryFolders;
    }

    public class SteamLibrary
    {
        public string Path { get; set; }
        public string Label { get; set; }
        public long ContentId { get; set; }
        public long TotalSize { get; set; }
        public long UpdateCleanBytesTally { get; set; }
        public long TimeLastUpdateVerified { get; set; }
        public List<long> Apps { get; set; }
    }
}