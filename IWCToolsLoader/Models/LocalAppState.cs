namespace IWCToolsLoader.Models;

public sealed class LocalAppState
{
    public string LastManifestVersion { get; set; } = string.Empty;
    public Dictionary<string, LocalAppRecord> Apps { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class LocalAppRecord
{
    public string Id { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string LocalPath { get; set; } = string.Empty;
    public string EntryExePath { get; set; } = string.Empty;
    public DateTime LastSyncedUtc { get; set; }
}
