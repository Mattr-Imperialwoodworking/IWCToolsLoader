using Microsoft.Data.SqlClient;

namespace IWCToolsLoader.Auth;

public sealed class UserAuthService
{
    private readonly string _connectionString;

    public UserAuthService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public LoginUserRecord? FindUserByLogin(string userLogin)
    {
        if (string.IsNullOrWhiteSpace(userLogin))
            return null;

        using var conn = new SqlConnection(_connectionString);
        conn.Open();

        const string sql = @"
SELECT TOP (1)
    ID,
    UserName,
    UserLogin,
    UserEmployeeID,
    UserStatus,
    ISNULL(UserProjAdminAccess, 0) AS UserProjAdminAccess,
    PasswordHash,
    PasswordSalt
FROM dbo.Mng_Users
WHERE LOWER(LTRIM(RTRIM(UserLogin))) = LOWER(LTRIM(RTRIM(@UserLogin)));";

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@UserLogin", userLogin.Trim());

        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
            return null;

        return ReadUser(reader);
    }

    public LoginUserRecord? FindActiveUserByLogin(string userLogin)
    {
        LoginUserRecord? user = FindUserByLogin(userLogin);
        return user is { IsActive: true } ? user : null;
    }

    public bool ValidatePassword(LoginUserRecord user, string password, out bool requiresPasswordSetup)
    {
        requiresPasswordSetup = false;

        if (user.PasswordHash is { Length: > 0 } hash && user.PasswordSalt is { Length: > 0 } salt)
            return PasswordHasher.Verify(password, hash, salt);

        requiresPasswordSetup = true;
        return user.UserEmployeeId.HasValue && string.Equals(password, user.UserEmployeeId.Value.ToString(), StringComparison.Ordinal);
    }

    public void SetPassword(int userId, string newPassword)
    {
        var (hash, salt) = PasswordHasher.HashPassword(newPassword);

        using var conn = new SqlConnection(_connectionString);
        conn.Open();

        const string sql = @"
UPDATE dbo.Mng_Users
SET PasswordHash = @PasswordHash,
    PasswordSalt = @PasswordSalt
WHERE ID = @ID;";

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@PasswordHash", hash);
        cmd.Parameters.AddWithValue("@PasswordSalt", salt);
        cmd.Parameters.AddWithValue("@ID", userId);
        cmd.ExecuteNonQuery();
    }

    public bool ChangePassword(int userId, string currentPassword, string newPassword, out string message)
    {
        message = string.Empty;

        LoginUserRecord? user = FindActiveUserById(userId);
        if (user == null)
        {
            message = "The user account was not found or is no longer active.";
            return false;
        }

        if (!ValidatePassword(user, currentPassword, out _))
        {
            message = "Current password is incorrect.";
            return false;
        }

        SetPassword(userId, newPassword);
        message = "Password updated.";
        return true;
    }

    public LoginUserRecord? FindActiveUserById(int userId)
    {
        using var conn = new SqlConnection(_connectionString);
        conn.Open();

        const string sql = @"
SELECT TOP (1)
    ID,
    UserName,
    UserLogin,
    UserEmployeeID,
    UserStatus,
    ISNULL(UserProjAdminAccess, 0) AS UserProjAdminAccess,
    PasswordHash,
    PasswordSalt
FROM dbo.Mng_Users
WHERE LOWER(LTRIM(RTRIM(ISNULL(UserStatus, '')))) = 'active'
  AND ID = @ID;";

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@ID", userId);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
            return null;

        return ReadUser(reader);
    }

    private static LoginUserRecord ReadUser(SqlDataReader reader)
    {
        return new LoginUserRecord
        {
            Id = reader.GetInt32(0),
            UserName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
            UserLogin = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
            UserEmployeeId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
            UserStatus = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
            UserProjAdminAccess = !reader.IsDBNull(5) && reader.GetBoolean(5),
            PasswordHash = reader.IsDBNull(6) ? null : (byte[])reader[6],
            PasswordSalt = reader.IsDBNull(7) ? null : (byte[])reader[7]
        };
    }
}
