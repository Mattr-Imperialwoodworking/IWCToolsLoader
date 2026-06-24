using System.Text.Json;
using Microsoft.Data.SqlClient;
using IWCToolsLoader.Models;

namespace IWCToolsLoader.Core;

public sealed class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public LoaderSettings LoadOrCreate()
    {
        Directory.CreateDirectory(PathProvider.DefaultSettingsDir);
        if (!File.Exists(PathProvider.DefaultSettingsPath))
        {
            // Earlier builds stored settings in Roaming AppData. The bootstrap
            // workflow keeps loader and app data in LocalAppData, but we still
            // migrate the existing SQL/settings file once so users do not have
            // to re-enter credentials.
            if (File.Exists(PathProvider.LegacyRoamingSettingsPath))
            {
                File.Copy(PathProvider.LegacyRoamingSettingsPath, PathProvider.DefaultSettingsPath, overwrite: true);
            }
            else
            {
                var settings = new LoaderSettings { LocalRoot = PathProvider.DefaultLocalRoot };
                Save(settings);
                return settings;
            }
        }

        string json = File.ReadAllText(PathProvider.DefaultSettingsPath);
        var loaded = JsonSerializer.Deserialize<LoaderSettings>(json, JsonOptions) ?? new LoaderSettings();
        if (string.IsNullOrWhiteSpace(loaded.LocalRoot)) loaded.LocalRoot = PathProvider.DefaultLocalRoot;
        if (string.IsNullOrWhiteSpace(loaded.ServerManifestPath)) loaded.ServerManifestPath = new LoaderSettings().ServerManifestPath;
        if (string.IsNullOrWhiteSpace(loaded.LoaderManifestPath)) loaded.LoaderManifestPath = new LoaderSettings().LoaderManifestPath;
        if (string.IsNullOrWhiteSpace(loaded.SqlServer)) loaded.SqlServer = "iwcprojectportal.database.windows.net";
        if (string.IsNullOrWhiteSpace(loaded.SqlDatabase)) loaded.SqlDatabase = "IWCProj";
        loaded.SqlConnectionString ??= string.Empty;
        loaded.SqlUserName ??= string.Empty;
        loaded.SqlPassword ??= string.Empty;

        // Migrate safe values from earlier full connection string setting when possible.
        // Do not preserve Integrated Security / Trusted_Connection because Azure SQL returns
        // "Windows logins are not supported" for this app configuration.
        if (!string.IsNullOrWhiteSpace(loaded.SqlConnectionString))
        {
            try
            {
                var builder = new SqlConnectionStringBuilder(loaded.SqlConnectionString);
                if (!string.IsNullOrWhiteSpace(builder.DataSource)) loaded.SqlServer = builder.DataSource;
                if (!string.IsNullOrWhiteSpace(builder.InitialCatalog)) loaded.SqlDatabase = builder.InitialCatalog;
                if (!string.IsNullOrWhiteSpace(builder.UserID)) loaded.SqlUserName = builder.UserID;
                if (!string.IsNullOrWhiteSpace(builder.Password)) loaded.SqlPassword = builder.Password;
            }
            catch
            {
                // Leave fields as-is. The Settings form will allow correction.
            }
        }

        loaded.SqlConnectionString = SqlConnectionFactory.BuildConnectionString(loaded);
        return loaded;
    }

    public void Save(LoaderSettings settings)
    {
        Directory.CreateDirectory(PathProvider.DefaultSettingsDir);
        settings.SqlServer = string.IsNullOrWhiteSpace(settings.SqlServer) ? "iwcprojectportal.database.windows.net" : settings.SqlServer.Trim();
        settings.SqlDatabase = string.IsNullOrWhiteSpace(settings.SqlDatabase) ? "IWCProj" : settings.SqlDatabase.Trim();
        settings.SqlUserName = settings.SqlUserName?.Trim() ?? string.Empty;
        settings.SqlPassword ??= string.Empty;
        settings.SqlConnectionString = SqlConnectionFactory.BuildConnectionString(settings);
        File.WriteAllText(PathProvider.DefaultSettingsPath, JsonSerializer.Serialize(settings, JsonOptions));
    }
}
