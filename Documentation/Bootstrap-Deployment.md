# IWC Tools Bootstrap Deployment

## New startup model

The loader no longer attempts to replace itself while it is running. Users should start a small fixed executable named `IWCToolsBootstrap.exe`.

`IWCToolsBootstrap.exe` does only four things:

1. Read its local bootstrap config file.
2. Read the server `loader-manifest.json`.
3. Copy the loader release folder to a local versioned folder if that version is not already installed.
4. Start the `IWCToolsLoader.exe` from that versioned folder.

## Local install layout

Install the bootstrap executable here:

```text
%LOCALAPPDATA%\ImperialWoodworking\IWCToolsBootstrap\IWCToolsBootstrap.exe
%LOCALAPPDATA%\ImperialWoodworking\IWCToolsBootstrap\iwc-loader-bootstrap.json
```

The bootstrap creates and manages loader versions here:

```text
%LOCALAPPDATA%\ImperialWoodworking\IWCToolsLoader\Versions\<version>\IWCToolsLoader.exe
```

It also writes:

```text
%LOCALAPPDATA%\ImperialWoodworking\IWCToolsLoader\active-loader.json
%LOCALAPPDATA%\ImperialWoodworking\IWCToolsLoader\CurrentVersion.txt
```

## What users should launch

Users should launch only:

```text
IWCToolsBootstrap.exe
```

They should not launch `IWCToolsLoaderUpdater.exe`, and they do not need to know which version folder contains the current loader.

## Server manifest

The bootstrap reads:

```text
\\IWCMAIN1\DRAFTING\LIBRARY\App\IWC Desktop Tools\IWCToolsLoader\loader-manifest.json
```

Example:

```json
{
  "appId": "IWCToolsLoader",
  "title": "IWC Tools Loader",
  "version": "1.0.7",
  "released": "2026-06-23",
  "notes": "Bootstrap/versioned loader release.",
  "sourceType": "folder",
  "source": "\\\\IWCMAIN1\\DRAFTING\\LIBRARY\\App\\IWC Desktop Tools\\IWCToolsLoader\\1.0.7",
  "entryExe": "IWCToolsLoader.exe",
  "required": true
}
```

The `source` folder should be the published loader output folder and should contain `IWCToolsLoader.exe` at or below that folder. The bootstrap searches one level or deeper if the manifest accidentally points to the parent folder.

## Release process

1. Publish/build `IWCToolsLoader`.
2. Copy the full publish folder to the server under a new version folder.
3. Confirm the server version folder contains `IWCToolsLoader.exe`.
4. Update `loader-manifest.json` last.
5. Users launch `IWCToolsBootstrap.exe`.

## Why this replaces Version.txt self-update

The prior design had `IWCToolsLoader.exe` try to update itself. That created a fragile update loop because the running executable and its `Version.txt` could be locked or overwritten at the wrong time. The bootstrap design never overwrites the running loader. Each version is copied to its own folder and then run from there.
