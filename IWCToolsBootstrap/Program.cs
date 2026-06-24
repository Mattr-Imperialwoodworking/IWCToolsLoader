using System.Diagnostics;
using System.Text.Json;

namespace IWCToolsBootstrap;

internal static class Program
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        using var form = new BootstrapStatusForm();
        form.Shown += async (_, _) =>
        {
            try
            {
                await Task.Run(() => RunBootstrap(form));
                form.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    form,
                    ex.Message,
                    "IWC Tools Loader Startup",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                form.Close();
            }
        };

        Application.Run(form);
    }

    private static void RunBootstrap(BootstrapStatusForm form)
    {
        form.SetMessage("Reading IWC Tools Loader bootstrap settings...");

        BootstrapConfig config = LoadOrCreateConfig();
        string manifestPath = ExpandPath(config.LoaderManifestPath);
        string localRoot = ExpandPath(config.LocalLoaderRoot);
        string versionsRoot = Path.Combine(localRoot, string.IsNullOrWhiteSpace(config.VersionsFolderName) ? "Versions" : config.VersionsFolderName);

        if (string.IsNullOrWhiteSpace(manifestPath))
            throw new InvalidOperationException("Loader manifest path is not configured.");

        if (!File.Exists(manifestPath))
            throw new FileNotFoundException("Loader manifest was not found.", manifestPath);

        Directory.CreateDirectory(localRoot);
        Directory.CreateDirectory(versionsRoot);

        form.SetMessage("Reading server loader manifest...");
        LoaderUpdateManifest manifest = ReadManifest(manifestPath);
        string version = NormalizeVersion(manifest.Version);
        string entryExe = FirstNonBlank(manifest.EntryExe, config.EntryExe, "IWCToolsLoader.exe");
        string localVersionDir = Path.Combine(versionsRoot, version);
        string localEntryExe = Path.Combine(localVersionDir, entryExe);

        if (!Directory.Exists(localVersionDir) || !File.Exists(localEntryExe))
        {
            form.SetMessage($"Installing IWC Tools Loader {version}...");
            string sourceRoot = ResolveSourceRoot(manifest, entryExe);

            string tempDir = Path.Combine(localRoot, "_bootstrap-staging", version + "_" + DateTime.Now.ToString("yyyyMMddHHmmss"));
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, recursive: true);
            Directory.CreateDirectory(tempDir);

            CopyDirectory(sourceRoot, tempDir);

            string stagedRoot = ResolveReleaseRoot(tempDir, entryExe);
            if (!File.Exists(Path.Combine(stagedRoot, entryExe)))
                throw new FileNotFoundException("The staged loader entry executable was not found.", Path.Combine(stagedRoot, entryExe));

            if (Directory.Exists(localVersionDir)) Directory.Delete(localVersionDir, recursive: true);
            Directory.CreateDirectory(localVersionDir);
            CopyDirectory(stagedRoot, localVersionDir);

            WriteVersionStamp(localVersionDir, version);
            SafeDeleteDirectory(Path.Combine(localRoot, "_bootstrap-staging"));
        }
        else
        {
            form.SetMessage($"IWC Tools Loader {version} is already installed locally...");
        }

        if (!File.Exists(localEntryExe))
            throw new FileNotFoundException("The local loader executable could not be found.", localEntryExe);

        WriteActiveLoaderState(localRoot, manifest, version, localEntryExe, manifestPath);

        if (config.LaunchAfterUpdate)
        {
            form.SetMessage("Starting IWC Tools Loader...");
            Process.Start(new ProcessStartInfo
            {
                FileName = localEntryExe,
                UseShellExecute = true,
                WorkingDirectory = Path.GetDirectoryName(localEntryExe)
            });
        }
    }

    private static BootstrapConfig LoadOrCreateConfig()
    {
        string configDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ImperialWoodworking",
            "IWCToolsBootstrap");
        string configPath = Path.Combine(configDir, "iwc-loader-bootstrap.json");

        Directory.CreateDirectory(configDir);

        if (!File.Exists(configPath))
        {
            var config = new BootstrapConfig();
            File.WriteAllText(configPath, JsonSerializer.Serialize(config, JsonOptions));
            return config;
        }

        string json = File.ReadAllText(configPath);
        var loaded = JsonSerializer.Deserialize<BootstrapConfig>(json, JsonOptions) ?? new BootstrapConfig();
        if (string.IsNullOrWhiteSpace(loaded.LoaderManifestPath)) loaded.LoaderManifestPath = new BootstrapConfig().LoaderManifestPath;
        if (string.IsNullOrWhiteSpace(loaded.LocalLoaderRoot)) loaded.LocalLoaderRoot = new BootstrapConfig().LocalLoaderRoot;
        if (string.IsNullOrWhiteSpace(loaded.VersionsFolderName)) loaded.VersionsFolderName = "Versions";
        if (string.IsNullOrWhiteSpace(loaded.EntryExe)) loaded.EntryExe = "IWCToolsLoader.exe";
        return loaded;
    }

    private static LoaderUpdateManifest ReadManifest(string manifestPath)
    {
        string json = File.ReadAllText(manifestPath);
        return JsonSerializer.Deserialize<LoaderUpdateManifest>(json, JsonOptions)
               ?? throw new InvalidOperationException("Loader manifest could not be read.");
    }

    private static string ResolveSourceRoot(LoaderUpdateManifest manifest, string entryExe)
    {
        string source = ExpandPath(manifest.Source);
        if (string.IsNullOrWhiteSpace(source))
            throw new InvalidOperationException("Loader manifest source is blank.");

        if (!Directory.Exists(source))
            throw new DirectoryNotFoundException("Loader source folder was not found: " + source);

        return ResolveReleaseRoot(source, entryExe);
    }

    private static string ResolveReleaseRoot(string root, string entryExe)
    {
        string direct = Path.Combine(root, entryExe);
        if (File.Exists(direct)) return root;

        string? found = Directory
            .GetFiles(root, entryExe, SearchOption.AllDirectories)
            .OrderBy(path => Path.GetRelativePath(root, path).Count(ch => ch == Path.DirectorySeparatorChar || ch == Path.AltDirectorySeparatorChar))
            .ThenBy(path => path.Length)
            .FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(found))
            return Path.GetDirectoryName(found) ?? root;

        throw new FileNotFoundException("The loader source folder does not contain the entry executable.", Path.Combine(root, entryExe));
    }

    private static void CopyDirectory(string sourceDir, string destinationDir)
    {
        Directory.CreateDirectory(destinationDir);

        foreach (string directory in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(sourceDir, directory);
            if (ShouldSkip(relative)) continue;
            Directory.CreateDirectory(Path.Combine(destinationDir, relative));
        }

        foreach (string file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(sourceDir, file);
            if (ShouldSkip(relative)) continue;

            string destination = Path.Combine(destinationDir, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(file, destination, overwrite: true);
        }
    }

    private static bool ShouldSkip(string relativePath)
    {
        return relativePath.StartsWith("_loader-staging", StringComparison.OrdinalIgnoreCase)
            || relativePath.StartsWith("_bootstrap-staging", StringComparison.OrdinalIgnoreCase)
            || relativePath.Contains(Path.DirectorySeparatorChar + ".vs" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            || relativePath.StartsWith(".vs" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    private static void WriteVersionStamp(string localVersionDir, string version)
    {
        File.WriteAllText(Path.Combine(localVersionDir, "Version.txt"), version.Trim());
    }

    private static void WriteActiveLoaderState(string localRoot, LoaderUpdateManifest manifest, string version, string localEntryExe, string manifestPath)
    {
        var state = new
        {
            ActiveVersion = version,
            ActiveExe = localEntryExe,
            ManifestPath = manifestPath,
            ManifestVersion = manifest.Version,
            Released = manifest.Released,
            UpdatedUtc = DateTime.UtcNow
        };

        File.WriteAllText(Path.Combine(localRoot, "active-loader.json"), JsonSerializer.Serialize(state, JsonOptions));
        File.WriteAllText(Path.Combine(localRoot, "CurrentVersion.txt"), version);
    }

    private static void SafeDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path)) Directory.Delete(path, recursive: true);
        }
        catch
        {
            // Non-fatal. A stale staging folder should not prevent the loader from starting.
        }
    }

    private static string ExpandPath(string path) => Environment.ExpandEnvironmentVariables(path ?? string.Empty);

    private static string NormalizeVersion(string? version)
    {
        if (string.IsNullOrWhiteSpace(version)) return "0.0.0";
        return version.Trim();
    }

    private static string FirstNonBlank(params string?[] values)
    {
        foreach (string? value in values)
        {
            if (!string.IsNullOrWhiteSpace(value)) return value.Trim();
        }
        return string.Empty;
    }
}

