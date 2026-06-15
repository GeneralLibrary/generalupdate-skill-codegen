# GeneralUpdate Auto-Update Rules

## Architecture
- Dual-process: Client (verification/download/IPC) + Upgrade (file replacement)
- 4 scenes: None/UpgradeOnly/MainOnly/Both
- IPC: Encrypted file(AES, default), NamedPipe, SharedMemory, AutoFallback

## Bootstrap
- Minimal: SetSource(url, key) -> SetOption(AppType.Client) -> LaunchAsync()
- Standard: SetConfig(UpdateRequest) -> SetOption() -> AddListener*() -> LaunchAsync()
- From config: LoadFromConfiguration(config.GetSection("GeneralUpdate"))

## NuGet
Core(required), Differential(delta), Bowl(crash), Extension(plugins), Drivelution(drivers)

## UpdateRequest Required
UpdateUrl, AppSecretKey, InstallPath, ClientVersion, MainAppName, UpdateAppName, ProductId

## Options
MaxConcurrency=3, PatchEnabled=false, BackupEnabled=false(v5+), Silent=false, Format=Zip, DownloadTimeout=60

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
Hooks, Strategy, UpdateReporter, DownloadSource, DownloadOrchestrator, DownloadPolicy, DownloadExecutor, DownloadPipeline, SslValidationPolicy, HttpAuthProvider

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
2. generalupdate.manifest.json valid
3. UpgradeApp.exe exists
4. Server API reachable
5. Logs in Logs/generalupdate-trace *.log
