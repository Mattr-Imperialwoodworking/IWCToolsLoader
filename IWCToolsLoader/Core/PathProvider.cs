namespace IWCToolsLoader.Core;

public static class PathProvider
{
    public static string ExpandPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return string.Empty;
        return Environment.ExpandEnvironmentVariables(path);
    }

    public static string DefaultSettingsDir => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ImperialWoodworking",
        "IWCToolsLoader");

    public static string LegacyRoamingSettingsDir => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ImperialWoodworking",
        "IWCToolsLoader");

    public static string LegacyRoamingSettingsPath => Path.Combine(LegacyRoamingSettingsDir, "loader-settings.json");

    public static string DefaultSettingsPath => Path.Combine(DefaultSettingsDir, "loader-settings.json");

    public static string DefaultLocalRoot => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ImperialWoodworking",
        "IWCTools");
}
