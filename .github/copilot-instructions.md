# GeneralUpdate Auto-Update Integration Guide

> Comprehensive reference for integrating GeneralUpdate into .NET applications.
> ⚠️ Targeting **NuGet v10.4.6 stable** API (Configinfo + SetConfig + LaunchAsync, no SetSource/SetOption).

## Architecture
- Dual-process: Client (verification/download/IPC) + Upgrade (file replacement)
- 4 update scenes: None/UpgradeOnly/MainOnly/Both
- AppType: **class** (ClientApp=1, UpgradeApp=2) — not an enum
- IPC: Encrypted file (default)

## NuGet Package Rules
- Core only: `dotnet add package GeneralUpdate.Core`
- With Bowl: reference **only** `GeneralUpdate.Bowl` (the two conflict if referenced together)
- Differential: already embedded in Core, no extra package needed

## Bootstrap Setup (v10.4.6 stable)
```csharp
var config = new Configinfo
{
    UpdateUrl = "https://server/api",
    AppSecretKey = "your-key",
    AppName = "MyApp.exe",
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

## Events (v10.4.6)
- AddListenerUpdateInfo → UpdateInfoEventArgs
- AddListenerMultiDownloadStatistics → MultiDownloadStatisticsEventArgs
- AddListenerMultiDownloadCompleted → MultiDownloadCompletedEventArgs **(IsComplated — note typo)**
- AddListenerMultiDownloadError → MultiDownloadErrorEventArgs
- AddListenerMultiAllDownloadCompleted → MultiAllDownloadCompletedEventArgs
- AddListenerException → ExceptionEventArgs

## Known Issues
- Upgrade not starting: Check UpgradeApp.exe exists in update/ dir
- Method not found: Align NuGet versions between Client and Upgrade
- Version wrong: Use 4-segment format
- Infinite loop: manifest writes back version
