# GeneralUpdate Auto-Update Rules

## Architecture
- Dual-process: Client (verification/download/IPC) + Upgrade (file replacement)
- 4 scenes: None/UpgradeOnly/MainOnly/Both
- IPC: Encrypted file (AES, default)

## Entry Point & Navigation
- Root SKILL.md contains developer roadmap: 6 scenarios → which skill to use → next step
- 5-question decision tree helps first-timers locate their starting skill
- Pre-Delivery Checklist + Anti-Pattern tables in every sub-skill SKILL.md
- Always extract user requirements template before generating code (frontmatter in SKILL.md)

## Bootstrap (v10.4.6 stable API)
- Configinfo + SetConfig() + LaunchAsync()
- Events: AddListenerUpdateInfo, AddListenerMultiDownloadStatistics, etc.

## NuGet Package Rules
- Core only: `dotnet add package GeneralUpdate.Core`
- With Bowl: **reference only** `GeneralUpdate.Bowl` (transitively includes Core, the two conflict)
- Differential: types are **embedded in Core**, no extra package needed
- Extension, Drivelution: standalone, no conflicts

## AppType (v10.4.6 stable — class, not enum)
- `AppType.ClientApp = 1`
- `AppType.UpgradeApp = 2`

## UpdateRequest Required
UpdateUrl, AppSecretKey, InstallPath, ClientVersion, MainAppName, UpdateAppName, ProductId

## 6 Strategies
1. Client-Server (needs backend)
2. OSS (S3/MinIO, no backend)
3. Silent (background polling)
4. Differential (delta patches)
5. CVP (cross-version jump)
6. SignalR Push (server-initiated)

## Platform
Windows: Hash->Decompress->Patch+Bowl+Drivelution
Linux/Mac: Hash->Decompress->Patch (no Bowl)

## Event Args (v10.4.6 stable)
- UpdateInfoEventArgs: Info?.Body (List<VersionInfo>)
- MultiDownloadStatisticsEventArgs: ProgressPercentage, Speed, Remaining, TotalBytesToReceive, BytesReceived
- MultiDownloadCompletedEventArgs: **Version** (object), **IsComplated** (bool) — note typo
- MultiDownloadErrorEventArgs: Exception, Version (object)
- MultiAllDownloadCompletedEventArgs: IsAllDownloadCompleted, FailedVersions
- ProgressEventArgs: Progress (DownloadProgress?), DiffProgress (DiffProgress?) — v10.4.6 Core only
- ExceptionEventArgs: Exception, Message

## Code Generation
- Use `.claude/scripts/generate.py` for parameterized code generation:
  ```bash
  python3 .claude/scripts/generate.py --framework wpf --strategy oss --bowl --project-name MyApp
  ```
- Generates 5 files: Bootstrap.cs, manifest.json, UpgradeProgram.cs, DeploymentChecklist.md, IssuesWarning.md
- Covers 336 combinations: 6 strategies × 7 frameworks × 2 Bowl × 4 scenes

## Troubleshooting Search
- Use BM25 search engine before manual reference.md lookup:
  ```bash
  python3 skills/generalupdate-troubleshoot/scripts/search.py "<symptom>" --domain issue
  ```
- CSV database covers 50+ known issues (51 entries: 8C + 11H + 20M + 12L)
- Strategies are also searchable: `--domain strategy`

## Template Conventions
- Template files use `{{PLACEHOLDER}}` syntax for parameter substitution
- Conditional blocks: `{{#KEY}}...{{/KEY}}` (show if KEY truthy), `{{^KEY}}...{{/KEY}}` (show if falsy)
- Templates stored in `.claude/scripts/generate/templates/`

## Quick Fixes
- Upgrade not starting: Check UpdatePath
- Method not found: Align NuGet versions
- Chinese garbled: Set Encoding.UTF8
- Linux no exec: Manually chmod +x (v10.4.6 has no IUpdateHooks)
- Version wrong: Use 4-segment
- Infinite loop: WriteBack after update

## Diagnostics
1. NuGet versions match
2. manifest.json valid
3. UpgradeApp.exe exists
4. Server API reachable
5. Logs in Logs/generalupdate-trace *.log
6. Use BM25 search engine for known issues