internal sealed class BootstrapConfig
{
    public string LoaderManifestPath { get; set; } = @"\\IWCMAIN1\DRAFTING\LIBRARY\App\IWC Desktop Tools\IWCToolsLoader\loader-manifest.json";
    public string LocalLoaderRoot { get; set; } = "%LOCALAPPDATA%\\ImperialWoodworking\\IWCToolsLoader";
    public string VersionsFolderName { get; set; } = "Versions";
    public string EntryExe { get; set; } = "IWCToolsLoader.exe";
    public bool KeepOldVersions { get; set; } = true;
    public bool LaunchAfterUpdate { get; set; } = true;
}

internal sealed class LoaderUpdateManifest
{
    public string AppId { get; set; } = "IWCToolsLoader";
    public string Title { get; set; } = "IWC Tools Loader";
    public string Version { get; set; } = "0.0.0";
    public string Released { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string SourceType { get; set; } = "folder";
    public string Source { get; set; } = string.Empty;
    public string EntryExe { get; set; } = "IWCToolsLoader.exe";
    public bool Required { get; set; } = true;
}

internal sealed class BootstrapStatusForm : Form
{
    private static readonly Color IwcBrown = Color.FromArgb(102, 50, 6);
    private static readonly Color IwcGold = Color.FromArgb(245, 206, 63);

