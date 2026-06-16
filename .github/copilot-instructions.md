# GeneralUpdate Auto-Update Integration Guide

> Comprehensive reference for integrating GeneralUpdate into .NET applications.

## Architecture
- Dual-process: Client (verification/download/IPC) + Upgrade (file replacement)
- 4 update scenes: None/UpgradeOnly/MainOnly/Both
- AppType: Client=1, Upgrade=2, OssClient=3, OssUpgrade=4
- IPC: Encrypted file (default), replaceable via custom Strategy

## NuGet Packages
- GeneralUpdate.Core -- Required. Bootstrap/Strategy/Download/IPC/Events
- GeneralUpdate.Differential -- Optional. BSDIFF/HDiffPatch
- GeneralUpdate.Bowl -- Optional. Crash monitoring
- GeneralUpdate.Extension -- Optional. Plugin management
- GeneralUpdate.Drivelution -- Optional. Driver installation

## Bootstrap Setup
- Minimal: SetSource(url, key) -> SetOption(AppType.Client) -> LaunchAsync()
- Standard: SetConfig(UpdateRequest) -> SetOption() -> AddListener*() -> LaunchAsync()
- AppSettings: LoadFromConfiguration(config.GetSection("GeneralUpdate"))
- File config: SetConfig("config.json")

## UpdateRequest Required Fields
UpdateUrl, AppSecretKey, InstallPath, ClientVersion, MainAppName, UpdateAppName, ProductId

## Options
MaxConcurrency=3, PatchEnabled=false, BackupEnabled=false(v5+), Silent=false, Format=Zip, DownloadTimeout=60

## 6 Strategies
1. Client-Server: Standard (needs backend)
2. OSS: S3/MinIO (no backend)
3. Silent: Background polling
4. Differential: Delta patches
5. CVP: Cross-version jump
6. SignalR Push: Server-initiated

## 10 Extension Points
Hooks, Strategy, UpdateReporter, DownloadSource, DownloadOrchestrator, DownloadPolicy, DownloadExecutor, DownloadPipeline, SslValidationPolicy, HttpAuthProvider

## Known Issues
- Upgrade not starting: Check UpdatePath
- Method not found: Align NuGet versions
- Silent mode broken: Call TryLaunchUpgrade()
- Chinese garbled: Set Encoding.UTF8
- Linux no exec: Add UnixPermissionHooks
- Version wrong: Use 4-segment format
- Infinite loop: WriteBack after update
