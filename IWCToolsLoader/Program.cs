using IWCToolsLoader.UI;

namespace IWCToolsLoader;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        // IWCToolsLoader is now started by IWCToolsBootstrap from a versioned
        // local folder. Do not self-update here; replacing a running executable
        // from inside itself was the source of the Version.txt/update loop.
        Application.Run(new MainForm());
    }
}
