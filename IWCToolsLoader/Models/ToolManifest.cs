using System.Text.Json.Serialization;

namespace IWCToolsLoader.Models;

public sealed class ToolManifest
{
    public string ManifestVersion { get; set; } = "1.0.0";
    public string Released { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public List<ToolApplication> Applications { get; set; } = new();
}

public sealed class ToolApplication
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = "0.0.0";
    public bool Enabled { get; set; } = true;
    public int DisplayOrder { get; set; } = 100;
    public string SourceType { get; set; } = "folder"; // folder or file
    public string Source { get; set; } = string.Empty;
    public string LocalFolderName { get; set; } = string.Empty;
    public string EntryExe { get; set; } = string.Empty;
    public string Arguments { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty; // relative to app root, absolute path, or server path
    public string Publisher { get; set; } = "Imperial Woodworking";
    public List<string> Tags { get; set; } = new();

    [JsonIgnore]
    public string EffectiveLocalFolderName => string.IsNullOrWhiteSpace(LocalFolderName) ? Id : LocalFolderName;
}
