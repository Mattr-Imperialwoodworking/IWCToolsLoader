using System.Diagnostics;
using IWCToolsLoader.Models;

namespace IWCToolsLoader.Core;

public sealed class AppLaunchService
{
    private readonly Logger _logger;

    public AppLaunchService(Logger logger)
    {
        _logger = logger;
    }

    public void Launch(ToolApplication app, LocalAppRecord? localRecord)
    {
        if (localRecord == null || string.IsNullOrWhiteSpace(localRecord.EntryExePath) || !File.Exists(localRecord.EntryExePath))
            throw new FileNotFoundException($"Local application executable not found for {app.Title}.", localRecord?.EntryExePath ?? app.EntryExe);

        var psi = new ProcessStartInfo
        {
            FileName = localRecord.EntryExePath,
            Arguments = app.Arguments ?? string.Empty,
            WorkingDirectory = Path.GetDirectoryName(localRecord.EntryExePath) ?? localRecord.LocalPath,
            UseShellExecute = true
        };

        Process.Start(psi);
        _logger.Info($"Launched {app.Title}: {localRecord.EntryExePath}");
    }
}
