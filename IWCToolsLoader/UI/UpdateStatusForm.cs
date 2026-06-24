namespace IWCToolsLoader.UI;

public sealed class UpdateStatusForm : Form
{
    private static readonly Color IwcBrown = Color.FromArgb(102, 50, 6);
    private static readonly Color IwcGold = Color.FromArgb(245, 206, 63);

    private readonly Panel _headerPanel = new();
    private readonly Label _title = new();
    private readonly Label _message = new();
    private readonly ProgressBar _progress = new();

    public UpdateStatusForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "IWC Tools Loader Update";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Size = new Size(520, 210);
        BackColor = Color.White;

        _headerPanel.Dock = DockStyle.Top;
        _headerPanel.Height = 72;
        _headerPanel.BackColor = IwcBrown;

        _title.Text = "Imperial Woodworking Company";
        _title.Font = new Font("Baskerville Old Face", 17f, FontStyle.Bold);
        _title.ForeColor = IwcGold;
        _title.BackColor = Color.Transparent;
        _title.Location = new Point(20, 18);
        _title.Size = new Size(460, 30);
        _headerPanel.Controls.Add(_title);

        _message.Text = "Checking for IWC Tools Loader updates...";
        _message.Location = new Point(22, 92);
        _message.Size = new Size(460, 26);
        _message.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);

        _progress.Location = new Point(22, 126);
        _progress.Size = new Size(460, 16);
        _progress.Style = ProgressBarStyle.Marquee;
        _progress.MarqueeAnimationSpeed = 30;

        Controls.AddRange(new Control[] { _headerPanel, _message, _progress });
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
