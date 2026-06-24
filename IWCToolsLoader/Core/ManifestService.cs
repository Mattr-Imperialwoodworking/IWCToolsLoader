using System.Text.Json;
using IWCToolsLoader.Models;

namespace IWCToolsLoader.Core;

public sealed class ManifestService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public ToolManifest Load(string manifestPath)
    {
        string expanded = PathProvider.ExpandPath(manifestPath);
        if (!File.Exists(expanded))
            throw new FileNotFoundException("Server manifest not found.", expanded);

        string json = File.ReadAllText(expanded);
        return JsonSerializer.Deserialize<ToolManifest>(json, JsonOptions)
               ?? throw new InvalidOperationException("Manifest could not be parsed.");
    }
}
