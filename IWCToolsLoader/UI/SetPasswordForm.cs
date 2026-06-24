namespace IWCToolsLoader.UI;

public sealed class SetPasswordForm : Form
{
    private readonly TextBox _password = new();
    private readonly TextBox _confirm = new();
    private readonly Label _message = new();
    private readonly Button _ok = new();
    private readonly Button _cancel = new();

    public string NewPassword => _password.Text;

    public SetPasswordForm(string userName)
    {
        InitializeComponent(userName);
    }

    private void InitializeComponent(string userName)
    {
        Text = "Set IWC Tools Password";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(430, 210);
        BackColor = Color.White;

        _message.Text = $"Welcome {userName}. Please set your IWC Tools password.";
        _message.Location = new Point(18, 16);
        _message.Size = new Size(390, 42);
        _message.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);

        var lblPass = new Label { Text = "New password:", Location = new Point(18, 72), Size = new Size(120, 22) };
        _password.Location = new Point(145, 70);
        _password.Size = new Size(240, 24);
        _password.UseSystemPasswordChar = true;

        var lblConfirm = new Label { Text = "Confirm password:", Location = new Point(18, 108), Size = new Size(120, 22) };
        _confirm.Location = new Point(145, 106);
        _confirm.Size = new Size(240, 24);
        _confirm.UseSystemPasswordChar = true;

        _ok.Text = "Save Password";
        _ok.Location = new Point(170, 156);
        _ok.Size = new Size(112, 30);
        _ok.Click += (_, _) => ValidateAndClose();

        _cancel.Text = "Cancel";
        _cancel.Location = new Point(292, 156);
        _cancel.Size = new Size(90, 30);
        _cancel.DialogResult = DialogResult.Cancel;

        AcceptButton = _ok;
        CancelButton = _cancel;
        Controls.AddRange(new Control[] { _message, lblPass, _password, lblConfirm, _confirm, _ok, _cancel });
    }

    private void ValidateAndClose()
    {
        if (_password.Text.Length < 6)
        {
            MessageBox.Show(this, "Password must be at least 6 characters.", "IWC Tools Loader", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!string.Equals(_password.Text, _confirm.Text, StringComparison.Ordinal))
        {
            MessageBox.Show(this, "Passwords do not match.", "IWC Tools Loader", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }
}
