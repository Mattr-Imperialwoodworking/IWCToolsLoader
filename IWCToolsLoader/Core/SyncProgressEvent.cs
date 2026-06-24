namespace IWCToolsLoader.Core;

public enum SyncProgressKind
{
    Checking,
    Current,
    Installing,
    Installed,
    Failed,
    Completed
}

public sealed class SyncProgressEvent
{
    public SyncProgressEvent(SyncProgressKind kind, string? appId, string? appTitle, string message, int current, int total)
    {
        Kind = kind;
        AppId = appId;
        AppTitle = appTitle;
        Message = message;
        Current = current;
        Total = total;
    }

    public SyncProgressKind Kind { get; }
    public string? AppId { get; }
    public string? AppTitle { get; }
    public string Message { get; }
    public int Current { get; }
    public int Total { get; }
}
