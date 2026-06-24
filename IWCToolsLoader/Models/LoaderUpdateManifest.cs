namespace IWCToolsLoader.Models;

public sealed class LoaderUpdateManifest
{
    public string AppId { get; set; } = "IWCToolsLoader";
    public string Title { get; set; } = "IWC Tools Loader";
    public string Version { get; set; } = "1.0.0";
    public string Released { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string SourceType { get; set; } = "folder";
    public string Source { get; set; } = string.Empty;
    public string EntryExe { get; set; } = "IWCToolsLoader.exe";
    public string UpdaterExe { get; set; } = "IWCToolsLoaderUpdater.exe";
    public bool Required { get; set; } = true;
}

public sealed class LoaderUpdateResult
{
    public bool UpdateStarted { get; init; }
    public bool ContinueStartup { get; init; } = true;
    public string Message { get; init; } = string.Empty;
}
