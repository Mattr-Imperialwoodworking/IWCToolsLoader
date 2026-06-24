namespace IWCToolsLoader.Models;

public sealed class LoaderSettings
{
    // Manifest for apps displayed in the dashboard.
    public string ServerManifestPath { get; set; } = @"\\IWCMAIN1\DRAFTING\LIBRARY\App\IWC Desktop Tools\desktop-tools-manifest.json";

    // Separate manifest for IWCToolsLoader self-updates. Checked before login.
    public string LoaderManifestPath { get; set; } = @"\\IWCMAIN1\DRAFTING\LIBRARY\App\IWC Desktop Tools\IWCToolsLoader\loader-manifest.json";

    public string LocalRoot { get; set; } = "%LOCALAPPDATA%\\ImperialWoodworking\\IWCTools";
    public bool AutoSyncOnStartup { get; set; } = true;
    public bool KeepOldVersions { get; set; } = true;
    public bool AutoUpdateLoaderOnStartup { get; set; } = true;

    // Preferred SQL settings. The app builds the working connection string from these values.
    public string SqlServer { get; set; } = "iwcprojectportal.database.windows.net";
    public string SqlDatabase { get; set; } = "IWCProj";
    public string SqlUserName { get; set; } = string.Empty;
    public string SqlPassword { get; set; } = string.Empty;

    // Retained for backward compatibility with earlier settings files.
    public string SqlConnectionString { get; set; } = string.Empty;
}
