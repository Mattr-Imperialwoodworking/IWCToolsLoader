using IWCToolsLoader.Branding;
using IWCToolsLoader.Core;
using IWCToolsLoader.Models;

namespace IWCToolsLoader.UI;

public sealed class AppCardControl : UserControl
{
    private readonly PictureBox _icon = new();
    private readonly Label _title = new();
    private readonly Label _version = new();
    private readonly Label _description = new();
    private readonly Label _statusBadge = new();
    private readonly ProgressBar _cardProgress = new();
    private readonly Button _openButton = new();

    public ToolApplication App { get; }
    public LocalAppRecord? LocalRecord { get; private set; }

    public event EventHandler? OpenRequested;

    public AppCardControl(ToolApplication app, LocalAppRecord? localRecord)
    {
        App = app;
        LocalRecord = localRecord;
        InitializeComponent();
        LoadData();
    }

    private void InitializeComponent()
    {
        BorderStyle = BorderStyle.FixedSingle;
        BackColor = IwcBrand.CardBackground;
        Width = 540;
        Height = 160;
        MinimumSize = new Size(540, 160);
        Margin = new Padding(8);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 3,
            BackColor = IwcBrand.CardBackground,
            Padding = new Padding(12),
            Margin = new Padding(0),
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 108f)); // icon
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));  // app text
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 72f));  // version
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 112f)); // open button
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 32f));

        _icon.Dock = DockStyle.Fill;
        _icon.Margin = new Padding(0, 2, 12, 2);
        _icon.SizeMode = PictureBoxSizeMode.Zoom;

        _title.Dock = DockStyle.Fill;
        _title.Margin = new Padding(0, 0, 8, 0);
        _title.Font = new Font(Font, FontStyle.Bold);
        _title.AutoEllipsis = true;
        _title.TextAlign = ContentAlignment.MiddleLeft;

        _version.Dock = DockStyle.Fill;
        _version.Margin = new Padding(0, 0, 8, 0);
        _version.TextAlign = ContentAlignment.MiddleRight;
        _version.ForeColor = IwcBrand.SecondaryText;
        _version.AutoEllipsis = true;

        _description.Dock = DockStyle.Fill;
        _description.Margin = new Padding(0, 2, 8, 2);
        _description.AutoEllipsis = true;
        _description.TextAlign = ContentAlignment.TopLeft;

        _statusBadge.Dock = DockStyle.Fill;
        _statusBadge.Margin = new Padding(0, 0, 8, 0);
        _statusBadge.TextAlign = ContentAlignment.MiddleLeft;
        _statusBadge.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
        _statusBadge.AutoEllipsis = true;

        _cardProgress.Dock = DockStyle.Fill;
        _cardProgress.Margin = new Padding(0, 8, 8, 8);
        _cardProgress.Style = ProgressBarStyle.Marquee;
        _cardProgress.MarqueeAnimationSpeed = 30;
        _cardProgress.Visible = false;

        _openButton.Dock = DockStyle.Fill;
        _openButton.Margin = new Padding(8, 42, 0, 42);
        _openButton.Text = "Open";
        _openButton.Click += (_, _) => OpenRequested?.Invoke(this, EventArgs.Empty);

        root.Controls.Add(_icon, 0, 0);
        root.SetRowSpan(_icon, 3);

        root.Controls.Add(_title, 1, 0);
        root.Controls.Add(_version, 2, 0);
        root.Controls.Add(_description, 1, 1);
        root.SetColumnSpan(_description, 2);

        root.Controls.Add(_statusBadge, 1, 2);
        root.Controls.Add(_cardProgress, 2, 2);

        root.Controls.Add(_openButton, 3, 0);
        root.SetRowSpan(_openButton, 3);

        Controls.Add(root);
    }

    private void LoadData()
    {
        _title.Text = App.Title;
        _version.Text = App.Version;
        _description.Text = App.Description;
        _openButton.Enabled = LocalRecord != null && File.Exists(LocalRecord.EntryExePath);
        _icon.Image = LoadIconImage();
        SetDefaultStatus();
    }

    public void UpdateLocalRecord(LocalAppRecord? localRecord)
    {
        LocalRecord = localRecord;
        _openButton.Enabled = LocalRecord != null && File.Exists(LocalRecord.EntryExePath);
        _icon.Image = LoadIconImage();
        SetDefaultStatus();
    }

    public void ApplySyncProgress(SyncProgressEvent progress)
    {
        if (!StringComparer.OrdinalIgnoreCase.Equals(progress.AppId, App.Id)) return;

        switch (progress.Kind)
        {
            case SyncProgressKind.Checking:
                SetStatus("Checking...", IwcBrand.StatusNeutral, showProgress: true, enableOpen: false);
                break;
            case SyncProgressKind.Installing:
                SetStatus("Installing update...", IwcBrand.StatusWarning, showProgress: true, enableOpen: false);
                break;
            case SyncProgressKind.Installed:
                SetStatus("Updated / installed", IwcBrand.StatusSuccess, showProgress: false, enableOpen: true);
                break;
            case SyncProgressKind.Current:
                SetStatus("Current", IwcBrand.StatusNeutral, showProgress: false, enableOpen: true);
                break;
            case SyncProgressKind.Failed:
                SetStatus("Update failed", IwcBrand.StatusError, showProgress: false, enableOpen: LocalRecord != null && File.Exists(LocalRecord.EntryExePath));
                break;
        }
    }

    private void SetDefaultStatus()
    {
        if (LocalRecord != null && File.Exists(LocalRecord.EntryExePath))
        {
            SetStatus($"Installed {LocalRecord.Version}", IwcBrand.StatusNeutral, showProgress: false, enableOpen: true);
        }
        else
        {
            SetStatus("Not installed", IwcBrand.StatusError, showProgress: false, enableOpen: false);
        }
    }

    private void SetStatus(string text, Color color, bool showProgress, bool enableOpen)
    {
        _statusBadge.Text = text;
        _statusBadge.ForeColor = color;
        _cardProgress.Visible = showProgress;
        _openButton.Enabled = enableOpen && LocalRecord != null && File.Exists(LocalRecord.EntryExePath);
    }

    private Image LoadIconImage()
    {
        try
        {
            string icon = App.Icon ?? string.Empty;
            string? iconPath = null;

            if (!string.IsNullOrWhiteSpace(icon))
            {
                if (Path.IsPathRooted(icon)) iconPath = Environment.ExpandEnvironmentVariables(icon);
                else if (LocalRecord != null) iconPath = Path.Combine(LocalRecord.LocalPath, icon);
            }

            if (!string.IsNullOrWhiteSpace(iconPath) && File.Exists(iconPath))
            {
                if (string.Equals(Path.GetExtension(iconPath), ".ico", StringComparison.OrdinalIgnoreCase))
                {
                    using var ico = new Icon(iconPath);
                    return ico.ToBitmap();
                }
                using var fs = new FileStream(iconPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var ms = new MemoryStream();
                fs.CopyTo(ms);
                return Image.FromStream(new MemoryStream(ms.ToArray()));
            }
        }
        catch
        {
            // Use fallback.
        }

        Bitmap bmp = new(96, 96);
        using var g = Graphics.FromImage(bmp);
        g.Clear(IwcBrand.CardBackground);
        using var pen = new Pen(IwcBrand.FallbackIconBorder, 3);
        using var brush = new SolidBrush(IwcBrand.FallbackIconFill);
        g.DrawRectangle(pen, 10, 10, 76, 76);
        g.FillRectangle(brush, 24, 24, 48, 48);
        return bmp;
    }
}
