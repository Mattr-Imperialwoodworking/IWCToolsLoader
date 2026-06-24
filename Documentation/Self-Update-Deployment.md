# IWCToolsLoader Self-Update Deployment Notes

The loader checks for its own updates before login. This is intentional so security, SQL login, dashboard, or manifest logic can be fixed before a user authenticates.

## Required files in each loader release folder

Each server release folder should contain the full published output for both projects:

- IWCToolsLoader.exe
- IWCToolsLoaderUpdater.exe
- IWCToolsLoader.dll and related runtime files, if produced by publish
- IWCToolsLoaderUpdater.dll and related runtime files, if produced by publish
- Version.txt, optional but recommended

## Required field behavior

If `required` is true in `loader-manifest.json` and the update cannot be staged, startup is stopped. If `required` is false, the installed loader may continue.

## Locked file behavior

`IWCToolsLoader.exe` cannot replace itself while running. The loader stages the update and runs `IWCToolsLoaderUpdater.exe` from the staged folder. The updater waits for the running loader to exit, then copies files over the installed loader directory and restarts the loader.
