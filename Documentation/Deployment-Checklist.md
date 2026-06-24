# IWCToolsLoader Deployment Checklist

## One-time workstation install

1. Build `IWCToolsLoader.exe` in Release mode.
2. Install or copy the loader to a stable workstation location, for example:

```text
C:\Program Files\Imperial Woodworking\IWCToolsLoader\IWCToolsLoader.exe
```

or for a non-admin install:

```text
%LOCALAPPDATA%\ImperialWoodworking\IWCToolsLoader\IWCToolsLoader.exe
```

3. Create a Start Menu shortcut or desktop shortcut to the loader.
4. Launch once and confirm it creates:

```text
%APPDATA%\ImperialWoodworking\IWCToolsLoader\loader-settings.json
```

## Server setup

1. Create the server folder:

```text
\\IWCMAIN1\DRAFTING\LIBRARY\App\IWC Desktop Tools
```

2. Copy `desktop-tools-manifest.sample.json` to this folder and rename it:

```text
desktop-tools-manifest.json
```

3. Create one folder per app, with one folder per version.
4. Update each manifest entry with the correct source folder, version, entry EXE, icon, title, and description.

## Updating an application

1. Publish the updated desktop app.
2. Copy it to a new server folder, for example:

```text
\\IWCMAIN1\DRAFTING\LIBRARY\App\IWC Desktop Tools\IWCDoorTools\1.0.1
```

3. Update only the application version and source path in the manifest:

```json
"version": "1.0.1",
"source": "\\\\IWCMAIN1\\DRAFTING\\LIBRARY\\App\\IWC Desktop Tools\\IWCDoorTools\\1.0.1"
```

4. Save the manifest last, after the server copy is complete.

## Local cache location

Recommended:

```text
%LOCALAPPDATA%\ImperialWoodworking\IWCTools\Apps
```

This avoids admin permissions and avoids network file locking.
