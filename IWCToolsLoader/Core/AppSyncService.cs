using IWCToolsLoader.Models;

namespace IWCToolsLoader.Core;

public sealed class AppSyncService
{
    private readonly string _localRoot;
    private readonly Logger _logger;
    private readonly LocalStateService _stateService;
    private readonly FileCopyService _copyService;

    public AppSyncService(string localRoot, Logger logger)
    {
        _localRoot = localRoot;
        _logger = logger;
        _stateService = new LocalStateService(localRoot);
        _copyService = new FileCopyService(logger);
    }

    public LocalAppState Sync(ToolManifest manifest, IProgress<SyncProgressEvent>? progress = null)
    {
        Directory.CreateDirectory(_localRoot);
        Directory.CreateDirectory(Path.Combine(_localRoot, "Apps"));

        var state = _stateService.Load();
        state.LastManifestVersion = manifest.ManifestVersion;

        var apps = manifest.Applications.Where(a => a.Enabled).OrderBy(a => a.DisplayOrder).ThenBy(a => a.Title).ToList();
        int index = 0;

        foreach (var app in apps)
        {
            index++;
            try
            {
                progress?.Report(new SyncProgressEvent(SyncProgressKind.Checking, app.Id, app.Title, $"Checking {app.Title}...", index, apps.Count));
                SyncApplication(app, state, progress, index, apps.Count);
            }
            catch (Exception ex)
            {
                _logger.Exception(ex, $"Failed to sync app {app.Id}");
                progress?.Report(new SyncProgressEvent(SyncProgressKind.Failed, app.Id, app.Title, $"Failed: {app.Title}", index, apps.Count));
            }
        }

        _stateService.Save(state);
        progress?.Report(new SyncProgressEvent(SyncProgressKind.Completed, null, null, "Application check complete.", apps.Count, apps.Count));
        return state;
    }

    private void SyncApplication(ToolApplication app, LocalAppState state, IProgress<SyncProgressEvent>? progress, int current, int total)
    {
        if (string.IsNullOrWhiteSpace(app.Id)) throw new InvalidOperationException("Manifest app entry is missing Id.");
        if (string.IsNullOrWhiteSpace(app.Version)) throw new InvalidOperationException($"Manifest app {app.Id} is missing Version.");
        if (string.IsNullOrWhiteSpace(app.Source)) throw new InvalidOperationException($"Manifest app {app.Id} is missing Source.");
        if (string.IsNullOrWhiteSpace(app.EntryExe)) throw new InvalidOperationException($"Manifest app {app.Id} is missing EntryExe.");

        string appFolder = Path.Combine(_localRoot, "Apps", app.EffectiveLocalFolderName, app.Version);
        string entryExePath = Path.Combine(appFolder, app.EntryExe);

        bool needsCopy = !state.Apps.TryGetValue(app.Id, out var local)
                         || !StringComparer.OrdinalIgnoreCase.Equals(local.Version, app.Version)
                         || !File.Exists(local.EntryExePath)
                         || !File.Exists(entryExePath);

        if (!needsCopy)
        {
            _logger.Info($"{app.Title} is current. Version={app.Version}");
            progress?.Report(new SyncProgressEvent(SyncProgressKind.Current, app.Id, app.Title, $"Current: {app.Title}", current, total));
            return;
        }

        string source = PathProvider.ExpandPath(app.Source);
        string sourceType = string.IsNullOrWhiteSpace(app.SourceType) ? "folder" : app.SourceType.Trim().ToLowerInvariant();

        progress?.Report(new SyncProgressEvent(SyncProgressKind.Installing, app.Id, app.Title, $"Installing {app.Title}...", current, total));

        if (sourceType == "folder")
        {
            _copyService.CopyFolder(source, appFolder);
        }
        else if (sourceType == "file")
        {
            _copyService.CopyFile(source, entryExePath);
        }
        else
        {
            throw new InvalidOperationException($"Unsupported sourceType '{app.SourceType}' for {app.Id}.");
        }

        if (!File.Exists(entryExePath))
            throw new FileNotFoundException($"Entry EXE was not found after copy: {entryExePath}", entryExePath);

        state.Apps[app.Id] = new LocalAppRecord
        {
            Id = app.Id,
            Version = app.Version,
            LocalPath = appFolder,
            EntryExePath = entryExePath,
            LastSyncedUtc = DateTime.UtcNow
        };

        _logger.Info($"{app.Title} synced. Version={app.Version}");
        progress?.Report(new SyncProgressEvent(SyncProgressKind.Installed, app.Id, app.Title, $"Installed/updated: {app.Title}", current, total));
    }
}
