using IWCToolsLoader.Models;

namespace IWCToolsLoader.UI;

public sealed class SettingsForm : Form
{
    private readonly TextBox _loaderManifestPath = new();
    private readonly TextBox _manifestPath = new();
    private readonly TextBox _localRoot = new();
    private readonly TextBox _sqlServer = new();
    private readonly TextBox _sqlDatabase = new();
    private readonly TextBox _sqlUser = new();
    private readonly TextBox _sqlPassword = new();
    private readonly CheckBox _autoUpdateLoader = new();
    private readonly CheckBox _autoSync = new();
    private readonly Button _ok = new();
    private readonly Button _cancel = new();

    public LoaderSettings Settings { get; private set; }

    public SettingsForm(LoaderSettings settings)
    {
        Settings = new LoaderSettings
        {
            ServerManifestPath = settings.ServerManifestPath,
            LoaderManifestPath = settings.LoaderManifestPath,
            LocalRoot = settings.LocalRoot,
            AutoSyncOnStartup = settings.AutoSyncOnStartup,
            AutoUpdateLoaderOnStartup = settings.AutoUpdateLoaderOnStartup,
            KeepOldVersions = settings.KeepOldVersions,
            SqlServer = settings.SqlServer,
            SqlDatabase = settings.SqlDatabase,
            SqlUserName = settings.SqlUserName,
            SqlPassword = settings.SqlPassword,
            SqlConnectionString = settings.SqlConnectionString
        };
        InitializeComponent();
        LoadData();
    }

    private void InitializeComponent()
    {
        Text = "IWC Tools Loader Settings";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Size = new Size(780, 560);
        BackColor = Color.White;

        var lblLoaderManifest = new Label { Text = "Loader update manifest path", Location = new Point(18, 20), Size = new Size(230, 22) };
        _loaderManifestPath.Location = new Point(18, 44);
        _loaderManifestPath.Size = new Size(720, 24);

        var lblManifest = new Label { Text = "Dashboard applications manifest path", Location = new Point(18, 82), Size = new Size(250, 22) };
        _manifestPath.Location = new Point(18, 106);
        _manifestPath.Size = new Size(720, 24);

        var lblLocal = new Label { Text = "Local application root", Location = new Point(18, 144), Size = new Size(180, 22) };
        _localRoot.Location = new Point(18, 168);
        _localRoot.Size = new Size(720, 24);

        _autoUpdateLoader.Text = "Check for IWC Tools Loader updates before login";
        _autoUpdateLoader.Location = new Point(18, 202);
        _autoUpdateLoader.Size = new Size(360, 24);

        _autoSync.Text = "Check dashboard applications for updates after login";
        _autoSync.Location = new Point(18, 230);
        _autoSync.Size = new Size(380, 24);

        var sqlHeader = new Label
        {
            Text = "IWCProj SQL settings",
            Location = new Point(18, 270),
            Size = new Size(220, 22),
            Font = new Font("Segoe UI", 9f, FontStyle.Bold)
        };

        var lblSqlServer = new Label { Text = "Database server", Location = new Point(18, 302), Size = new Size(140, 22) };
        _sqlServer.Location = new Point(160, 299);
        _sqlServer.Size = new Size(578, 24);

        var lblSqlDatabase = new Label { Text = "Database", Location = new Point(18, 336), Size = new Size(140, 22) };
        _sqlDatabase.Location = new Point(160, 333);
        _sqlDatabase.Size = new Size(220, 24);

        var lblSqlUser = new Label { Text = "SQL username", Location = new Point(18, 370), Size = new Size(140, 22) };
        _sqlUser.Location = new Point(160, 367);
        _sqlUser.Size = new Size(220, 24);

        var lblSqlPassword = new Label { Text = "SQL password", Location = new Point(18, 404), Size = new Size(140, 22) };
        _sqlPassword.Location = new Point(160, 401);
        _sqlPassword.Size = new Size(220, 24);
        _sqlPassword.UseSystemPasswordChar = true;

        var note = new Label
        {
            Text = "SQL username and password are required for the Azure SQL IWCProj connection.",
            Location = new Point(160, 430),
            Size = new Size(540, 22),
            ForeColor = Color.FromArgb(90, 90, 90)
        };

        _ok.Text = "OK";
        _ok.Location = new Point(566, 482);
        _ok.Size = new Size(80, 30);
        _ok.Click += (_, _) => SaveAndClose();

        _cancel.Text = "Cancel";
        _cancel.Location = new Point(658, 482);
        _cancel.Size = new Size(80, 30);
        _cancel.Click += (_, _) => DialogResult = DialogResult.Cancel;

        Controls.AddRange(new Control[]
        {
            lblLoaderManifest, _loaderManifestPath,
            lblManifest, _manifestPath,
            lblLocal, _localRoot,
            _autoUpdateLoader, _autoSync,
            sqlHeader,
            lblSqlServer, _sqlServer,
            lblSqlDatabase, _sqlDatabase,
            lblSqlUser, _sqlUser,
            lblSqlPassword, _sqlPassword,
            note,
            _ok, _cancel
        });
    }

    private void LoadData()
    {
        _loaderManifestPath.Text = Settings.LoaderManifestPath;
        _manifestPath.Text = Settings.ServerManifestPath;
        _localRoot.Text = Settings.LocalRoot;
        _autoUpdateLoader.Checked = Settings.AutoUpdateLoaderOnStartup;
        _autoSync.Checked = Settings.AutoSyncOnStartup;
        _sqlServer.Text = string.IsNullOrWhiteSpace(Settings.SqlServer) ? "iwcprojectportal.database.windows.net" : Settings.SqlServer;
        _sqlDatabase.Text = string.IsNullOrWhiteSpace(Settings.SqlDatabase) ? "IWCProj" : Settings.SqlDatabase;
        _sqlUser.Text = Settings.SqlUserName;
        _sqlPassword.Text = Settings.SqlPassword;
    }

    private void SaveAndClose()
    {
        if (string.IsNullOrWhiteSpace(_sqlServer.Text))
        {
            MessageBox.Show(this, "Enter the database server. Default: iwcprojectportal.database.windows.net", "IWC Tools Loader Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _sqlServer.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(_sqlDatabase.Text))
        {
            MessageBox.Show(this, "Enter the database name. Default: IWCProj", "IWC Tools Loader Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _sqlDatabase.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(_sqlUser.Text))
        {
            MessageBox.Show(this, "Enter the SQL username. Windows authentication is not supported for this Azure SQL connection.", "IWC Tools Loader Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _sqlUser.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(_sqlPassword.Text))
        {
            MessageBox.Show(this, "Enter the SQL password.", "IWC Tools Loader Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _sqlPassword.Focus();
            return;
        }

        Settings.LoaderManifestPath = _loaderManifestPath.Text.Trim();
        Settings.ServerManifestPath = _manifestPath.Text.Trim();
        Settings.LocalRoot = _localRoot.Text.Trim();
        Settings.AutoUpdateLoaderOnStartup = _autoUpdateLoader.Checked;
        Settings.AutoSyncOnStartup = _autoSync.Checked;
        Settings.SqlServer = _sqlServer.Text.Trim();
        Settings.SqlDatabase = string.IsNullOrWhiteSpace(_sqlDatabase.Text) ? "IWCProj" : _sqlDatabase.Text.Trim();
        Settings.SqlUserName = _sqlUser.Text.Trim();
        Settings.SqlPassword = _sqlPassword.Text;
        DialogResult = DialogResult.OK;
    }
}