    private readonly Label _message = new();
    private readonly ProgressBar _progress = new();

    public BootstrapStatusForm()
    {
        Text = "IWC Tools Loader Startup";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Size = new Size(560, 210);
        BackColor = Color.White;

        var header = new Panel { Dock = DockStyle.Top, Height = 76, BackColor = IwcBrown };
        var title = new Label
        {
            Text = "Imperial Woodworking Company",
            Font = new Font("Baskerville Old Face", 17f, FontStyle.Bold),
            ForeColor = IwcGold,
            BackColor = Color.Transparent,
            Location = new Point(20, 18),
            Size = new Size(500, 30)
        };
        var subtitle = new Label
        {
            Text = "IWC Tools startup bootstrap",
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            ForeColor = IwcGold,
            BackColor = Color.Transparent,
            Location = new Point(22, 48),
            Size = new Size(500, 20)
        };
        header.Controls.AddRange(new Control[] { title, subtitle });

        _message.Text = "Starting IWC Tools Loader...";
        _message.Location = new Point(22, 94);
        _message.Size = new Size(500, 26);
        _message.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);

        _progress.Location = new Point(22, 130);
        _progress.Size = new Size(500, 16);
        _progress.Style = ProgressBarStyle.Marquee;
        _progress.MarqueeAnimationSpeed = 30;

        Controls.AddRange(new Control[] { header, _message, _progress });
    }

    public void SetMessage(string message)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => SetMessage(message));
            return;
        }

        _message.Text = message;
        Application.DoEvents();
    }
}
