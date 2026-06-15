---
name: generalupdate-advanced
description: |
  Complete guide to all GeneralUpdate extension points, IPC providers (encrypted file /
  named pipe / shared memory / auto-fallback), custom strategies, lifecycle hooks,
  Bowl crash daemon, download pipeline, filetree system, blacklist, event system,
  AOT compatibility, and cross-project dependencies (Drivelution/Differential).
  Triggers on: "extension points", "custom hooks", "Bowl", "crash dump", "IPC",
  "named pipe", "shared memory", "custom strategy", "download pipeline",
  "SSL policy", "auth provider", "custom download", "extension management",
  "黑名单", "BlackList", "FileTree", "AOT", "NativeAOT", "高级定制",
  "自定义策略", "自定义认证", "Bowl守护", "IPC替换".
when_to_use: |
  - User wants to customize GeneralUpdate beyond basic Bootstrap config
  - User needs lifecycle hooks (before/after update, download complete, app start)
  - User wants crash monitoring and auto-restore (Bowl)
  - User wants to replace IPC mechanism (NamedPipe instead of encrypted file)
  - User needs custom authentication provider (not HMAC/Basic)
  - User wants custom download executor/orchestrator/policy
  - User asks about AOT compatibility or trim warnings
  - User needs file filtering (blacklist) or file tree diffing
  - User wants to integrate Drivelution for driver updates
  - User already completed basic integration and wants more control
allowed-tools: "Read, Write, Edit, Glob"
---

# 🔧 GeneralUpdate 高级定制 — 全部扩展点与深度集成

覆盖所有 10+ 扩展点、4 种 IPC 机制、下载子系统分层架构、Bowl 崩溃守护、事件系统、文件系统工具等。

---

## 1. 扩展点架构全景

GeneralUpdate 采用"策略 + 管道 + 扩展点"架构，几乎所有核心组件都可替换：

```
┌─────────────────────────────────────────────────────────┐
│                 GeneralUpdateBootstrap                    │
│  .Hooks<T>()      — 生命周期回调                           │
│  .Strategy<T>()   — 平台更新策略                           │
│  .UpdateReporter<T>() — 状态上报                           │
│  .DownloadSource<T>() — 版本源（HTTP/OSS/SignalR）        │
│  .DownloadOrchestrator<T>() — 下载编排                     │
│  .DownloadPolicy<T>() — 重试策略                           │
│  .DownloadExecutor<T>() — 单文件下载执行器                  │
│  .DownloadPipeline<T>() — 下载后处理（解密/校验/扫描）      │
│  .SslValidationPolicy<T>() — SSL 证书验证                  │
│  .HttpAuthProvider<T>() — HTTP 认证                        │
│  .DirtyStrategy<T>() — 补丁应用策略                        │
│  .CleanStrategy<T>() — 补丁生成策略                        │
└─────────────────────────────────────────────────────────┘
```

### 注入方式

```csharp
new GeneralUpdateBootstrap()
    .SetSource(url, key)
    .Hooks<MyCustomHooks>()
    .UpdateReporter<CustomReporter>()
    .HttpAuthProvider<BearerAuthProvider>()
    .DownloadPolicy<CustomRetryPolicy>()
    .SslValidationPolicy<CustomSslPolicy>()
    .SetOption(Option.AppType, AppType.Client)
    .LaunchAsync();
```

### ⚠️ 已知问题（#455、#457、#373）

所有扩展点通过 `_extensions` 字典注册，由 `ResolveExtension<T>()` 消费。
旧版本中存在扩展点注册后不消费的 Bug（#455）。**v5.0+ 已全部修复**。

---

## 2. 生命周期 Hooks（IUpdateHooks）

Hooks 在更新流程的关键时间点被调用。所有方法都有默认实现（返回 true/null）：

