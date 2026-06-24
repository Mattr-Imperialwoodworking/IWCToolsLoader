using IWCToolsLoader.Auth;
using IWCToolsLoader.Branding;
using IWCToolsLoader.Core;
using IWCToolsLoader.Models;

namespace IWCToolsLoader.UI;

public sealed class MainForm : Form
{
    private readonly SettingsService _settingsService = new();
    private LoaderSettings _settings = new();
    private Logger? _logger;
    private ToolManifest? _manifest;
    private LocalAppState _state = new();
    private bool _syncInProgress;
    private AuthenticatedUser? _currentUser;
    private string _sqlConnectionString = string.Empty;


    private readonly Panel _headerPanel = new();
    private readonly PictureBox _headerIcon = new();
    private readonly Label _header = new();
    private readonly Label _subHeader = new();
    private readonly Label _workingLabel = new();
    private readonly Button _refreshButton = new();
    private readonly Button _settingsButton = new();
    private readonly Button _changePasswordButton = new();
    private readonly ProgressBar _mainProgress = new();
    private readonly FlowLayoutPanel _appsPanel = new();
    private readonly TextBox _logBox = new();
    private readonly StatusStrip _status = new();
    private readonly ToolStripStatusLabel _statusText = new();
    private readonly Dictionary<string, AppCardControl> _cardsByAppId = new(StringComparer.OrdinalIgnoreCase);

    public MainForm()
    {
        InitializeComponent();
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        _settings = _settingsService.LoadOrCreate();
        string localRoot = PathProvider.ExpandPath(_settings.LocalRoot);
        _logger = new Logger(localRoot);
        _logger.MessageLogged += line => BeginInvoke(() => AppendLog(line));

        if (!PromptForLogin())
        {
            Close();
            return;
        }

        ApplyUserGreeting();

        if (_settings.AutoSyncOnStartup)
            await SyncAndRenderAsync();
        else
            LoadManifestAndRender(sync: false);
    }

