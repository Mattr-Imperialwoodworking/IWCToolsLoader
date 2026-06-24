using System.Text;

namespace IWCToolsLoader.Core;

public sealed class Logger
{
    private readonly string _logPath;
    public event Action<string>? MessageLogged;

    public Logger(string localRoot)
    {
        Directory.CreateDirectory(localRoot);
        _logPath = Path.Combine(localRoot, "loader.log");
    }

    public string LogPath => _logPath;

    public void Info(string message) => Write("INFO", message);
    public void Warn(string message) => Write("WARN", message);
    public void Error(string message) => Write("ERROR", message);

    public void Exception(Exception ex, string context)
    {
        Write("ERROR", $"{context}: {ex.Message}\r\n{ex}");
    }

    private void Write(string level, string message)
    {
        string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
        try
        {
            File.AppendAllText(_logPath, line + Environment.NewLine, Encoding.UTF8);
        }
        catch
        {
            // Logging must never stop app launch.
        }
        MessageLogged?.Invoke(line);
    }
}
