using System.Security.Cryptography;

namespace IWCToolsLoader.Auth;

public static class PasswordHasher
{
    private const int SaltSizeBytes = 32;
    private const int HashSizeBytes = 64;
    private const int Iterations = 120_000;

    public static (byte[] Hash, byte[] Salt) HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be blank.", nameof(password));

        byte[] salt = RandomNumberGenerator.GetBytes(SaltSizeBytes);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSizeBytes);
        return (hash, salt);
    }

    public static bool Verify(string password, byte[] expectedHash, byte[] salt)
    {
        if (string.IsNullOrEmpty(password) || expectedHash.Length == 0 || salt.Length == 0)
            return false;

        byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, expectedHash.Length);
        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}
