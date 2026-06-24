using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using IWCToolsLoader.Models;
using IWCToolsLoader.UI;

namespace IWCToolsLoader.Core;

public sealed class LoaderSelfUpdateService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public LoaderUpdateResult CheckAndStartUpdaterIfNeeded(LoaderSettings settings, UpdateStatusForm? statusForm = null)
    {
        if (!settings.AutoUpdateLoaderOnStartup)
            return new LoaderUpdateResult { ContinueStartup = true, Message = "Loader self-update check disabled." };

        string manifestPath = PathProvider.ExpandPath(settings.LoaderManifestPath);
        if (string.IsNullOrWhiteSpace(manifestPath))
            return new LoaderUpdateResult { ContinueStartup = true, Message = "Loader manifest path not configured." };

        statusForm?.SetMessage("Checking for IWC Tools Loader updates...");

        if (!File.Exists(manifestPath))
            return new LoaderUpdateResult { ContinueStartup = true, Message = $"Loader manifest not found: {manifestPath}" };

        LoaderUpdateManifest manifest = LoadManifest(manifestPath);
        Version currentVersion = GetCurrentVersion();
        Version serverVersion = ParseVersion(manifest.Version);

        if (serverVersion <= currentVersion)
            return new LoaderUpdateResult { ContinueStartup = true, Message = $"IWC Tools Loader is current. Version {currentVersion}." };

        statusForm?.SetMessage($"IWC Tools Loader {manifest.Version} is available. Staging update...");

        string source = PathProvider.ExpandPath(manifest.Source);
        if (string.IsNullOrWhiteSpace(source) || !Directory.Exists(source))
        {
            string message = $"Loader update source folder not found: {source}";
            if (manifest.Required) return new LoaderUpdateResult { ContinueStartup = false, Message = message };
            return new LoaderUpdateResult { ContinueStartup = true, Message = message };
        }

        string installDir = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        string stagingRoot = Path.Combine(PathProvider.DefaultSettingsDir, "_loader-staging", manifest.Version);

        if (Directory.Exists(stagingRoot)) Directory.Delete(stagingRoot, recursive: true);
        Directory.CreateDirectory(stagingRoot);

        CopyDirectory(source, stagingRoot);

        string stagedUpdater = Path.Combine(stagingRoot, manifest.UpdaterExe);
        string restartExe = Path.Combine(installDir, manifest.EntryExe);

        if (!File.Exists(stagedUpdater))
        {
            string message = $"Updater executable not found in staged update: {stagedUpdater}";
            if (manifest.Required) return new LoaderUpdateResult { ContinueStartup = false, Message = message };
            return new LoaderUpdateResult { ContinueStartup = true, Message = message };
        }

        statusForm?.SetMessage("Starting IWC Tools Loader updater...");

        string args =
            $"--source \"{stagingRoot}\" " +
            $"--target \"{installDir}\" " +
            $"--processId {Environment.ProcessId} " +
            $"--restart \"{restartExe}\" " +
            $"--version \"{manifest.Version}\"";

        Process.Start(new ProcessStartInfo
        {
            FileName = stagedUpdater,
            Arguments = args,
            UseShellExecute = true,
            WorkingDirectory = stagingRoot
        });

        return new LoaderUpdateResult
        {
            UpdateStarted = true,
            ContinueStartup = false,
            Message = $"IWC Tools Loader update {manifest.Version} started."
        };
    }

    private static LoaderUpdateManifest LoadManifest(string manifestPath)
    {
        string json = File.ReadAllText(manifestPath);
        return JsonSerializer.Deserialize<LoaderUpdateManifest>(json, JsonOptions)
               ?? throw new InvalidOperationException("Loader manifest could not be read.");
    }

    private static Version GetCurrentVersion()
    {
        string versionFile = Path.Combine(AppContext.BaseDirectory, "Version.txt");
        if (File.Exists(versionFile))
        {
            string text = File.ReadAllText(versionFile).Trim();
            if (Version.TryParse(text, out var fileVersion)) return fileVersion;
        }

        string? assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
        return ParseVersion(assemblyVersion ?? "0.0.0");
    }

    private static Version ParseVersion(string version)
    {
        if (Version.TryParse(version, out var parsed)) return parsed;
        string numeric = new string(version.TakeWhile(c => char.IsDigit(c) || c == '.').ToArray());
        return Version.TryParse(numeric, out parsed) ? parsed : new Version(0, 0, 0);
    }

    private static void CopyDirectory(string sourceDir, string destinationDir)
    {
        Directory.CreateDirectory(destinationDir);

        foreach (string directory in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(sourceDir, directory);
            Directory.CreateDirectory(Path.Combine(destinationDir, relative));
        }

        foreach (string file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(sourceDir, file);
            string destination = Path.Combine(destinationDir, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(file, destination, overwrite: true);
        }
    }
}
