# GeneralUpdate Auto-Update Rules

## Architecture
- Dual-process: Client (verification/download/IPC) + Upgrade (file replacement)
- 4 scenes: None/UpgradeOnly/MainOnly/Both
- IPC: Encrypted file (AES-CBC, default)

## Bootstrap (v10.5.0-beta.4 API)
- UpdateRequest / SetSource() + SetConfig() + LaunchAsync()
- Events: AddListenerUpdateInfo, AddListenerMultiDownloadStatistics, etc.
- Zero-config: `SetSource(updateUrl, appSecretKey)` — auto-configures identity from manifest
- Hooks: `Hooks<T>()` — lifecycle hooks (IUpdateHooks)
- Options: `SetOption(Option.Silent, true)`, `SetOption(Option.MaxConcurrency, 4)`, etc.
- Strategy: `Strategy<T>()` — custom update strategy (IStrategy)

## NuGet Package Rules
- Core only: `dotnet add package GeneralUpdate.Core --version 10.5.0-beta.4`
- With Bowl: **reference only** `GeneralUpdate.Bowl` (transitively includes Core, the two conflict)
- Differential: types are **embedded in Core**, no extra package needed
- Extension, Drivelution: standalone, no conflicts

## AppType (v10.5.0-beta.4 — enum)
- `AppType.Client = 1`, `AppType.Upgrade = 2`, `AppType.OssClient = 3`, `AppType.OssUpgrade = 4`

## UpdateRequest Required
UpdateUrl, AppSecretKey, ClientVersion, MainAppName, ProductId

## 6 Strategies
1. Client-Server (needs backend)
2. OSS (S3/MinIO, no backend)
3. Silent (background polling with `Option.Silent`)
4. Differential (delta patches with `UseDiffPipeline()`)
5. CVP (cross-version jump)
6. SignalR Push (server-initiated)

## Platform
Windows: Hash->Decompress->Patch+Bowl+Drivelution
Linux/Mac: Hash->Decompress->Patch (no Bowl)

## Event Args (v10.5.0-beta.4)
- UpdateInfoEventArgs: Info?.Body (List<VersionEntry>)
- MultiDownloadStatisticsEventArgs: ProgressPercentage, Speed, Remaining, TotalBytesToReceive, BytesReceived
- MultiDownloadCompletedEventArgs: **Version** (object), **IsCompleted** (bool)
- MultiDownloadErrorEventArgs: Exception, Version (object)
- MultiAllDownloadCompletedEventArgs: IsAllDownloadCompleted, FailedVersions
- ProgressEventArgs: Progress (DownloadProgress?), DiffProgress (DiffProgress?)
- ExceptionEventArgs: Exception, Message

## Key Namespaces
- `GeneralUpdate.Core` — GeneralUpdateBootstrap
- `GeneralUpdate.Core.Configuration` — UpdateRequest, UpdateRequestBuilder, Option, AppType
- `GeneralUpdate.Core.Download` — UpdateInfoEventArgs, MultiDownloadStatisticsEventArgs, etc.
- `GeneralUpdate.Core.Event` — ExceptionEventArgs, ProgressEventArgs, IUpdateEventListener
- `GeneralUpdate.Core.Hooks` — IUpdateHooks, HookContext, DownloadContext, NoOpUpdateHooks, UnixPermissionHooks
- `GeneralUpdate.Core.Strategy` — IStrategy, AbstractStrategy
- `GeneralUpdate.Core.Pipeline` — PipelineContext, PipelineBuilder, DiffPipelineBuilder
- `GeneralUpdate.Core.Download.Reporting` — IUpdateReporter, HttpUpdateReporter
- `GeneralUpdate.Core.Download.Models` — DownloadProgress, DownloadAsset

## Quick Fixes
- Upgrade not starting: Check UpdatePath
- Method not found: Align NuGet versions
- Chinese garbled: Ensure UTF-8 ZIP encoding
- Linux no exec: `bootstrap.Hooks<UnixPermissionHooks>()`
- Version wrong: Use 4-segment
- Infinite loop: WriteBack after update

## Diagnostics
1. NuGet versions match
2. manifest.json valid
3. UpgradeApp.exe exists
4. Server API reachable
5. Logs in Logs/generalupdate-trace *.log
