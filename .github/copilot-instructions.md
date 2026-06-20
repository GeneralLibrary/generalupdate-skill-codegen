# GeneralUpdate Auto-Update Integration Guide

> Comprehensive reference for integrating GeneralUpdate into .NET applications.
> ⚠️ Targeting **NuGet v10.5.0-beta.6** API (UpdateRequest + SetConfig + LaunchAsync, with SetSource/SetOption/Hooks/Strategy).

## Architecture
- Dual-process: Client (verification/download/IPC) + Upgrade (file replacement)
- 4 update scenes: None/UpgradeOnly/MainOnly/Both
- AppType: **enum** (Client=1, Upgrade=2, OssClient=3, OssUpgrade=4)
- IPC: Encrypted file (default)

## NuGet Package Rules
- Core only: `dotnet add package GeneralUpdate.Core`
- With Bowl: reference **only** `GeneralUpdate.Bowl` (the two conflict if referenced together)
- Differential: already embedded in Core, no extra package needed

## Bootstrap Setup (v10.5.0-beta.6)
```csharp
var config = new UpdateRequest
{
    UpdateUrl = "https://server/api",
    AppSecretKey = "your-key",
    MainAppName = "MyApp.exe",
    ClientVersion = "1.0.0.0",
    ProductId = "my-product-001",
    InstallPath = "."
};
await new GeneralUpdateBootstrap()
    .SetConfig(config)
    .AddListenerUpdateInfo((_, e) => { })
    .LaunchAsync();
```

Or using zero-config SetSource API:
```csharp
await new GeneralUpdateBootstrap()
    .SetSource("https://server/api", "your-key")
    .SetOption(Option.Silent, true)
    .Hooks<UnixPermissionHooks>()
    .LaunchAsync();
```

## Events (v10.5.0-beta.6)
- AddListenerUpdateInfo → UpdateInfoEventArgs (namespace: GeneralUpdate.Core.Download)
- AddListenerMultiDownloadStatistics → MultiDownloadStatisticsEventArgs
- AddListenerMultiDownloadCompleted → MultiDownloadCompletedEventArgs **(IsCompleted)**
- AddListenerMultiDownloadError → MultiDownloadErrorEventArgs
- AddListenerMultiAllDownloadCompleted → MultiAllDownloadCompletedEventArgs
- AddListenerException → ExceptionEventArgs (namespace: GeneralUpdate.Core.Event)
- AddListenerProgress → ProgressEventArgs (new in v10.5)

## Known Issues
- Upgrade not starting: Check UpgradeApp.exe exists in update/ dir
- Method not found: Align NuGet versions between Client and Upgrade
- Version wrong: Use 4-segment format
- Infinite loop: manifest writes back version
