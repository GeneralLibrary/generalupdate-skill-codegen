# GeneralUpdate Troubleshooting Reference Manual (Complete Edition)

> Covers 50+ symptoms from GitHub Issues (#308-#517), Gitee Issues (30 items),
> and full code audit (17 CRITICAL/HIGH items + 14 MEDIUM items + 10 INFO items)
> Audit date: 2026-06-16

---

## How to Use

Find symptom -> Confirm root cause -> Apply fix. If no symptom matches, run the general diagnostic workflow (see bottom).

---

## Level 1: Critical/Blocking Faults

### C1. Upgrade process doesn't start / "FileNotFoundException: upgrade application not found"

| Source | Root Cause | Diagnosis |
|------|------|------|
| #485, #ID3H5V | UpgradeApp.exe not shipped with main app | Check `UpdatePath` + `UpdateAppName` |

**Fix**:
1. UpgradeApp.exe **must** be shipped together with the main app starting from the first version
2. In OSS mode, Upgrade.exe must be placed in the `update/` subdirectory (#485)
3. `generalupdate.manifest.json` `UpdateAppName` must include `.exe`

```
Deployment directory structure:
/InstallPath
  ├── MyApp.exe
  ├── generalupdate.manifest.json
  └── update/
      └── UpgradeApp.exe   ← Must exist
```

---

### C2. "Method not found" — NuGet version conflict

| Source | Root Cause | Diagnosis |
|------|------|------|
| #I7MCA5 | Client and Upgrade use different NuGet versions | Check csproj of both projects |

**Fix**: Client.csproj and Upgrade.csproj use exactly the same version:
```xml
<PackageReference Include="GeneralUpdate.Core" Version="5.*" />
```

---

### C3. BSOD / Memory overflow / Process crash — BSDIFF integer overflow

| Source | Code Audit #3, #4 |
|------|----------------|
| **Root Cause** | `BsdiffDiffer.WriteInt64` negation overflow on `long.MinValue`; control value `> int.MaxValue` cast truncation produces negative values |

**Impact**: Maliciously crafted patch files or normal patches exceeding 2GB can cause process crash or OOM
**Fix**: Update to v5.0+ (fixed in #514). If unable to update, add `MaxInputFileSize` limit in the diff engine

---

### C4. Backup recursive nesting → PathTooLongException (Path too long)

| Source | #501 |
|------|------|
| **Root Cause** | `StorageManager.Backup()` creates backup directory **inside** `InstallPath`, and an empty list `new List<string>()` does not trigger the default directory skip logic |

**Fix**: Update to v5.0+ (#510 disables backup by default). When manually enabled, explicitly specify skip directories:
```csharp
.SetOption(Option.BackupEnabled, true)
// Make sure DirectoryNames is not empty, or use the default skip list
```

---

### C5. ZIP extraction path traversal — Malicious package can overwrite arbitrary files

| Source | Code Audit #7 |
|------|-----------|
| **Root Cause** | `ZipCompressionStrategy.Decompress` only does `Regex.Replace` cleanup, does not verify `Path.GetFullPath(combinedPath).StartsWith(Path.GetFullPath(unZipDir))` |

**Impact**: Attacker can escape to arbitrary directories via `../../evil.exe` entries
**Fix**: Update to v5.0+ (fixed). For older versions, manually add path validation

---

### C6. Hardcoded AES key — IPC encryption is effectively useless

| Source | Code Audit #1, #2, #14 |
|------|---------------------|
| **Root Cause** | AES key derived from constant `SHA256("GeneralUpdate.IPC.EnvironmentProvider.v1")`, only the 1st byte of the 16-byte IV is non-zero |

**Impact**: Anyone with decompiled code can decrypt IPC files
**Fix**: Use NamedPipe IPC (see advanced/templates/NamedPipeIPC.cs); or deploy DPAPI encryption

---

### C7. Cross-tenant data leakage (Server-side)

| Source | Code Audit #15 |
|------|------------|
| **Impact Scope** | 11 server-side vulnerabilities: package/client/group/upgrade record/file/tenant isolation all missing |

**Fix**: Upgrade to latest GeneralSpacestation; emergency measure: deploy separate instances per tenant

| Specific Vulnerability | File Location | Impact |
|---------|---------|------|
| GroupId filter condition inverted | `ClientService.cs:36-37` | Group query returns wrong clients |
| UserService can modify TenantId | `UserService.cs:338-356` | Cross-tenant privilege escalation |
| Upgrade records lack tenant ID | `UpgradeService.cs:242-256` | Tenant isolation completely ineffective |
| Global packages visible to all tenants | `UpgradeService.cs:49-57` | Cross-tenant data exposure |
| File deletion without tenant filtering | `FileService.cs:98-108` | Any file can be deleted |

---

### C8. PushJob silently swallows exceptions — Quartz unaware of job failure

| Source | Code Audit #16 |
|------|------------|
| **Root Cause** | `PushJob.Execute` is wrapped in `try-catch(Exception)` which only `LogError`s, Quartz does not trigger retry |

**Impact**: Push tasks are completely invisible to operations
**Fix**: Rethrow in the catch block or remove the outer catch

---

## Level 2: High Priority / Scenario-Blocking

### H1. Silent mode not working

| Source | #484, #471, #443, #IJQ0Q5 |
|------|--------------------------|
| **Root Cause** (multiple): | |
| | ① `ProcessExit` event is not guaranteed to fire (does not fire under FailFast/TerminateProcess/Ctrl+C) |
| | ② `manifest.json` default non-empty fields block `AppMetadataDiscoverer.Discover()` |
| | ③ `PatchMiddleware` throws exception in silent mode (DiffPipeline not injected) |
| | ④ `manifest.json` default MainAppName "GeneralUpdate.Core.exe" blocks identity discovery during silent startup |

**Fix**:
```csharp
// ① Trigger explicitly when app closes (replaces ProcessExit dependency)
public void OnAppClosing()
{
    _bootstrap.SilentOrchestrator?.TryLaunchUpgrade();
}

// ② Ensure manifest fields have correct version numbers
// ③ Explicitly disable differential updates
.SetOption(Option.PatchEnabled, false)

// ④ If auto-start after update is not desired (#443)
// Configure SilentAutoRestart = false
```

---

### H2. Infinite update loop ("new version" found on every launch)

| Source | #475, #467, Code Audit #20 |
|------|------------------------|
| **Root Cause** (multiple): | |
| | ① Scenario detection inconsistent with DownloadPlan (server says update exists but no package to download) |
| | ② manifest.json did not WriteBack version number |
| | ③ Version is null/empty → converted to default "1.0.0.0" → always older than server |

**Fix**:
1. Update to v5.0+ (WriteBack + scenario detection fixed)
2. Older versions: manually write back version number in `OnAfterUpdateAsync` hook (see H11)
3. Ensure `ClientVersion` is always a valid 4-segment version number

---

### H3. Process.Start return value not checked after launch

| Source | Code Audit H2 |
|------|-----------|
| **Root Cause** | `Process.Start()` return value not checked in 5 Strategy files (null → silent failure) |

**Fix**: Update to v5.0+ (fixed, throws exception on failure)

---

### H4. UpdateReporter injection not working / ReportUrl not configured throws exception

| Source | #470 |
|------|------|
| **Root Cause** | Registered implementation of `UpdateReporter<T>()` is never consumed; `ProcessInfo` constructor requires `ReportUrl` as mandatory field |

**Fix**: Update to v5.0+ or explicitly set `ReportUrl` (even if not planning to use it)

---

### H5. Sync-over-async deadlock — GetAwaiter().GetResult()

| Source | #451, Code Audit #6 |
|------|-----------------|
| **Root Cause** | `AppDomain.ProcessExit` event handler synchronously calls `.GetAwaiter().GetResult()`, causing deadlock on WPF/WinForms SynchronizationContext |

**Impact**: Desktop apps using silent mode may hang on process exit
**Fix**: Update to v5.0+ (fixed, changed to `ConfigureAwait(false)` + Task.Run)

---

### H6. Before-hook (SafeOnBeforeUpdateAsync) returns true on exception (should return false)

| Source | Code Audit H4 |
|------|-----------|
| **Root Cause** | `ClientStrategy.cs:1015-1026` returns `true` on exception (proceeds with update), should return `false` (abort) |

**Impact**: `OnBeforeUpdateAsync` in Hooks continues even if it throws an exception
**Fix**: Update to v5.0+

---

### H7. Scenario = Both misjudgment — DownloadPlan is empty but judged as having update

| Source | #465, #475 |
|------|-----------|
| **Root Cause** | `HttpDownloadSource.ListAsync()` only checks `Body.Count > 0`, does not verify `AppType` match; `DownloadPlanBuilder.Build()` may return empty list after version filtering |

**Fix**: Update to v5.0+ (scenario detection logic fixed)

---

### H8. OSS mode: Download completed but no update applied

| Source | #485, #487 |
|------|-----------|
| **Root Cause** | ① OSS does not distinguish Main/Upgrade updates (HasMainUpdate and HasUpgradeUpdate are always the same) ② SSL validation does not cover file downloads |

**Fix**:
```csharp
// Recommended OSS mode configuration
.SetOption(Option.PatchEnabled, false)
// Custom SSL policy to ensure download requests are covered
bootstrap.SslValidationPolicy<CustomSslPolicy>();
```

---

### H9. PatchMiddleware always throws in silent mode

| Source | #471 |
|------|------|
| **Root Cause** | `SilentPollOrchestrator.CreateStrategy()` creates a bare `WindowsStrategy` without injecting `DiffPipeline` |

**Fix**:
```csharp
.SetOption(Option.PatchEnabled, false)  // Disable differential updates in silent mode
```

---

### H10. HttpClient infinite timeout

| Source | Code Audit M2 |
|------|-----------|
| **Root Cause** | `HttpClientProvider.Shared` sets `Timeout = InfiniteTimeSpan` |

**Fix**: Update to v5.0+ (set to 5-minute safe timeout limit)

---

### H11. Version number not written back after successful update

| Source | #467, #475 |
|------|-----------|
| **Root Cause** | manifest.json is not updated, version number is still the old one on next startup |

**Fix**: Update to v5.0+ (WriteBack implemented). For older versions, handle manually:
```csharp
// Manually write back in hooks
public async Task OnAfterUpdateAsync(UpdateContext context)
{
    var manifestPath = Path.Combine(context.InstallPath, "generalupdate.manifest.json");
    if (File.Exists(manifestPath))
    {
        var manifest = JsonSerializer.Deserialize<ManifestInfo>(
            await File.ReadAllTextAsync(manifestPath));
        manifest.ClientVersion = context.LastVersion;
        await File.WriteAllTextAsync(manifestPath,
            JsonSerializer.Serialize(manifest));
    }
}
```

---

## Level 3: Medium / Needs Attention

### M1. Incremental update error: patch application failed

| Source | #II75WI, #I8T0QX |
|------|-----------------|
| **Root Cause** | ① Old patch temp files remain ② Files have been modified causing hash mismatch |

**Fix**:
```csharp
.SetOption(Option.AutoCleanTemp, true)
// Manual cleanup:
if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
```

### M2. Same-named files in different directories cause packaging errors

| Source | #II77NS |
|------|---------|
| **Root Cause** | `DefaultCleanMatcher.Match` does not use relative path matching |

**Fix**: Use custom CleanMatcher:
```csharp
new DiffPipelineBuilder()
    .UseCleanMatcher(new CustomRelativePathCleanMatcher())
    .Build();
```

### M3. Multi-level folder structure causes file location errors after update

| Source | #I59QRI |
|------|---------|
| **Root Cause** | Subdirectory files incorrectly updated to root directory |

**Fix**: Update to latest version; ensure differential package path includes full relative path

### M4. Chinese filename garbled

| Source | #I502QQ |
|------|---------|
| **Root Cause** | ZIP extraction did not specify encoding |

**Fix**:
```csharp
.SetOption(Option.Encoding, CompressionEncoding.UTF8)
```

### M5. Abnormal characters in version number

| Source | #I8TNPE |
|------|---------|
| **Root Cause** | Version chain calculation defect, intermediate version numbers polluted |

**Fix**: Update to v5.0+ (fixed)

### M6. File in use / "file in use"

| Source | #479, #ID3UDN |
|------|-------------|
| **Root Cause** | File handles not fully released after process exits |

**Fix**:
```csharp
// Increase wait time, or add retry logic
.SetOption(Option.DownloadTimeout, 120)
```

### M7. Environment.GetEnvironmentVariable("ProcessInfo") is null on Linux

| Source | #ID4ZF5 |
|------|---------|
| **Root Cause** | Linux environment variable scope issue |

**Fix**: Use latest version (now uses encrypted file IPC) or NamedPipe IPC

### M8. Files lack execute permissions after update on Linux/macOS

| Source | #ID5049 |
|------|---------|
| **Root Cause** | New files missing Unix executable permissions |

**Fix**:
```csharp
bootstrap.Hooks<UnixPermissionHooks>();
```

### M9. IPC encrypted file quarantined by antivirus software

| Source | Code Audit #1, #2 |
|------|----------------|
| **Root Cause** | IPC path is fixed at `%TEMP%/GeneralUpdate/ipc/process_info.enc` |

**Fix**: Use NamedPipe IPC instead

### M10. Version comparison error: "1.0" != "1.0.0.0"

| Source | #475, Server #26 |
|------|----------------|
| **Root Cause** | `System.Version` parses "1.0" as `1.0.-1.-1`, `< "1.0.0.0"` |

**Fix**: Server and client version numbers should be unified to 4-segment format

### M11. Assembly.GetExecutingAssembly returns incorrect version

| Source | #I5O4KV |
|------|---------|
| **Root Cause** | Should use `Assembly.GetEntryAssembly()`, not `GetExecutingAssembly()` |

**Fix**: Explicitly fill `ClientVersion` in manifest.json

### M12. SignalR push has no response (ObjectDisposedException)

| Source | #402, Code Audit #5 |
|------|-----------------|
| **Root Cause** | `UpgradeHubService.DisposeAsync` does not set null, crashes on reconnect |

**Fix**: Use `SafeHubConnection` wrapper class (see PushStrategy.cs)

### M13. Bowl does not generate dump files

| Source | #492 |
|------|------|
| **Root Cause** | Bowl IPC file auto-deleted after each read, multi-process race condition |

**Fix**: Update to v5.0+ (Bowl IPC architecture fixed); manually download procdump

### M14. Default backup retains at most 3 versions

| Source | Default behavior |
|------|---------|
| **Root Cause** | `StorageManager.CleanBackup` only keeps the most recent 3 backups |

**Fix**: To retain more, customize BackupConfig:
```csharp
.SetOption(Option.BackupConfig, new BackupConfig { KeepVersions = 10 })
```

### M15. DefaultCleanMatcher creates new StorageManager instance on each call (not thread-safe)

| Source | Code Audit #17 |
|------|------------|
| **Root Cause** | Instance-level fields `_fileCount` and `ComparisonResult` are shared across parallel calls |

**Fix**: Update to v5.0+ or add locking in `DiffPipeline.CleanAsync`

### M16. HttpDownloadExecutor does not validate Content-Length

| Source | Code Audit #22 |
|------|------------|
| **Root Cause** | `StreamDownloadAsync` does not verify downloaded byte count |

**Fix**: Update to v5.0+ (validation added)

### M17. OssStrategy.StartAppAsync returns Task.CompletedTask

| Source | Code Audit #30 |
|------|------------|
| **Root Cause** | Returns directly when `appName` is empty, caller cannot distinguish "started" from "skipped" |

**Fix**: Explicitly check for null and throw exception

### M18. EventManager singleton — accessible after Dispose

| Source | Code Audit #11 |
|------|------------|
| **Root Cause** | `Lazy<EventManager>` singleton, `_lazy.Value` returns disposed instance after Dispose |

**Fix**: Manage lifecycle manually, call Clear at the end of Bootstrap

### M19. GeneralTracer.Dispose clears global Trace.Listeners

| Source | Code Audit #13 |
|------|------------|
| **Root Cause** | `Dispose()` calls `Trace.Listeners.Clear()`, affecting log output of other libraries in the same process |

**Fix**: Update to v5.0+ (changed to only remove its own Listener)

### M20. GeneralTracer logs rotate daily but never expire

| Source | Code Audit #28 |
|------|------------|
| **Root Cause** | `generalupdate-trace {yyyy-MM-dd}.log` never expires |

**Fix**: Manually configure log retention policy, or periodically clean the `Logs/` directory

---

## Level 4: Low Priority / Code Smell / Known Behavior

### L1. DefaultRetryPolicy uses string containment to check HTTP status codes

| Source | Code Audit #10 |
|------|------------|
| **Root Cause** | `s.Contains("500")` may incorrectly match "500" in URLs or response body |
| **Suggestion** | Use `HttpRequestException.StatusCode` property |

### L2. OssDownloadSource does not distinguish Main/Upgrade

| Source | Code Audit #27 |
|------|------------|
| **Root Cause** | Sets both `HasMainUpdate` and `HasUpgradeUpdate` to `assets.Count > 0` |
| **Suggestion** | Accept this behavior in OSS mode, or implement IDownloadSource yourself |

### L3. ProcessContract constructor null-check order is wrong

| Source | Code Audit #9 |
|------|------------|
| **Root Cause** | Checks `Directory.Exists(installPath)` first, then `?? throw` |
| **Suggestion** | Minor issue, does not affect functionality |

### L4. ConfigurationMapper.MapToUpdateContext silently accepts null

| Source | Code Audit #20 |
|------|------------|
| **Root Cause** | `source == null` returns an empty `UpdateContext` |
| **Suggestion** | Check whether configuration is loaded correctly |

### L5. StorageManager uses string.Contains for directory skipping

| Source | Code Audit #21 |
|------|------------|
| **Root Cause** | `dirName.Contains("backup-")`, so directory `backup-custom` is also skipped because it contains "backup-" |
| **Suggestion** | Low impact; use custom skip strategy for precise control |

### L6. FileTreeComparer FAT32 2-second timestamp precision misses changes

| Source | Code Audit #18 |
|------|------------|
| **Root Cause** | FAT32 filesystem timestamp precision is 2 seconds |
| **Suggestion** | Add hash comparison fallback for FAT32 volumes |

### L7. DiffPipeline.CopyUnknownFiles uses Replace to extract relative path

| Source | Code Audit #31 |
|------|------------|
| **Root Cause** | `file.FullName.Replace(targetPath, "")` fails when targetPath appears in the middle of a path |
| **Suggestion** | Use `StartsWith + Substring` |

### L8. StreamingHdiffDiffer truncates files that exceed size limit

| Source | Code Audit #32 |
|------|------------|
| **Root Cause** | When exceeding `MaxWindowSize` (default 128MB), truncates to read only first 128MB |
| **Suggestion** | Use full update instead of differential for large files |

### L9. Bowl StorageHelper.Restore executes unconditionally

| Source | Code Audit #33 |
|------|------------|
| **Root Cause** | When `AutoRestore=true`, no validation of restore result |
| **Suggestion** | Add verification after rollback |

### L10. OssStrategy version comparison may throw exception

| Source | Code Audit #23 |
|------|------------|
| **Root Cause** | `new Version("")` throws ArgumentException |
| **Suggestion** | Use `ParseVersion` for safe parsing |

### L11. Silent mode auto-starts app after update

| Source | #IJQ0Q5 |
|------|---------|
| **Suggestion** | Control via the `SilentAutoRestart` option |

### L12. ZIP package encoding in OSS mode cannot be extracted

| Source | #I59Q5W, #I502QQ |
|------|----------------|
| **Suggestion** | Specify UTF-8 when building ZIP, verify extraction before uploading |

---

## General Diagnostic Workflow

When the user's reported issue is not found in the above checklist, perform a systematic diagnosis:

### Step 1: Version Check
```
□ Client and Upgrade use the same NuGet version?
□ Using the latest stable version (v5.0+ recommended)?
```

### Step 2: Configuration File Check
```
□ Does generalupdate.manifest.json exist?
□ Is the format correct (valid JSON)?
□ Is ClientVersion filled in (non-empty string)?
□ Does MainAppName include .exe extension?
□ Does UpdateAppName point to an existing file?
□ Is InstallPath accessible?
```

### Step 3: Dual-Process Check
```
□ Does UpgradeApp.exe exist in the deployment directory?
□ Do Client and Upgrade use the same AppSecretKey?
□ Is %TEMP%/GeneralUpdate/ipc/ directory writable?
□ Has antivirus software not quarantined this directory?
```

### Step 4: Strategy Configuration Check
```
Standard Mode:
  □ Is UpdateUrl accessible (HTTP 200)?
  □ Does /Upgrade/Verification endpoint return correct format?
  □ Is AppSecretKey consistent with the server?

OSS Mode:
  □ Can versions.json URL be downloaded?
  □ Is versions.json format correct?
  □ Are version comparisons working correctly?

Silent Mode:
  □ Can ProcessExit fire (non-FailFast scenario)?
  □ Is TryLaunchUpgrade() explicitly called when app closes?
  □ Are all manifest fields correctly filled in?
```

### Step 5: Log Check
```
□ Check generalupdate-trace {yyyy-MM-dd}.log (located at {BaseDir}/Logs/)
□ Did EventManager fire the Exception event?
□ Did AddListenerException receive an exception?
```

### Step 6: Platform-Specific Check
```
Windows:
  □ Is antivirus software blocking IPC files or temp directories?
  □ Is administrator privilege required?

Linux/macOS:
  □ Are file executable permissions set?
  □ Is Mono or .NET runtime version compatible?

AOT:
  □ Does SignalR use JSON protocol + JsonSerializerContext?
  □ Are reflection calls preserved?
```

---

## Quick Diagnostic Commands

```bash
# 1. Check manifest file
cat generalupdate.manifest.json | python3 -m json.tool

# 2. Check if upgrade program exists
ls -la update/UpgradeApp.exe

# 3. Check IPC files
ls -la /tmp/GeneralUpdate/ipc/  # 或 %TEMP%/GeneralUpdate/ipc/

# 4. Check update logs
cat Logs/generalupdate-trace\ *.log | tail -100

# 5. Verify server API
curl -X POST https://your-server.com/Upgrade/Verification \
  -H "Content-Type: application/json" \
  -d '{"appKey":"test","appType":0,"clientVersion":"1.0.0.0","productId":"test"}'
```

---

## Issue Index (Quick Navigation)

| Scope | Content | GitHub | Gitee |
|------|------|--------|-------|
| v5 重构 | 策略/配置/Bootstrap 重写 | #308–#361 | — |
| 扩展点修复 | 扩展点注入不消费 | #455, #457, #373 | — |
| 静默修复 | ProcessExit/PatchMiddleware | #471, #484 | #IJQ0Q5 |
| 场景判断 | Both 误判 / 无限循环 | #465, #475 | — |
| 差分问题 | 同名文件/残留 | — | #II77NS, #I8T0QX |
| 中文乱码 | ZIP 编码 | — | #I502QQ |
| 多级文件夹 | 文件位置错乱 | — | #I59QRI |
| Linux 权限 | 可执行权限 | — | #ID5049 |
| Linux IPC | 环境变量为空 | — | #ID4ZF5 |
| NuGet 版本 | Method not found | — | #I7MCA5 |
| 备份嵌套 | PathTooLongException | #501 | — |
| Bowl IPC | 文件读写冲突 | #492 | — |
| 推送 | SignalR 重连崩溃 | #402 | — |
| OSS | 不区分场景、SSL 覆盖 | #485, #487 | — |
| IPC 加密 | 固定密钥、固定路径 | 代码审计 #1, #2 | — |
| BSDIFF | 整数溢出、OOM | 代码审计 #3, #4 | — |
| 路径穿越 | ZIP 解压 | 代码审计 #7 | — |
| 跨租户 | 11 处服务端漏洞 | 代码审计 #15 | — |
