namespace IWCToolsLoader.Auth;

public sealed class AuthenticatedUser
{
    public int Id { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string UserLogin { get; init; } = string.Empty;
    public int? UserEmployeeId { get; init; }
    public string FirstName
    {
        get
        {
            var value = (UserName ?? string.Empty).Trim();
            if (value.Length == 0) return string.IsNullOrWhiteSpace(UserLogin) ? "User" : UserLogin.Trim();
            var parts = value.Split(new[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 0 ? parts[0] : value;
        }
    }
}