```csharp
public class MyHooks : IUpdateHooks
{
    // Client 进程生命周期:
    // T0: 更新开始前 → OnBeforeUpdateAsync → false? 中止更新 → 检查磁盘空间/用户确认
    // T1: 下载完成 → OnDownloadCompletedAsync → 通知 UI/扫描文件
    // T2: 所有版本下载完成 → 启动 Upgrade 进程

    // Upgrade 进程生命周期:
    // T3: 更新开始前 → OnBeforeUpdateAsync → false? 中止更新
    // T4: 更新完成 → OnAfterUpdateAsync → 清理临时文件/迁移配置
    // T5: 启动主应用前 → OnBeforeStartAppAsync → false? 阻止启动 → 设置执行权限

    // 任意时刻出错:
    // T6: 更新出错 → OnUpdateErrorAsync → 记录日志/通知管理员

    public async Task<bool> OnBeforeUpdateAsync(UpdateContext context) { ... }
    public async Task OnDownloadCompletedAsync(UpdateContext context) { ... }
    public async Task OnAfterUpdateAsync(UpdateContext context) { ... }
    public async Task OnUpdateErrorAsync(UpdateContext context, Exception ex) { ... }
    public async Task<bool> OnBeforeStartAppAsync(UpdateContext context) { ... }
}
```

**完整示例**：`templates/CustomHooks.cs`

### UnixPermissionHooks（Linux/macOS 必备 #ID5049）

```csharp
bootstrap.Hooks<UnixPermissionHooks>();
// 在 Linux/macOS 上自动设置文件可执行权限（chmod +x）
```

---

## 3. IPC 通信机制（4 种实现）

GeneralUpdate 支持通过 `IProcessInfoProvider` 接口替换 IPC 实现：

| 实现 | 类名 | 安全性 | 性能 | 跨平台 | 适用场景 |
|------|------|:------:|:----:|:-----:|---------|
| **加密文件** (默认) | `EncryptedFileProcessContractProvider` | ⚠️ 中 | 中 | ✅ | 大多数场景 |
| **命名管道** | `NamedPipeProcessContractProvider` | ✅ 高 | 高 | ⚠️ Windows | 安全要求高、反病毒干扰 |
| **共享内存** | `SharedMemoryProcessContractProvider` | ✅ 高 | 最高 | ⚠️ Windows | 高性能、低延迟 |
| **自动降级** | `AutoFallbackProcessContractProvider` | 可变 | 中 | ✅ | 不确定环境 |

### IPC 数据结构（ProcessContract）

Client → Upgrade 传递的核心数据：

```json
{
  "AppName": "MyApp.exe",
  "InstallPath": "C:\\Program Files\\MyApp",
  "CurrentVersion": "1.0.0.0",
  "LastVersion": "1.1.0.0",
  "UpdateVersions": [
    {
      "Version": "1.1.0.0",
      "Url": "https://...v1.1.0.0.zip",
      "Hash": "sha256-hex",
      "Size": 1048576
    }
  ],
  "CompressFormat": 0,
  "CompressEncoding": 0,
  "BackupDirectory": "C:\\backup",
  "TempPath": "%TEMP%\\upgrade_temp",
  "UpdatePath": "update",
  "LaunchClientAfterUpdate": true,
  "ReportType": 1
}
```

### NamedPipe IPC 示例（替换加密文件）

**完整示例**：`templates/NamedPipeIPC.cs`

### ⚠️ 已知问题

- **硬编码 AES 密钥**（代码审计）：密钥由 `SHA256("GeneralUpdate.IPC.EnvironmentProvider.v1")` 派生，IV 仅第 1 字节非零
- **固定 IPC 路径**（代码审计）：`%TEMP%/GeneralUpdate/ipc/process_info.enc`，本地进程可篡改
- **Bowl IPC 自删除**（#492）：每次读取后自动删除文件，多进程竞争可能导致读写冲突

---

## 4. Bowl 崩溃守护

Bowl 是更新完成后的崩溃监控进程，自动检测主应用是否崩溃并生成诊断报告。

### 工作流程

```
Upgrade 进程完成更新
  → StartAppAsync()
    → 启动主应用
    → (可选) 启动 Bowl 守护进程
    
Bowl.LaunchAsync():
  Phase 1: 准备 procdump 监控子进程
  Phase 2: 运行监控（附加到主进程）
  Phase 3: 检查 dump 文件
  Phase 4 (崩溃检测):
    ├── 生成 MiniDump (.dmp)
    ├── 写入故障报告 (.json)
    ├── 导出系统诊断 (evtx / drivers / systeminfo)
    ├── AutoRestore? → 从备份恢复上个版本
    └── OnCrash 回调通知
```

