# IWCLoader Solution

This solution contains the IWC desktop tools launcher and its self-update helper.

## Projects

- `IWCToolsLoader` - user-facing WinForms dashboard. Users launch this executable.
- `IWCToolsLoaderUpdater` - small helper executable used only to replace/update `IWCToolsLoader` while the main loader is closed.

## Startup workflow

1. User starts `IWCToolsLoader.exe`.
2. Before the login form is shown, the loader reads the separate loader manifest:

   `\\IWCMAIN1\DRAFTING\LIBRARY\App\IWC Desktop Tools\IWCToolsLoader\loader-manifest.json`

3. If the loader manifest version is newer than the installed loader version, the loader stages the new loader files locally, starts `IWCToolsLoaderUpdater.exe`, and exits.
4. The updater waits for the old loader process to close, copies the staged files over the installed loader folder, writes `Version.txt`, and restarts `IWCToolsLoader.exe`.
5. After the loader is current, the login form is shown.
6. After login, the normal dashboard application manifest is read and application updates are copied.

## Why two manifests?

Use two separate manifests:

- `loader-manifest.json` controls only the launcher/updater itself.
- `desktop-tools-manifest.json` controls dashboard apps such as `IWCProjectTools`, `IWCSampleLabels`, `IWCDoorTools`, and future tools.

This keeps loader/security fixes independent from normal application releases.

## Recommended local install folder

Install the loader itself here:

```text
%LOCALAPPDATA%\ImperialWoodworking\IWCToolsLoader\
```

Example:

```text
C:\Users\mattr\AppData\Local\ImperialWoodworking\IWCToolsLoader\
├── IWCToolsLoader.exe
├── IWCToolsLoaderUpdater.exe
├── Version.txt
└── other runtime files
```

The dashboard applications remain here:

```text
%LOCALAPPDATA%\ImperialWoodworking\IWCTools\Apps\<ApplicationId>\<Version>\
```

## Loader manifest deployment

Server file:

```text
\\IWCMAIN1\DRAFTING\LIBRARY\App\IWC Desktop Tools\IWCToolsLoader\loader-manifest.json
```

Sample included:

```text
Manifests\loader-manifest.sample.json
```

The `source` folder in the loader manifest must contain a complete published loader folder, including both:

```text
IWCToolsLoader.exe
IWCToolsLoaderUpdater.exe
```

Example loader manifest:

```json
{
  "appId": "IWCToolsLoader",
  "title": "IWC Tools Loader",
  "version": "1.0.1",
  "released": "2026-06-18",
  "notes": "Self-update enabled loader release.",
  "sourceType": "folder",
  "source": "\\\\IWCMAIN1\\DRAFTING\\LIBRARY\\App\\IWC Desktop Tools\\IWCToolsLoader\\1.0.1",
  "entryExe": "IWCToolsLoader.exe",
  "updaterExe": "IWCToolsLoaderUpdater.exe",
  "required": true
}
```

Increment `version` whenever you publish a new loader build. The loader checks this before login.

## Dashboard app manifest deployment

Server file:

```text
\\IWCMAIN1\DRAFTING\LIBRARY\App\IWC Desktop Tools\desktop-tools-manifest.json
```

Sample included:

```text
Manifests\desktop-tools-manifest.sample.json
```

## Settings

The Settings dialog includes:

- Loader update manifest path
- Dashboard applications manifest path
- Local application root
- SQL server
- SQL database
- SQL username
- SQL password
- startup update options

Default SQL server:

```text
iwcprojectportal.database.windows.net
```

Default database:

```text
IWCProj
```

Login uses `dbo.Mng_Users.UserLogin` and displays the greeting from `dbo.Mng_Users.UserName`.

## Publishing order for a new loader version

1. Build/publish `IWCToolsLoader` and `IWCToolsLoaderUpdater`.
2. Copy the full published loader folder to the server under a new version folder, for example:

   `\\IWCMAIN1\DRAFTING\LIBRARY\App\IWC Desktop Tools\IWCToolsLoader\1.0.1`

3. Confirm the folder contains `IWCToolsLoader.exe` and `IWCToolsLoaderUpdater.exe`.
4. Update `loader-manifest.json` last and increment the version number.
5. Next user launch will update the loader before login.
# IWC Tools Loader - SQL Auth / First-Time Login Fix

This update prevents the loader from attempting Windows authentication against the Azure SQL database. The IWCProj SQL connection now requires the SQL server, database name, SQL username, and SQL password fields in Settings.

Default SQL server:

```text
iwcprojectportal.database.windows.net
```

Default database:

```text
IWCProj
```

The login workflow remains:

1. Admin creates or updates the user in `dbo.Mng_Users` and sets `UserLogin`.
2. User enters `UserLogin` on the login form.
3. If `PasswordHash` and `PasswordSalt` are blank, the user enters `UserEmployeeID` as the temporary password.
4. On successful temporary login, the user is prompted to set a new password.
5. The new password is saved into `PasswordHash` and `PasswordSalt`.
6. Future logins use the user-created password.

The app still uses `UserName` only for display/greeting text.