    private void InitializeComponent()
    {
        Text = "IWC Tools Loader";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(780, 620);
        Size = new Size(940, 720);
        BackColor = IwcBrand.BodyBackground;
        AutoScaleMode = AutoScaleMode.Dpi;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            BackColor = IwcBrand.BodyBackground,
            Padding = new Padding(0),
            Margin = new Padding(0),
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 76f));  // branded header
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34f));  // status text
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 22f));  // progress bar
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));  // application cards
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 120f)); // log

        _headerPanel.Dock = DockStyle.Fill;
        _headerPanel.Height = 76;
        _headerPanel.BackColor = IwcBrand.HeaderBrown;
        _headerPanel.Padding = new Padding(20, 8, 20, 8);

        var headerLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = IwcBrand.HeaderBrown,
            Margin = new Padding(0),
            Padding = new Padding(0),
        };
        headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 76f));
        headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 390f));
        headerLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        _headerIcon.Dock = DockStyle.Fill;
        _headerIcon.Margin = new Padding(0, 0, 16, 0);
        _headerIcon.SizeMode = PictureBoxSizeMode.Zoom;
        _headerIcon.BackColor = Color.Transparent;
        _headerIcon.Image = LoadImageFromAppFolder(Path.Combine("Resources", "IWCToolsLoaderHeaderIcon.png"));

        var titleStack = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = IwcBrand.HeaderBrown,
            Margin = new Padding(0),
            Padding = new Padding(0),
        };
        titleStack.RowStyles.Add(new RowStyle(SizeType.Percent, 58f));
        titleStack.RowStyles.Add(new RowStyle(SizeType.Percent, 42f));

        _header.Text = "Imperial Woodworking Company";
        _header.Dock = DockStyle.Fill;
        _header.AutoEllipsis = true;
        _header.Font = new Font("Baskerville Old Face", 18f, FontStyle.Bold);
        _header.ForeColor = IwcBrand.Gold;
        _header.BackColor = Color.Transparent;
        _header.TextAlign = ContentAlignment.BottomLeft;
        _header.Margin = new Padding(0);

        _subHeader.Text = "Company application dashboard";
        _subHeader.Dock = DockStyle.Fill;
        _subHeader.AutoEllipsis = true;
        _subHeader.Font = new Font("Adobe Caslon Pro", 9.5f, FontStyle.Bold);
        _subHeader.ForeColor = IwcBrand.Gold;
        _subHeader.BackColor = Color.Transparent;
        _subHeader.TextAlign = ContentAlignment.TopLeft;
        _subHeader.Margin = new Padding(0);

        titleStack.Controls.Add(_header, 0, 0);
        titleStack.Controls.Add(_subHeader, 0, 1);

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            BackColor = IwcBrand.HeaderBrown,
            Padding = new Padding(0, 16, 0, 0),
            Margin = new Padding(12, 0, 0, 0),
        };

        ConfigureHeaderButton(_settingsButton, "Settings", (_, _) => ShowSettings(), width: 85);
        ConfigureHeaderButton(_changePasswordButton, "Change Password", (_, _) => ShowChangePassword(), width: 130);
        ConfigureHeaderButton(_refreshButton, "Check for Updates", async (_, _) => await SyncAndRenderAsync(), width: 140);

        buttonPanel.Controls.Add(_settingsButton);
        buttonPanel.Controls.Add(_changePasswordButton);
        buttonPanel.Controls.Add(_refreshButton);

        headerLayout.Controls.Add(_headerIcon, 0, 0);
        headerLayout.Controls.Add(titleStack, 1, 0);
        headerLayout.Controls.Add(buttonPanel, 2, 0);
        _headerPanel.Controls.Add(headerLayout);

        _workingLabel.Text = "Ready";
        _workingLabel.Dock = DockStyle.Fill;
        _workingLabel.Margin = new Padding(20, 6, 20, 0);
        _workingLabel.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        _workingLabel.ForeColor = IwcBrand.StatusText;
        _workingLabel.BackColor = IwcBrand.BodyBackground;
        _workingLabel.AutoEllipsis = true;
        _workingLabel.TextAlign = ContentAlignment.MiddleLeft;

        _mainProgress.Dock = DockStyle.Fill;
        _mainProgress.Margin = new Padding(20, 2, 20, 6);
        _mainProgress.Minimum = 0;
        _mainProgress.Value = 0;

        _appsPanel.Dock = DockStyle.Fill;
        _appsPanel.Margin = new Padding(18, 8, 18, 8);
        _appsPanel.AutoScroll = true;
        _appsPanel.WrapContents = false;
        _appsPanel.FlowDirection = FlowDirection.TopDown;
        _appsPanel.BackColor = IwcBrand.BodyBackground;
        _appsPanel.Resize += (_, _) => ResizeAppCards();

        _logBox.Dock = DockStyle.Fill;
        _logBox.Margin = new Padding(18, 4, 18, 4);
        _logBox.Multiline = true;
        _logBox.ScrollBars = ScrollBars.Vertical;
        _logBox.ReadOnly = true;
        _logBox.Font = new Font("Consolas", 8.5f);
        _logBox.BackColor = IwcBrand.LogBackground;
        _logBox.BorderStyle = BorderStyle.FixedSingle;

        _status.Dock = DockStyle.Bottom;
        _status.BackColor = IwcBrand.BodyBackground;
        _statusText.Text = "Ready";
        _status.Items.Add(_statusText);

        root.Controls.Add(_headerPanel, 0, 0);
        root.Controls.Add(_workingLabel, 0, 1);
        root.Controls.Add(_mainProgress, 0, 2);
        root.Controls.Add(_appsPanel, 0, 3);
        root.Controls.Add(_logBox, 0, 4);

        Controls.Add(root);
        Controls.Add(_status);
    }

    private static void ConfigureHeaderButton(Button button, string text, EventHandler clickHandler, int width)
    {
        button.Text = text;
        button.Size = new Size(width, 32);
        button.Margin = new Padding(5, 0, 5, 0);
        button.BackColor = IwcBrand.BodyBackground;
        button.ForeColor = IwcBrand.MainText;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = IwcBrand.ControlBorder;
        button.Click += clickHandler;
    }

    private void ResizeAppCards()
    {
        if (_appsPanel.ClientSize.Width <= 0) return;

        int targetWidth = Math.Max(540, _appsPanel.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 12);
        foreach (Control control in _appsPanel.Controls)
        {
            if (control is AppCardControl card)
                card.Width = targetWidth;
        }
    }

    private static Image? LoadImageFromAppFolder(string relativePath)
    {
        try
        {
            string path = Path.Combine(AppContext.BaseDirectory, relativePath);
            if (!File.Exists(path))
                return null;

            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var ms = new MemoryStream();
            fs.CopyTo(ms);
            return Image.FromStream(new MemoryStream(ms.ToArray()));
        }
        catch
        {
            return null;
        }
    }

    private void PositionHeaderButtons()
    {
        // Layout is handled by TableLayoutPanel and FlowLayoutPanel.
    }


    private bool PromptForLogin()
    {
        if (!ConnectionStringValidator.TryValidate(_settings, out string connectionMessage, out _sqlConnectionString))
        {
            MessageBox.Show(this,
                connectionMessage + "\r\n\r\nOpen Settings and enter the IWCProj SQL server, SQL username, and SQL password before logging in.",
                "IWC Tools Login",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            ShowSettings();
            _settings = _settingsService.LoadOrCreate();

            if (!ConnectionStringValidator.TryValidate(_settings, out connectionMessage, out _sqlConnectionString))
            {
                MessageBox.Show(this,
                    "Login cannot continue until the IWCProj SQL settings are valid.",
                    "IWC Tools Login",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return false;
            }
        }

        string localRoot = PathProvider.ExpandPath(_settings.LocalRoot);
        using var login = new LoginForm(new UserAuthService(_sqlConnectionString), localRoot);
        if (login.ShowDialog(this) != DialogResult.OK || login.AuthenticatedUser == null)
            return false;

        _currentUser = login.AuthenticatedUser;
        _logger?.Info($"User logged in: {_currentUser.UserLogin}");
        return true;
    }

    private void ApplyUserGreeting()
    {
        if (_currentUser == null) return;
        _subHeader.Text = $"Welcome, {_currentUser.FirstName} — Company application dashboard";
        _workingLabel.Text = $"Welcome, {_currentUser.FirstName}. Checking application status...";
        _statusText.Text = $"Logged in as {_currentUser.UserLogin}";
    }

    private async Task SyncAndRenderAsync()
    {
        if (_syncInProgress) return;

        try
        {
            _syncInProgress = true;
            SetBusy(true, "Checking for updates...");
            await LoadManifestAndRenderAsync(sync: true);
            SetBusy(false, "Applications updated and ready.");
        }
        catch (Exception ex)
        {
            _logger?.Exception(ex, "Sync failed");
            MessageBox.Show(this, ex.Message, "IWC Tools Loader", MessageBoxButtons.OK, MessageBoxIcon.Error);
            SetBusy(false, "Sync failed.");
        }
        finally
        {
            _syncInProgress = false;
        }
    }

    private Task LoadManifestAndRenderAsync(bool sync)
    {
        if (_logger == null) return Task.CompletedTask;

        string localRoot = PathProvider.ExpandPath(_settings.LocalRoot);
        var manifestService = new ManifestService();
        _manifest = manifestService.Load(_settings.ServerManifestPath);
        _logger.Info($"Manifest loaded. Version={_manifest.ManifestVersion}; Released={_manifest.Released}");

        var stateService = new LocalStateService(localRoot);
        _state = stateService.Load();
        RenderApps();

        if (!sync)
            return Task.CompletedTask;

        int enabledCount = _manifest.Applications.Count(a => a.Enabled);
        _mainProgress.Maximum = Math.Max(enabledCount, 1);
        _mainProgress.Value = 0;

        var progress = new Progress<SyncProgressEvent>(OnSyncProgress);
        return Task.Run(() =>
        {
            var syncService = new AppSyncService(localRoot, _logger);
            _state = syncService.Sync(_manifest, progress);
        }).ContinueWith(t =>
        {
            if (t.Exception != null) throw t.Exception.GetBaseException();
            RenderApps();
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    private void LoadManifestAndRender(bool sync)
    {
        if (_logger == null) return;
        string localRoot = PathProvider.ExpandPath(_settings.LocalRoot);
        var manifestService = new ManifestService();
        _manifest = manifestService.Load(_settings.ServerManifestPath);
        _logger.Info($"Manifest loaded. Version={_manifest.ManifestVersion}; Released={_manifest.Released}");

        var syncService = new AppSyncService(localRoot, _logger);
        var stateService = new LocalStateService(localRoot);
        _state = sync ? syncService.Sync(_manifest) : stateService.Load();
        RenderApps();
    }

    private void RenderApps()
    {
        _appsPanel.SuspendLayout();
        _appsPanel.Controls.Clear();
        _cardsByAppId.Clear();

        if (_manifest == null)
        {
            _appsPanel.ResumeLayout();
            return;
        }

        foreach (var app in _manifest.Applications.Where(a => a.Enabled).OrderBy(a => a.DisplayOrder).ThenBy(a => a.Title))
        {
            _state.Apps.TryGetValue(app.Id, out var local);
            var card = new AppCardControl(app, local);
            card.Width = Math.Max(540, _appsPanel.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 12);
            card.OpenRequested += (_, _) => LaunchApp(app, local);
            _appsPanel.Controls.Add(card);
            if (!string.IsNullOrWhiteSpace(app.Id))
                _cardsByAppId[app.Id] = card;
        }

        ResizeAppCards();
        _appsPanel.ResumeLayout();
    }

    private void OnSyncProgress(SyncProgressEvent progress)
    {
        _workingLabel.Text = progress.Message;
        _statusText.Text = progress.Message;

        if (progress.Total > 0)
        {
            _mainProgress.Maximum = Math.Max(progress.Total, 1);
            _mainProgress.Value = Math.Min(Math.Max(progress.Current, 0), _mainProgress.Maximum);
        }

        if (!string.IsNullOrWhiteSpace(progress.AppId) && _cardsByAppId.TryGetValue(progress.AppId, out var card))
            card.ApplySyncProgress(progress);

        AppendLog(progress.Message);
    }

    private void LaunchApp(ToolApplication app, LocalAppRecord? local)
    {
        try
        {
            if (_logger == null) return;
            _state.Apps.TryGetValue(app.Id, out var currentLocal);
            new AppLaunchService(_logger).Launch(app, currentLocal ?? local);
        }
        catch (Exception ex)
        {
            _logger?.Exception(ex, $"Launch failed for {app.Title}");
            MessageBox.Show(this, ex.Message, "Launch Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ShowChangePassword()
    {
        if (_currentUser == null)
        {
            MessageBox.Show(this, "Log in before changing your password.", "IWC Tools Loader", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(_sqlConnectionString))
        {
            MessageBox.Show(this, "SQL connection settings are missing.", "IWC Tools Loader", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var dlg = new ChangePasswordForm(new UserAuthService(_sqlConnectionString), _currentUser);
        dlg.ShowDialog(this);
    }

    private void ShowSettings()
    {
        using var dlg = new SettingsForm(_settings);
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            _settings = dlg.Settings;
            _settingsService.Save(_settings);
            MessageBox.Show(this, "Settings saved. Click Check for Updates to reload from the new manifest location.", "IWC Tools Loader", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void SetBusy(bool busy, string text)
    {
        _refreshButton.Enabled = !busy;
        _settingsButton.Enabled = !busy;
        _changePasswordButton.Enabled = !busy;
        _statusText.Text = text;
        _workingLabel.Text = text;
        Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
        if (!busy && text.Contains("ready", StringComparison.OrdinalIgnoreCase))
            _mainProgress.Value = _mainProgress.Maximum > 0 ? _mainProgress.Maximum : 0;
    }

    private void AppendLog(string line)
    {
        _logBox.AppendText(line + Environment.NewLine);
    }
}