### Bowl 控制台诊断输出的示例结构（来自 BowlSample.cs）

```
故障目录内容:
  ▸ 1.0.0.0_fail.dmp    — 进程 MiniDump (WinDbg 可分析崩溃堆栈)
  ▸ 1.0.0.0_fail.json   — 故障报告 (Bowl 生成的 crash 元数据)
  ▸ systemlog.evtx      — Windows 系统事件日志 (最近24h)
  ▸ driverInfo.txt      — 已安装驱动列表 (driverquery /v)
  ▸ systeminfo.txt      — 系统信息 (OS/硬件/补丁)
  ▸ RESTORED.txt        — 备份恢复标记 (AutoRestore)
```

### BowlContext 配置

```csharp
var context = new BowlContext
{
    ProcessNameOrId = "MyApp.exe",
    DumpFileName = "v1.0.0.0_fail.dmp",
    FailFileName = "v1.0.0.0_fail.json",
    TargetPath = baseDir,
    FailDirectory = failDir,        // 故障报告输出目录
    BackupDirectory = backupDir,    // 备份目录（回滚用）
    WorkModel = "Upgrade",
    DumpType = DumpType.Mini,
    AutoRestore = true
}.Normalize();
```

**完整示例**：`templates/BowlIntegration.cs`

### ⚠️ 已知问题（#492）

- Bowl 的 IPC 文件在每次读取后自动删除，与 Upgrade 进程同时读写时可能冲突
- Bowl 需要 `procdump` 工具（Windows），Linux/macOS 不支持
- MiniDumpWriteDump API 能写当前进程的 dump（不需要管理员权限）

---

## 5. 下载子系统（分层架构）

GeneralUpdate 的下载系统采用分层抽象，每层都可替换：

```
IDownloadSource         → 版本源（HTTP/OSS/SignalR Hub）
  ↓
DownloadPlanBuilder     → 构建下载计划（过滤版本/平台/最小版本）
  ↓
IDownloadOrchestrator   → 编排下载（并发控制/队列管理）
  ↓
IDownloadExecutor       → 执行下载（HTTP/FTP/自定义协议）
  ↓
IDownloadPipeline       → 后处理（解密/解压/病毒扫描/哈希校验）
  ↓
IDownloadPolicy         → 策略（重试/超时/断点续传）
```

### 自定义下载执行器

```csharp
public class FtpDownloadExecutor : IDownloadExecutor
{
    public async Task<DownloadResult> DownloadAsync(
        DownloadAsset asset, string path, DownloadProgress progress,
        CancellationToken token)
    {
        // 实现 FTP 文件下载
        // 支持进度回调、断点续传
    }
}

// 注入
bootstrap.DownloadExecutor<FtpDownloadExecutor>();
```

---

## 6. 事件系统

EventManager 是一个全局单例（`Lazy<EventManager>`），提供 7 种事件类型：

| 事件类型 | EventArgs | 触发时机 |
|---------|----------|---------|
| `UpdateInfo` | `UpdateInfoEventArgs` | 版本验证完成 |
| `MultiDownloadStatistics` | `MultiDownloadStatisticsEventArgs` | 批量下载进度更新 |
| `MultiDownloadCompleted` | `MultiDownloadCompletedEventArgs` | 单个版本下载完成 |
| `MultiDownloadError` | `MultiDownloadErrorEventArgs` | 单文件下载错误 |
| `MultiAllDownloadCompleted` | `MultiAllDownloadCompletedEventArgs` | 全部版本下载完成 |
| `Progress` | `ProgressEventArgs` | 单文件处理进度（含解压/补丁） |
| `Exception` | `ExceptionEventArgs` | 更新流程异常 |

### IUpdateEventListener（批量注册）

