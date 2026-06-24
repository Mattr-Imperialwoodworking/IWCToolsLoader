using Microsoft.Data.SqlClient;
using IWCToolsLoader.Models;

namespace IWCToolsLoader.Core;

public static class SqlConnectionFactory
{
    public static string BuildConnectionString(LoaderSettings settings)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = string.IsNullOrWhiteSpace(settings.SqlServer)
                ? "iwcprojectportal.database.windows.net"
                : settings.SqlServer.Trim(),
            InitialCatalog = string.IsNullOrWhiteSpace(settings.SqlDatabase)
                ? "IWCProj"
                : settings.SqlDatabase.Trim(),
            UserID = settings.SqlUserName?.Trim() ?? string.Empty,
            Password = settings.SqlPassword ?? string.Empty,
            IntegratedSecurity = false,
            TrustServerCertificate = true,
            Encrypt = true
        };

        return builder.ConnectionString;
    }
}
