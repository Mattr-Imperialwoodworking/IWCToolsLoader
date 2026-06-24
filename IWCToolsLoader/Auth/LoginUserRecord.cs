namespace IWCToolsLoader.Auth;

public sealed class LoginUserRecord
{
    public int Id { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string UserLogin { get; init; } = string.Empty;
    public int? UserEmployeeId { get; init; }
    public string UserStatus { get; init; } = string.Empty;
    public bool UserProjAdminAccess { get; init; }
    public byte[]? PasswordHash { get; init; }
    public byte[]? PasswordSalt { get; init; }

    public bool IsActive => string.Equals((UserStatus ?? string.Empty).Trim(), "active", StringComparison.OrdinalIgnoreCase);

    public bool ShouldRemoveLocalAppsOnLoginAttempt =>
        string.Equals((UserStatus ?? string.Empty).Trim(), "inactive", StringComparison.OrdinalIgnoreCase)
        && !UserProjAdminAccess;

    public override string ToString() => string.IsNullOrWhiteSpace(UserLogin) ? UserName : UserLogin;
}
