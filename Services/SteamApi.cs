using System.IO;
using System.Net.Http;
using System.Text.Json;

namespace SteamShortcutCreator.Services;

public static class SteamApi
{
    private static readonly HttpClient Client = new();
    private const string CacheDir = "SteamAppCache";
    private const int AppsPerChunk = 2500;
    private static readonly SemaphoreSlim CacheLock = new(1, 1);

    public static async Task<string> GetNameFromAppId(long appId)
    {
        try
        {
            await EnsureAppListCached();
            
            var chunkLocation = await FindAppLocation(appId);
            if (chunkLocation == -1) 
                return $"App {appId} (Not Found)";
            
            return await GetNameFromChunk(chunkLocation, appId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching name for {appId}: {ex.Message}");
            return $"Error fetching name for {appId}";
        }
    }

    private static async Task EnsureAppListCached()
    {
        if (Directory.Exists(CacheDir) && File.Exists(Path.Combine(CacheDir, "index.json")))
            return;
        
        await CacheLock.WaitAsync();
        try
        {
            if (Directory.Exists(CacheDir) && File.Exists(Path.Combine(CacheDir, "index.json")))
                return;

            Directory.CreateDirectory(CacheDir);
            await FetchAndSplitAppList();
        }
        finally
        {
            CacheLock.Release();
        }
    }

    private static async Task FetchAndSplitAppList()
    {
        const string url = "https://api.steampowered.com/ISteamApps/GetAppList/v2/";
        using var response = await Client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        await using var stream = await response.Content.ReadAsStreamAsync();

        var json = await JsonSerializer.DeserializeAsync<JsonDocument>(stream);
        var apps = json?.RootElement.GetProperty("applist").GetProperty("apps").EnumerateArray();
        if (apps is null) return;

        var chunkIndex = new Dictionary<long, int>();
        var chunkNumber = 0;
        var appCount = 0;
        List<(long AppId, string Name)> currentChunk = [];

        foreach (var app in apps.Value)
        {
            var appId = app.GetProperty("appid").GetInt64();
            var name = app.GetProperty("name").GetString() ?? "";
            if (string.IsNullOrEmpty(name))
            {
                name = $"App {appId} (No Name)";
            }

            if (appCount % AppsPerChunk == 0 && currentChunk.Count > 0)
            {
                var chunkFile = Path.Combine(CacheDir, $"chunk_{chunkNumber}.json");
                await WriteChunkToFile(chunkFile, currentChunk, chunkIndex);
                currentChunk.Clear();
                chunkNumber++;
            }

            currentChunk.Add((appId, name));
            chunkIndex[appId] = chunkNumber;
            appCount++;
        }

        if (currentChunk.Count > 0)
        {
            var chunkFile = Path.Combine(CacheDir, $"chunk_{chunkNumber}.json");
            await WriteChunkToFile(chunkFile, currentChunk, chunkIndex);
        }

        await File.WriteAllTextAsync(
            Path.Combine(CacheDir, "index.json"),
            JsonSerializer.Serialize(chunkIndex, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static async Task WriteChunkToFile(string filePath, List<(long AppId, string Name)> chunk, Dictionary<long, int> index)
    {
        await using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, true);
        await using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
        
        writer.WriteStartArray();
        foreach (var (appId, name) in chunk)
        {
            writer.WriteStartObject();
            writer.WriteNumber("appid", appId);
            writer.WriteString("name", name);
            writer.WriteEndObject();
            await writer.FlushAsync();
        }
        writer.WriteEndArray();
        await writer.FlushAsync();
    }

    private static async Task<int> FindAppLocation(long appId)
    {
        var indexPath = Path.Combine(CacheDir, "index.json");
        var text = await File.ReadAllTextAsync(indexPath);
        var index = JsonSerializer.Deserialize<Dictionary<long, int>>(text);
        
        return index?.GetValueOrDefault(appId, -1) ?? -1;
    }

    private static async Task<string> GetNameFromChunk(int chunkIndex, long appId)
    {
        var chunkFile = Path.Combine(CacheDir, $"chunk_{chunkIndex}.json");
        await using var stream = new FileStream(chunkFile, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
        var json = await JsonSerializer.DeserializeAsync<JsonDocument>(stream);
        if(json is null) 
            return $"App {appId} (Not Found in Chunk)";
        
        var apps = json.RootElement.EnumerateArray();
        
        foreach (var app in apps.Where(app => app.GetProperty("appid").GetInt64() == appId))
            return app.GetProperty("name").GetString() ?? $"App {appId} (No Name)";
        
        return $"App {appId} (Not Found in Chunk)";
    }
}