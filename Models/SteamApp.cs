namespace SteamShortcutCreator.Models;

public readonly struct SteamApp(long appId, string name, string icon)
{
    public long AppId { get; } = appId;
    public string Name { get; } = name;
    public string Icon { get; } = icon;
}