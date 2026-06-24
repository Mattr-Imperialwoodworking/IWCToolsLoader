using System.Text.Json;
using IWCToolsLoader.Models;

namespace IWCToolsLoader.Core;

public sealed class LocalStateService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly string _statePath;

    public LocalStateService(string localRoot)
    {
        Directory.CreateDirectory(localRoot);
        _statePath = Path.Combine(localRoot, "app-state.json");
    }

    public LocalAppState Load()
    {
        if (!File.Exists(_statePath)) return new LocalAppState();
        string json = File.ReadAllText(_statePath);
        return JsonSerializer.Deserialize<LocalAppState>(json, JsonOptions) ?? new LocalAppState();
    }

    public void Save(LocalAppState state)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_statePath)!);
        File.WriteAllText(_statePath, JsonSerializer.Serialize(state, JsonOptions));
    }
}
