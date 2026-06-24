using Microsoft.Data.SqlClient;
using IWCToolsLoader.Auth;
using IWCToolsLoader.Core;

namespace IWCToolsLoader.UI;

public sealed class LoginForm : Form
{
    private static readonly Color IwcBrown = Color.FromArgb(102, 50, 6);
    private static readonly Color IwcGold = Color.FromArgb(245, 206, 63);

    private readonly UserAuthService _authService;
    private readonly string _localRoot;
    private readonly TextBox _userName = new();
    private readonly TextBox _password = new();
    private readonly Label _status = new();
    private readonly Button _login = new();
    private readonly Button _cancel = new();
    
    public AuthenticatedUser? AuthenticatedUser { get; private set; }

    public LoginForm(UserAuthService authService, string localRoot)
    {
        _authService = authService;
        _localRoot = localRoot;
        InitializeComponent();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        _userName.Focus();
    }

    private void InitializeComponent()
    {
        Text = "IWC Tools Login";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(460, 300);
        BackColor = Color.White;

        var header = new Panel { Dock = DockStyle.Top, Height = 78, BackColor = IwcBrown };
        var title = new Label
        {
            Text = "Imperial Woodworking Company",
            Font = new Font("Baskerville Old Face", 17, FontStyle.Bold),
            ForeColor = IwcGold,
            BackColor = Color.Transparent,
            Location = new Point(20, 14),
            Size = new Size(400, 28)
        };
        var subtitle = new Label
        {
            Text = "IWC Tools secure login",
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            ForeColor = IwcGold,
            BackColor = Color.Transparent,
            Location = new Point(22, 45),
            Size = new Size(330, 20)
        };
        header.Controls.AddRange(new Control[] { title, subtitle });

        var lblUser = new Label { Text = "User login:", Location = new Point(44, 110), Size = new Size(95, 22) };
        _userName.Location = new Point(150, 107);
        _userName.Size = new Size(250, 24);
        _userName.AutoCompleteMode = AutoCompleteMode.None;
        _userName.AutoCompleteSource = AutoCompleteSource.None;

        var lblPassword = new Label { Text = "Password:", Location = new Point(44, 148), Size = new Size(95, 22) };
        _password.Location = new Point(150, 145);
        _password.Size = new Size(250, 24);
        _password.UseSystemPasswordChar = true;

        _status.Text = "Enter your IWC user login and password.";
        _status.Location = new Point(44, 184);
        _status.Size = new Size(360, 34);
        _status.ForeColor = Color.FromArgb(80, 80, 80);

        _login.Text = "Log In";
        _login.Location = new Point(210, 232);
        _login.Size = new Size(90, 32);
        _login.Click += (_, _) => AttemptLogin();

        _cancel.Text = "Cancel";
        _cancel.Location = new Point(310, 232);
        _cancel.Size = new Size(90, 32);
        _cancel.DialogResult = DialogResult.Cancel;

        AcceptButton = _login;
        CancelButton = _cancel;
        Controls.AddRange(new Control[] { header, lblUser, _userName, lblPassword, _password, _status, _login, _cancel });
    }

    private void HandleInactiveUser(LoginUserRecord user)
    {
        if (user.ShouldRemoveLocalAppsOnLoginAttempt)
        {
            LocalApplicationCleanupResult cleanup = LocalApplicationCleanupService.DeleteInstalledApplications(_localRoot);

            string message = "You no longer have access to IWC Project tools." +
                             "\r\n\r\nLocal IWC application copies have been removed from this workstation.";

            if (cleanup.HasErrors)
            {
                message += "\r\n\r\nSome files could not be removed. Close any running IWC applications and contact your administrator if the issue continues." +
                           "\r\n\r\n" + string.Join("\r\n", cleanup.Errors);
            }

            MessageBox.Show(this, message, "IWC Tools Access Removed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            DialogResult = DialogResult.Cancel;
            Close();
            return;
        }

        _status.Text = "This user account is not active. Contact your IWC administrator.";
        _password.Clear();
        _userName.Focus();
        _userName.SelectAll();
    }

    private void AttemptLogin()
    {
        string userName = _userName.Text.Trim();
        if (string.IsNullOrWhiteSpace(userName))
        {
            MessageBox.Show(this, "Enter your user login.", "IWC Tools Loader", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _userName.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(_password.Text))
        {
            MessageBox.Show(this, "Enter your password.", "IWC Tools Loader", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _password.Focus();
            return;
        }

        try
        {
            LoginUserRecord? user = _authService.FindUserByLogin(userName);
            if (user == null)
            {
                _status.Text = "User login was not found.";
                _password.Clear();
                _userName.Focus();
                _userName.SelectAll();
                return;
            }

            if (!user.IsActive)
            {
                HandleInactiveUser(user);
                return;
            }

            if (!_authService.ValidatePassword(user, _password.Text, out bool requiresPasswordSetup))
            {
                _status.Text = requiresPasswordSetup
                    ? "Password does not match the employee ID on file."
                    : "Password is incorrect.";
                _password.Clear();
                _password.Focus();
                return;
            }

            if (requiresPasswordSetup)
            {
                using var passwordForm = new SetPasswordForm(string.IsNullOrWhiteSpace(user.UserName) ? user.UserLogin : user.UserName);
                if (passwordForm.ShowDialog(this) != DialogResult.OK)
                {
                    _status.Text = "Password setup is required before continuing.";
                    return;
                }

                _authService.SetPassword(user.Id, passwordForm.NewPassword);
            }

            AuthenticatedUser = new AuthenticatedUser
            {
                Id = user.Id,
                UserName = user.UserName,
                UserLogin = user.UserLogin,
                UserEmployeeId = user.UserEmployeeId
            };

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (ArgumentException ex) when (ex.Message.Contains("initialization string", StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show(this,
                "The IWCProj SQL settings are not configured correctly. Open Settings from the main loader screen and enter the database server, SQL username, and SQL password.\r\n\r\nDefault server:\r\niwcprojectportal.database.windows.net\r\n\r\nDetails: " + ex.Message,
                "Login Failed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        catch (SqlException ex) when (ex.Message.Contains("Windows logins are not supported", StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show(this,
                "This workstation is trying to use Windows authentication for Azure SQL. Open Settings and enter the SQL username and SQL password for the IWCProj connection.\r\n\r\nWindows login is not supported for this SQL Server.",
                "Login Failed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