```csharp
public class MyEventListener : IUpdateEventListener
{
    public void OnUpdateInfo(object? sender, UpdateInfoEventArgs e) { }
    public void OnMultiDownloadStatistics(object? sender, MultiDownloadStatisticsEventArgs e) { }
    public void OnMultiDownloadCompleted(object? sender, MultiDownloadCompletedEventArgs e) { }
    public void OnMultiDownloadError(object? sender, MultiDownloadErrorEventArgs e) { }
    public void OnMultiAllDownloadCompleted(object? sender, MultiAllDownloadCompletedEventArgs e) { }
    public void OnProgress(object? sender, ProgressEventArgs e) { }
    public void OnException(object? sender, ExceptionEventArgs e) { }
}

// 批量注册
bootstrap.AddListener(new MyEventListener());
```

### ⚠️ 已知问题

- EventManager 是全局单例，`Dispose()` 后 `_lazy.Value` 仍返回已释放实例（代码审计 #11）
- 长生命周期应用中建议在 Bootstrap 生命周期结束时调用 Clear

---

## 7. 文件系统工具

### Blacklist（文件过滤黑名单）

用于在备份/复过程中排除特定文件或目录：

```csharp
var blacklist = new BlackListConfig
{
    BlackFiles = new[] { "*.log", "*.tmp", "cache.db" },         // 文件名模式
    BlackFormats = new[] { ".pdb", ".vshost.exe" },              // 扩展名
    SkipDirectorys = new[] { "logs", "cache", "temp" }           // 目录名
};

bootstrap.SetOption(Option.BlackListConfig, blacklist);
```

**默认跳过目录**（`BlackDefaults.DefaultDirectories`）：`.backups`, `backup-` 等

### FileTree（文件树系统）

用于追踪文件变更：`FileTreeSnapshot → FileTreeComparer → FileTreeDiffer`

```csharp
// 对目录拍照
var snapshot = new FileTreeSnapshot();
var files = await snapshot.CaptureAsync(installPath);

// 比较两个快照
var comparer = new FileTreeComparer();
var changes = comparer.Compare(oldSnapshot, newSnapshot);

// 生成差异报告
var differ = new FileTreeDiffer();
var diffResult = differ.Diff(changes, patchDir);
```

---

## 8. AOT / Trim 兼容性

GeneralUpdate v5.0+ 支持 .NET Native AOT：

```xml
<PropertyGroup>
  <IsAotCompatible>true</IsAotCompatible>
  <EnableTrimAnalyzer>true</EnableTrimAnalyzer>
</PropertyGroup>
```

### AOT 注意事项

1. **SignalR 使用 JSON 协议**（非 MessagePack），需注册 `JsonSerializerContext`
2. **动态类型创建**（`Activator.CreateInstance`）需 `DynamicDependency` 注解
3. **反射调用**（如策略发现）需 preserve 策略
4. **ILogger 泛型**的 `[LoggerMessage]` 需要使用 Source Generator

---

## 9. Drivelution（Windows 驱动更新）

`GeneralUpdate.Drivelution` 提供 Windows 驱动安装能力：

```csharp
// 扫描驱动目录
var drivers = await GeneralDrivelution.GetDriversFromDirectoryAsync(
    driverDir, searchPattern: null, cancellationToken: ct);

// 验证驱动
var valid = await GeneralDrivelution.ValidateAsync(driver, ct);

// 安装（3种方法级联：DIFx → SetupAPI → PnPUtil）
// 在 WindowsStrategy 中通过 DrivelutionMiddleware 集成
```

**完整示例**：参考 `ImDiskQuickInstallSample.cs` 或使用 DrivelutionMiddleware。

---

## 模板文件索引

| 文件 | 覆盖内容 |
|------|---------|
| `templates/CustomHooks.cs` | 完整 IUpdateHooks 实现 + UnixPermissionHooks |
| `templates/CustomStrategy.cs` | 自定义 IStrategy（替换 Windows/Linux/Mac 策略） |
| `templates/BowlIntegration.cs` | BowlContext 配置 + Bowl.LaunchAsync + OnCrash 回调 |
| `templates/NamedPipeIPC.cs` | NamedPipe 服务端/客户端 + ProcessContract 序列化 |

**参考文档**：`reference.md` — 扩展点 API 速查、Bowl 选项、HTTP 认证提供者完整列表
