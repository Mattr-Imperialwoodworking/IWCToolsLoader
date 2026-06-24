using Microsoft.Data.SqlClient;
using IWCToolsLoader.Models;

namespace IWCToolsLoader.Core;

public static class ConnectionStringValidator
{
    public static bool TryValidate(LoaderSettings settings, out string message, out string connectionString)
    {
        connectionString = SqlConnectionFactory.BuildConnectionString(settings);
        return TryValidate(settings, connectionString, out message);
    }

    public static bool TryValidate(LoaderSettings settings, string? connectionString, out string message)
    {
        if (settings == null)
        {
            message = "The IWCProj SQL settings have not been configured.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(settings.SqlServer))
        {
            message = "The SQL settings are missing the database server value. Default: iwcprojectportal.database.windows.net";
            return false;
        }

        if (string.IsNullOrWhiteSpace(settings.SqlDatabase))
        {
            message = "The SQL settings are missing the database name. It should be IWCProj.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(settings.SqlUserName))
        {
            message = "The SQL settings are missing the SQL username. Azure SQL does not support Windows login for this application.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(settings.SqlPassword))
        {
            message = "The SQL settings are missing the SQL password. Enter the SQL password for the configured SQL username.";
            return false;
        }

        return TryValidate(connectionString, out message);
    }

    public static bool TryValidate(string? connectionString, out string message)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            message = "The IWCProj SQL settings have not been configured.";
            return false;
        }

        try
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            if (string.IsNullOrWhiteSpace(builder.DataSource))
            {
                message = "The SQL settings are missing the database server value.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(builder.InitialCatalog))
            {
                message = "The SQL settings are missing the database name. It should point to the IWCProj database.";
                return false;
            }

            if (builder.IntegratedSecurity)
            {
                message = "Windows authentication is not supported for this Azure SQL connection. Enter the SQL username and SQL password in Settings.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(builder.UserID))
            {
                message = "The SQL settings are missing the SQL username.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(builder.Password))
            {
                message = "The SQL settings are missing the SQL password.";
                return false;
            }

            message = string.Empty;
            return true;
        }
        catch (ArgumentException ex)
        {
            message = "The IWCProj SQL settings could not be converted to a valid connection string. Check the server, database, SQL username, and SQL password fields.\r\n\r\nDetails: " + ex.Message;
            return false;
        }
    }
}
