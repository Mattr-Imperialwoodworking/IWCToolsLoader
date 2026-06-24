namespace IWCToolsLoader.Core;

public static class LocalApplicationCleanupService
{
    public static LocalApplicationCleanupResult DeleteInstalledApplications(string localRoot)
    {
        var result = new LocalApplicationCleanupResult();

        string expandedRoot = PathProvider.ExpandPath(localRoot);
        if (string.IsNullOrWhiteSpace(expandedRoot))
        {
            result.Errors.Add("Local application root is not configured.");
            return result;
        }

        string appsDir = Path.Combine(expandedRoot, "Apps");
        string stateFile = Path.Combine(expandedRoot, "app-state.json");

        try
        {
            if (Directory.Exists(appsDir))
            {
                Directory.Delete(appsDir, recursive: true);
                result.DeletedPaths.Add(appsDir);
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Could not delete installed application folder '{appsDir}': {ex.Message}");
        }

        try
        {
            if (File.Exists(stateFile))
            {
                File.Delete(stateFile);
                result.DeletedPaths.Add(stateFile);
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Could not delete local application state file '{stateFile}': {ex.Message}");
        }

        return result;
    }
}

public sealed class LocalApplicationCleanupResult
{
    public List<string> DeletedPaths { get; } = new();
    public List<string> Errors { get; } = new();
    public bool HasErrors => Errors.Count > 0;
}
