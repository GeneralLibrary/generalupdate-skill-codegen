# GeneralUpdate Auto-Update Rules

## Architecture
- Dual-process: Client (verification/download/IPC) + Upgrade (file replacement)
- 4 scenes: None/UpgradeOnly/MainOnly/Both
- IPC: Encrypted file (AES, default), custom IPC via Strategy override

## Bootstrap
- Minimal: SetSource(url, key) -> SetOption(Client) -> LaunchAsync()
- Standard: SetConfig(UpdateRequest) -> SetOption() -> AddListener*() -> LaunchAsync()
- From config: LoadFromConfiguration(config.GetSection("GeneralUpdate"))
- From file: SetConfig("config.json")

## NuGet
Core(required), Differential(delta), Bowl(crash), Extension(plugins), Drivelution(drivers)

## UpdateRequest Required
UpdateUrl, AppSecretKey, InstallPath, ClientVersion, MainAppName, UpdateAppName, ProductId

## AppType Enum
Client=1, Upgrade=2, OssClient=3, OssUpgrade=4

## Options
MaxConcurrency=3, PatchEnabled=true(v5+), BackupEnabled=false(v5+), Silent=false, Format=Zip, DownloadTimeout=30, RetryCount=3
RetryInterval=1s, LaunchClientAfterUpdate=true, DiffMode=Serial

## 6 Strategies
1. Client-Server (needs backend)
2. OSS (S3/MinIO, no backend)
3. Silent (background polling)
4. Differential (delta patches)
5. CVP (cross-version jump)
6. SignalR Push (server-initiated)

## Platform
Windows: Hash->Decompress->Patch+Bowl+Drivelution
Linux/Mac: Hash->Decompress->Patch (no Bowl, need UnixPermissionHooks)

## Extension Points
Hooks, Strategy, UpdateReporter, DownloadSource, DownloadOrchestrator, DownloadPolicy, DownloadExecutor, DownloadPipeline, HttpAuthProvider, DirtyStrategy, CleanStrategy

## Event Args (v5.0+)
- UpdateInfoEventArgs: Info?.Body (List<VersionEntry>)
- MultiDownloadStatisticsEventArgs: ProgressPercentage, Speed, Remaining, TotalBytesToReceive, BytesReceived
- MultiDownloadCompletedEventArgs: Version (object), IsCompleted
- MultiDownloadErrorEventArgs: Exception, Version (object)
- MultiAllDownloadCompletedEventArgs: IsAllDownloadCompleted, FailedVersions
- ProgressEventArgs: Progress (DownloadProgress?), DiffProgress (DiffProgress?)
- ExceptionEventArgs: Exception, Message

## Quick Fixes
- Upgrade not starting: Check UpdatePath
- Method not found: Align NuGet versions
- Silent mode broken: Call TryLaunchUpgrade()
- Chinese garbled: Set Encoding.UTF8
- Linux no exec: Add UnixPermissionHooks
- Version wrong: Use 4-segment
- Infinite loop: WriteBack after update
- Path too long: Upgrade to v5.0+

## Diagnostics
1. NuGet versions match
2. manifest.json valid
3. UpgradeApp.exe exists
4. Server API reachable
5. Logs in Logs/generalupdate-trace *.log
