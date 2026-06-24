/*
IWC Tools Loader password support for dbo.Mng_Users.
Run only if these columns are not already present.
*/
IF COL_LENGTH('dbo.Mng_Users', 'PasswordHash') IS NULL
BEGIN
    ALTER TABLE dbo.Mng_Users ADD PasswordHash varbinary(64) NULL;
END;

IF COL_LENGTH('dbo.Mng_Users', 'PasswordSalt') IS NULL
BEGIN
    ALTER TABLE dbo.Mng_Users ADD PasswordSalt varbinary(32) NULL;
END;
GO

/* Useful index for active-user typed username login. */
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_Mng_Users_UserStatus_UserName'
      AND object_id = OBJECT_ID('dbo.Mng_Users')
)
BEGIN
    CREATE INDEX IX_Mng_Users_UserStatus_UserName
    ON dbo.Mng_Users(UserStatus, UserName)
    INCLUDE (UserEmployeeID);
END;
GO

/*
Optional administrator reset for a forgotten password.
After this reset, the user can log in once with UserEmployeeID and will be prompted to set a new password.

UPDATE dbo.Mng_Users
SET PasswordHash = NULL,
    PasswordSalt = NULL
WHERE UserName = 'USER NAME HERE';
*/

/*
Defensive access cleanup behavior used by IWCToolsLoader:
- Login is based on dbo.Mng_Users.UserLogin.
- Active users require UserStatus = 'active'.
- If UserStatus = 'inactive' and UserProjAdminAccess = 0, the loader denies access and removes locally cached IWC applications from the workstation.

Example admin disable:
UPDATE dbo.Mng_Users
SET UserStatus = 'inactive',
    UserProjAdminAccess = 0
WHERE UserLogin = 'userlogin';
*/
