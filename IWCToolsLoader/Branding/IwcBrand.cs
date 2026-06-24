using System.Drawing;

namespace IWCToolsLoader.Branding;

public static class IwcBrand
{
    // Central color palette for IWC Tools Loader.
    // Update these values to change the application branding globally.
    public static readonly Color HeaderBrown = Color.FromArgb(102, 50, 6);
    public static readonly Color Gold = Color.FromArgb(245, 206, 63);
    public static readonly Color BodyBackground = Color.White;
    public static readonly Color CardBackground = Color.White;
    public static readonly Color ControlBorder = Color.FromArgb(210, 210, 210);
    public static readonly Color MainText = Color.Black;
    public static readonly Color SecondaryText = Color.DimGray;
    public static readonly Color StatusText = Color.FromArgb(65, 65, 65);
    public static readonly Color LogBackground = Color.FromArgb(248, 248, 248);

    public static readonly Color StatusNeutral = Color.FromArgb(85, 85, 85);
    public static readonly Color StatusWarning = Color.FromArgb(180, 105, 0);
    public static readonly Color StatusSuccess = Color.FromArgb(34, 120, 60);
    public static readonly Color StatusError = Color.FromArgb(170, 30, 30);

    public static readonly Color FallbackIconFill = Gold;
    public static readonly Color FallbackIconBorder = HeaderBrown;
}
