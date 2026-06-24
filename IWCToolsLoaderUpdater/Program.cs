using System.Diagnostics;

namespace IWCToolsLoaderUpdater;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        var options = UpdaterOptions.Parse(args);
        using var form = new UpdaterForm(options);
        Application.Run(form);
    }
}

internal sealed class UpdaterOptions
{
    public string Source { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public int ProcessId { get; init; }
    public string Restart { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;

    public static UpdaterOptions Parse(string[] args)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < args.Length; i++)
        {
            if (!args[i].StartsWith("--", StringComparison.Ordinal)) continue;
            string key = args[i][2..];
            string value = i + 1 < args.Length ? args[++i] : string.Empty;
            map[key] = value;
        }

        return new UpdaterOptions
        {
            Source = map.TryGetValue("source", out var source) ? source : string.Empty,
            Target = map.TryGetValue("target", out var target) ? target : string.Empty,
            Restart = map.TryGetValue("restart", out var restart) ? restart : string.Empty,
            Version = map.TryGetValue("version", out var version) ? version : string.Empty,
            ProcessId = map.TryGetValue("processId", out var pidText) && int.TryParse(pidText, out var pid) ? pid : 0
        };
    }
}

internal sealed class UpdaterForm : Form
{
    private static readonly Color IwcBrown = Color.FromArgb(102, 50, 6);
    private static readonly Color IwcGold = Color.FromArgb(245, 206, 63);

    private readonly UpdaterOptions _options;
    private readonly Label _message = new();
    private readonly ProgressBar _progress = new();

    public UpdaterForm(UpdaterOptions options)
    {
        _options = options;
        InitializeComponent();
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        try
        {
            await Task.Run(RunUpdate);
            SetMessage("Restarting IWC Tools Loader...");
            if (!string.IsNullOrWhiteSpace(_options.Restart) && File.Exists(_options.Restart))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = _options.Restart,
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(_options.Restart)
                });
            }
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "IWC Tools Loader Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Close();
        }
    }

    private void InitializeComponent()
    {
        Text = "Updating IWC Tools Loader";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Size = new Size(520, 210);
        BackColor = Color.White;

        var header = new Panel { Dock = DockStyle.Top, Height = 72, BackColor = IwcBrown };
        var title = new Label
        {
            Text = "Imperial Woodworking Company",
            Font = new Font("Baskerville Old Face", 17f, FontStyle.Bold),
            ForeColor = IwcGold,
            BackColor = Color.Transparent,
            Location = new Point(20, 18),
            Size = new Size(460, 30)
        };
        header.Controls.Add(title);

        _message.Text = "Updating IWC Tools Loader...";
        _message.Location = new Point(22, 92);
        _message.Size = new Size(460, 26);
        _message.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);

        _progress.Location = new Point(22, 126);
        _progress.Size = new Size(460, 16);
        _progress.Style = ProgressBarStyle.Marquee;
        _progress.MarqueeAnimationSpeed = 30;

        Controls.AddRange(new Control[] { header, _message, _progress });
    }

    private void RunUpdate()
    {
        if (string.IsNullOrWhiteSpace(_options.Source) || !Directory.Exists(_options.Source))
            throw new DirectoryNotFoundException("Update source folder not found: " + _options.Source);

        if (string.IsNullOrWhiteSpace(_options.Target))
            throw new InvalidOperationException("Update target folder was not specified.");

        if (_options.ProcessId > 0)
        {
            SetMessage("Waiting for the current loader to close...");
            try
            {
                using var process = Process.GetProcessById(_options.ProcessId);
                process.WaitForExit(20000);
            }
            catch
            {
                // Process is already gone or unavailable. Continue with the update.
            }
        }

        SetMessage("Copying updated loader files...");
        CopyDirectory(_options.Source, _options.Target);

        if (!string.IsNullOrWhiteSpace(_options.Version))
        {
            File.WriteAllText(Path.Combine(_options.Target, "Version.txt"), _options.Version);
        }
    }

    private void SetMessage(string message)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => SetMessage(message));
            return;
        }

        _message.Text = message;
        Application.DoEvents();
    }

    private static void CopyDirectory(string sourceDir, string destinationDir)
    {
        Directory.CreateDirectory(destinationDir);

        foreach (string directory in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(sourceDir, directory);
            if (relative.StartsWith("_loader-staging", StringComparison.OrdinalIgnoreCase)) continue;
            Directory.CreateDirectory(Path.Combine(destinationDir, relative));
        }

        foreach (string file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(sourceDir, file);
            if (relative.StartsWith("_loader-staging", StringComparison.OrdinalIgnoreCase)) continue;
            string destination = Path.Combine(destinationDir, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(file, destination, overwrite: true);
        }
    }
}
