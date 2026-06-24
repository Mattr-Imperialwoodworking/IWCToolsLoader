using IWCToolsLoader.Auth;

namespace IWCToolsLoader.UI;

public sealed class ChangePasswordForm : Form
{
    private readonly UserAuthService _authService;
    private readonly AuthenticatedUser _user;
    private readonly TextBox _currentPassword = new();
    private readonly TextBox _newPassword = new();
    private readonly TextBox _confirmPassword = new();
    private readonly Label _message = new();
    private readonly Button _save = new();
    private readonly Button _cancel = new();

    public ChangePasswordForm(UserAuthService authService, AuthenticatedUser user)
    {
        _authService = authService;
        _user = user;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "Change IWC Tools Password";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(470, 275);
        BackColor = Color.White;

        _message.Text = $"Change password for {(string.IsNullOrWhiteSpace(_user.UserName) ? _user.UserLogin : _user.UserName)}.";
        _message.Location = new Point(22, 18);
        _message.Size = new Size(420, 32);
        _message.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);

        var lblCurrent = new Label { Text = "Current password:", Location = new Point(22, 68), Size = new Size(135, 22) };
        _currentPassword.Location = new Point(170, 65);
        _currentPassword.Size = new Size(250, 24);
        _currentPassword.UseSystemPasswordChar = true;

        var lblNew = new Label { Text = "New password:", Location = new Point(22, 110), Size = new Size(135, 22) };
        _newPassword.Location = new Point(170, 107);
        _newPassword.Size = new Size(250, 24);
        _newPassword.UseSystemPasswordChar = true;

        var lblConfirm = new Label { Text = "Confirm password:", Location = new Point(22, 152), Size = new Size(135, 22) };
        _confirmPassword.Location = new Point(170, 149);
        _confirmPassword.Size = new Size(250, 24);
        _confirmPassword.UseSystemPasswordChar = true;

        _save.Text = "Save Password";
        _save.Location = new Point(210, 210);
        _save.Size = new Size(120, 32);
        _save.Click += (_, _) => SavePassword();

        _cancel.Text = "Cancel";
        _cancel.Location = new Point(340, 210);
        _cancel.Size = new Size(80, 32);
        _cancel.DialogResult = DialogResult.Cancel;

        AcceptButton = _save;
        CancelButton = _cancel;
        Controls.AddRange(new Control[]
        {
            _message, lblCurrent, _currentPassword, lblNew, _newPassword, lblConfirm, _confirmPassword, _save, _cancel
        });
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        _currentPassword.Focus();
    }

    private void SavePassword()
    {
        if (string.IsNullOrWhiteSpace(_currentPassword.Text))
        {
            MessageBox.Show(this, "Enter your current password.", "IWC Tools Loader", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _currentPassword.Focus();
            return;
        }

        if (_newPassword.Text.Length < 6)
        {
            MessageBox.Show(this, "New password must be at least 6 characters.", "IWC Tools Loader", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _newPassword.Focus();
            return;
        }

        if (!string.Equals(_newPassword.Text, _confirmPassword.Text, StringComparison.Ordinal))
        {
            MessageBox.Show(this, "New passwords do not match.", "IWC Tools Loader", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _confirmPassword.Focus();
            return;
        }

        if (string.Equals(_currentPassword.Text, _newPassword.Text, StringComparison.Ordinal))
        {
            MessageBox.Show(this, "New password must be different from the current password.", "IWC Tools Loader", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _newPassword.Focus();
            return;
        }

        try
        {
            bool changed = _authService.ChangePassword(_user.Id, _currentPassword.Text, _newPassword.Text, out string message);
            if (!changed)
            {
                MessageBox.Show(this, message, "Password Not Changed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _currentPassword.Clear();
                _currentPassword.Focus();
                return;
            }

            MessageBox.Show(this, "Your password has been updated.", "IWC Tools Loader", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Password Change Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
