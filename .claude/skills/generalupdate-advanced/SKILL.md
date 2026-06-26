---
name: generalupdate-advanced
description: |
  Reference guide for GeneralUpdate internal architecture — Pipeline, middleware,
  Strategy, Differential engine, Bowl crash monitor, FileTree, blacklist, and AOT.
  Covers all extension points available in v10.5.0-rc.1 including Pipeline, Hooks, Bowl, AOT, and DiffPipeline.
  Triggers on: "extension points", "custom hooks", "Bowl", "crash dump", "IPC",
  "named pipe", "shared memory", "custom strategy", "download pipeline",
  "SSL policy", "auth provider", "custom download", "extension management",
  "黑名单", "BlackList", "FileTree", "AOT", "NativeAOT", "高级定制",
  "自定义策略", "自定义认证", "Bowl守护", "IPC替换".
when_to_use: |
  - User wants to customize GeneralUpdate beyond basic Bootstrap config
  - User wants crash monitoring and auto-restore (Bowl)
  - User needs custom authentication provider
  - User asks about AOT compatibility or trim warnings
  - User needs file filtering (blacklist) or file tree diffing
  - User wants to integrate Drivelution for driver updates
  - User already completed basic integration and wants more control
allowed-tools: "Read, Write, Edit, Glob"
---

# GeneralUpdate Advanced Customization Reference / GeneralUpdate 高级定制参考

Covers extension point architecture, Pipeline, differential engine, Bowl crash daemon, event system, file system tools, and more.
涵盖扩展点架构、Pipeline 管道、差分引擎、Bowl 崩溃守护、事件系统、文件系统工具等。

> **API Version Notes**: This guide targets **NuGet v10.5.0-rc.1**.
> The following features are all **available** in v10.5.0-rc.1:
> - ✅ `IUpdateHooks` lifecycle hooks (`Hooks<T>()`)
> - ✅ `IStrategy` custom strategy injection (`Strategy<T>()`)
> - ✅ `SilentPollOrchestrator` silent poll orchestrator (`Option.Silent`)
> - ✅ `Option` programmable configuration system
> - ✅ `ISslValidationPolicy` SSL policy interface
> - ✅ `IHttpAuthProvider` HTTP auth provider
> - ✅ `DiffPipelineBuilder` diff pipeline configuration
>
> Namespaces and usage for each feature are noted in the text.
>
> ⚠️ **API 版本说明**：本指南基于 **NuGet v10.5.0-rc.1**。
> 以下功能在 v10.5.0-rc.1 中全部**可用**：
> - ✅ `IUpdateHooks` 生命周期钩子（`Hooks<T>()`）
> - ✅ `IStrategy` 自定义策略注入（`Strategy<T>()`）
> - ✅ `SilentPollOrchestrator` 静默轮询器（`Option.Silent`）
> - ✅ `Option` 可编程配置系统
> - ✅ `ISslValidationPolicy` SSL 策略接口
> - ✅ `IHttpAuthProvider` HTTP 认证提供者
> - ✅ `DiffPipelineBuilder` 差分管道配置
>
> 各功能的命名空间和用法在文中已标注。

---

## User Requirements Checklist (Must Confirm Before Advanced Customization) / 用户需求提取（高级定制前必须确认）

```
### Customization Goals (Required) / 定制目标（必需）
- What customization is needed: ______ (Bowl crash daemon / IPC replacement / Pipeline customization / Custom strategy / AOT / Drivelution / Blacklist / Auth provider / Differential engine)
  需要什么定制: ______（Bowl 崩溃守护 / IPC 替换 / Pipeline 定制 / 自定义策略 / AOT / Drivelution / 黑名单 / 认证提供者 / 差分引擎）
- GeneralUpdate version used: ______ (v10.4.6 stable / v10.5.0+ dev branch)
  使用的 GeneralUpdate 版本: ______（v10.4.6 稳定版 / v10.5.0+ 开发分支）
- .NET version: ______ (.NET 6/8/9/10)
  .NET 版本: ______（.NET 6/8/9/10）

### Bowl (If Selected) / Bowl（如果选择）
- Monitored process name: ______
  被监控进程名: ______
- Work model: ______ (Normal / Upgrade)
  工作模式: ______（Normal / Upgrade）
- Need crash dump: ______ (Yes/No)
  是否需要崩溃 Dump: ______（是/否）
- Backup directory path: ______
  备份目录路径: ______

### IPC Replacement (If Selected) / IPC 替换（如果选择）
- Replacement method: ______ (NamedPipe / SharedMemory / Custom)
  替换方式: ______（NamedPipe / SharedMemory / 自定义）
- Target platform: ______ (Windows / Linux / macOS / Cross-platform)
  目标平台: ______（Windows / Linux / macOS / 跨平台）
- Security requirements: ______ (Encryption / Signing / No extra security)
  安全要求: ______（加密 / 签名 / 无额外安全）

### AOT (If Selected) / AOT（如果选择）
- Current trim warnings: ______ (Yes/No)
  当前剪裁警告: ______（有/无）
- Using reflection: ______ (Yes/No)
  是否使用反射: ______（是/否）
- JSON serialization needed: ______ (Yes/No)
  JSON 序列化需求: ______（有/无）
```

---

## 1. Pipeline System (Available in v10.5.0-rc.1) / Pipeline 管道系统（v10.5.0-rc.1 可用）

GeneralUpdate uses the Pipeline pattern to handle verification, decompression, and patch application for update packages.
GeneralUpdate 使用 Pipeline 管道模式处理更新包的校验、解压、补丁应用。

### PipelineBuilder API

```csharp
using GeneralUpdate.Core.Pipeline;

// Create pipeline context / 创建管道上下文
var context = new PipelineContext();
context.Add("ZipFilePath", @"C:\temp\update.zip");
context.Add("Hash", "sha256-hex-value");
context.Add("Format", 0);  // 0=Zip
context.Add("Encoding", System.Text.Encoding.UTF8);
context.Add("SourcePath", @"C:\Program Files\MyApp");
context.Add("PatchEnabled", true);

// Build and execute pipeline / 构建并执行管道
await new PipelineBuilder(context)
    .UseMiddleware<HashMiddleware>()      // Hash verification / 哈希校验
    .UseMiddleware<CompressMiddleware>()  // Decompress / 解压
    .UseMiddleware<PatchMiddleware>()     // Diff patch / 差分补丁
    .Build();
```

| Middleware / 中间件 | Class / 类名 | Namespace / 命名空间 | Function / 功能 |
|--------|------|---------|------|
| Hash Verification / 哈希校验 | `HashMiddleware` | `GeneralUpdate.Core.Pipeline` | SHA256 integrity check / SHA256 完整性校验 |
| Decompress / 解压 | `CompressMiddleware` | `GeneralUpdate.Core.Pipeline` | Decompress ZIP package / 解压 ZIP 包 |
| Diff Patch / 差分补丁 | `PatchMiddleware` | `GeneralUpdate.Core.Pipeline` | Apply BSDIFF/HDiffPatch patches / 应用 BSDIFF/HDiffPatch 补丁 |
| Driver Update / 驱动更新 | `DrivelutionMiddleware` | `GeneralUpdate.Core.Pipeline` | Windows driver installation / Windows 驱动安装 |

---

## 2. Strategy System (Available in v10.5.0-rc.1) / 策略系统（v10.5.0-rc.1 可用）

GeneralUpdate includes three built-in platform strategies, implemented via the `IStrategy` interface:
GeneralUpdate 内置三种平台策略，通过 `IStrategy` 接口实现：

| Strategy / 策略 | Class / 类名 | Platform / 平台 |
|------|------|------|
| Windows | `WindowsStrategy` | Windows |
| Linux | `LinuxStrategy` | Linux |
| OSS | `OSSStrategy` | Cross-platform (object storage) / 跨平台（对象存储） |

> ✅ Custom strategies can be injected via `bootstrap.Strategy<T>()`.
> Custom strategies must implement the `IStrategy` interface.
>
> ✅ 支持通过 `bootstrap.Strategy<T>()` 注入自定义策略。
> 自定义策略需要实现 `IStrategy` 接口。

---

## 3. Bowl Crash Daemon (v10.5.0-rc.1) / Bowl 崩溃守护（v10.5.0-rc.1）

Bowl is a crash monitoring component configured via `BowlContext`.
Bowl 是一个崩溃监控组件，通过 `BowlContext` 配置。

### BowlContext Configuration / BowlContext 配置

```csharp
using GeneralUpdate.Bowl;

var context = new BowlContext
{
    ProcessNameOrId = "MyApp.exe",
    DumpFileName = "v1.0.0.0_fail.dmp",
    FailFileName = "v1.0.0.0_fail.json",
    TargetPath = @"C:\Program Files\MyApp",
    FailDirectory = @"C:\Program Files\MyApp\fail",
    BackupDirectory = @"C:\Program Files\MyApp\backup",
    WorkModel = "Upgrade",
    TimeoutMs = 30_000,
    AutoRestore = true,
    OnCrash = async (info, ct) => Console.WriteLine($"Crash: {info.DumpFilePath}"),
};

var bowl = new BowlBootstrap();
var result = await bowl.LaunchAsync(context);
Console.WriteLine($"Result: Success={result.Success}, Restored={result.Restored}");
```

| Property / 属性 | Type / 类型 | Description / 说明 | Default / 默认值 |
|------|------|------|--------|
| `ProcessNameOrId` | string | Monitored process name or PID / 被监控的进程名或 PID | Required / 必填 |
| `TargetPath` | string | App installation root directory / 应用安装根目录 | Required / 必填 |
| `DumpFileName` | string | Dump file name / Dump 文件名 | Required / 必填 |
| `FailFileName` | string | Failure report file name / 故障报告文件名 | Required / 必填 |
| `FailDirectory` | string | Crash report output directory / 崩溃报告输出目录 | Required / 必填 |
| `BackupDirectory` | string | Backup directory / 备份目录 | Required / 必填 |
| `WorkModel` | string | "Upgrade" or "Normal" | "Upgrade" |
| `TimeoutMs` | int | Monitoring timeout (ms) / 监控超时(毫秒) | 30000 |
| `AutoRestore` | bool | Auto rollback after crash / 崩溃后自动回滚 | false |
| `DumpType` | DumpType | Mini / Full | Full |
| `OnCrash` | delegate | Crash callback / 崩溃回调 | null |

> ⚠️ In NuGet v10.5.0-rc.1, Bowl and Core have **no type conflicts** and can be referenced together.
> ⚠️ NuGet v10.5.0-rc.1 中 Bowl 和 Core **无类型冲突**，可以同时引用。

---

## 4. EventManager Event System (Available in v10.5.0-rc.1) / EventManager 事件系统（v10.5.0-rc.1 可用）

EventManager is a global singleton that provides event publishing and subscription:
EventManager 是一个全局单例，提供事件的发布和订阅：

```csharp
using GeneralUpdate.Core.Event;

// Add listener / 添加监听
EventManager.Instance.AddListener((object? sender, UpdateInfoEventArgs e) =>
{
    // Handle version discovery event / 处理版本发现事件
});

// Manually dispatch event / 手动分发事件
EventManager.Instance.Dispatch(this, new ExceptionEventArgs(ex, "Custom error / 自定义错误"));

// Clear all listeners / 清空所有监听
EventManager.Instance.Clear();

// Dispose / 释放
EventManager.Instance.Dispose();
```

> ⚠️ EventManager is a global singleton; `Instance` remains accessible after `Dispose()`.
> ⚠️ EventManager 是全局单例，`Dispose()` 后 `Instance` 仍然可访问。

---

## 4.5 IUpdateHooks Lifecycle Hooks (Available in v10.5.0-rc.1) / IUpdateHooks 生命周期钩子（v10.5.0-rc.1 可用）

The `IUpdateHooks` interface allows intercepting various stages of the update flow:
通过 `IUpdateHooks` 接口可以拦截更新流程的各个阶段：

```csharp
using GeneralUpdate.Core;
using GeneralUpdate.Core.Hooks;

// Implement custom Hooks / 实现自定义 Hooks
public class MyCustomHooks : IUpdateHooks
{
    public async Task<bool> OnBeforeUpdateAsync(HookContext ctx)
    {
        Console.WriteLine($"[Hooks] Before update check: {ctx.CurrentVersion} → {ctx.TargetVersion}");
        // [Hooks] 更新前检查: {ctx.CurrentVersion} → {ctx.TargetVersion}
        return true; // return false to cancel this update / 返回 false 取消本次更新
    }

    public async Task OnDownloadCompletedAsync(DownloadContext ctx)
    {
        Console.WriteLine($"[Hooks] Download complete: {ctx.AssetName} ({ctx.TotalBytes} bytes)");
        // [Hooks] 下载完成: {ctx.AssetName} ({ctx.TotalBytes} bytes)
    }

    public async Task OnAfterUpdateAsync(HookContext ctx)
    {
        Console.WriteLine($"[Hooks] Update complete: {ctx.CurrentVersion} → {ctx.TargetVersion}");
        // [Hooks] 更新完成: {ctx.CurrentVersion} → {ctx.TargetVersion}
    }

    public async Task OnUpdateErrorAsync(HookContext ctx, Exception ex)
    {
        Console.WriteLine($"[Hooks] Update failed: {ex.Message}");
        // [Hooks] 更新失败: {ex.Message}
    }

    public async Task OnBeforeStartAppAsync(HookContext ctx)
    {
        Console.WriteLine($"[Hooks] About to start: {ctx.InstallPath}");
        // [Hooks] 即将启动: {ctx.InstallPath}
    }
}

// Register Hooks / 注册 Hooks
await new GeneralUpdateBootstrap()
    .SetConfig(config)
    .Hooks<MyCustomHooks>()
    .LaunchAsync();
```

Built-in implementations / 内置实现：
- `NoOpUpdateHooks` (default, no-op / 默认，空操作)
- `UnixPermissionHooks` (Linux/macOS set executable permissions: `chmod +x` / Linux/macOS 设置可执行权限：`chmod +x`)

> Full template: `templates/CustomHooks.cs`
> 📁 完整模板：`templates/CustomHooks.cs`

---

## 4.6 Custom SSL Policy & HTTP Auth (Available in v10.5.0-rc.1) / 自定义 SSL 策略 & HTTP 认证（v10.5.0-rc.1 可用）

### ISslValidationPolicy — Custom SSL Certificate Validation / 自定义 SSL 证书验证

```csharp
using GeneralUpdate.Core.Security;

// Development: accept all certificates (testing only!) / 开发环境：接受所有证书（仅测试用！）
public class DevelopmentSslPolicy : ISslValidationPolicy
{
    public bool ValidateCertificate(
        System.Security.Cryptography.X509Certificates.X509Certificate2? certificate,
        System.Security.Cryptography.X509Certificates.X509Chain? chain,
        System.Net.Security.SslPolicyErrors sslPolicyErrors)
    {
        return true; // ⚠️ Do NOT do this in production! / 生产环境不要这样做！
    }
}

// Register SSL policy (global) / 注册 SSL 策略（全局生效）
VersionService.SetSslValidationPolicy(new DevelopmentSslPolicy());
```

### IHttpAuthProvider — Custom HTTP Authentication / 自定义 HTTP 认证

```csharp
using GeneralUpdate.Core.Security;

// Custom Bearer Token auth provider example / 自定义 Bearer Token 认证提供者示例
// Note: for the built-in version, use BearerTokenAuthProvider directly
// 注意：如需内置版本，直接使用 BearerTokenAuthProvider
public class CustomBearerTokenAuthProvider : IHttpAuthProvider
{
    private readonly string _token;

    public CustomBearerTokenAuthProvider(string token)
    {
        _token = token;
    }

    public async Task ApplyAuthAsync(HttpRequestMessage request, CancellationToken token = default)
    {
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
        await Task.CompletedTask;
    }
}

// Register custom auth provider (global) / 注册自定义认证提供者（全局生效）
VersionService.SetDefaultAuthProvider(new CustomBearerTokenAuthProvider("your-jwt-token"));
```

> ⚠️ SSL policy and auth provider are globally registered via `VersionService`, not through `SetOption()`. Built-in implementations include `StrictSslValidationPolicy` (default), `NoOpAuthProvider`, `BearerTokenAuthProvider`, `ApiKeyAuthProvider`, `HmacAuthProvider`, and `BasicAuthProvider`.
>
> ⚠️ SSL 策略和认证提供者通过 `VersionService` 全局注册，而非 `SetOption()`。内置实现包括 `StrictSslValidationPolicy`（默认）、`NoOpAuthProvider`、`BearerTokenAuthProvider`、`ApiKeyAuthProvider`、`HmacAuthProvider`、`BasicAuthProvider`。

---

## 4.7 NamedPipe IPC Reference Implementation / NamedPipe IPC 参考实现

GeneralUpdate's built-in IPC mechanism uses encrypted file transfer. For higher-performance inter-process communication, refer to the following NamedPipe implementation:
GeneralUpdate 内置的 IPC 机制使用加密文件传递。如果需要更高性能的进程间通信，可以参考以下 NamedPipe 实现：

> Full template: `templates/NamedPipeIPC.cs`
> 📁 完整模板：`templates/NamedPipeIPC.cs`
>
> ⚠️ In v10.4.6 stable, IPC is not replaceable. NamedPipe replacement requires v10.5.0-rc.1.
> ⚠️ v10.4.6 稳定版中 IPC 不可替换。NamedPipe 替换方案需要 v10.5.0-rc.1。

```csharp
// Basic usage (see NamedPipeIPC.cs template for details) / 基本用法（详情见 NamedPipeIPC.cs 模板）
var ipc = new NamedPipeIpcProvider();

// Server side (Client process — send parameters) / 服务端（Client 进程 — 发送参数）
var pipeName = await ipc.ServerWaitAsync(processId, timeoutMs: 30_000);
await ipc.SendAsync(new { InstallPath = "...", Version = "2.0.0.0" });

// Client side (Upgrade process — receive parameters) / 客户端（Upgrade 进程 — 接收参数）
await ipc.ClientConnectAsync(pipeName);
var data = await ipc.ReceiveAsync<UpdateArgs>();
```

> ⚠️ **Security note**: NamedPipe has no built-in encryption. If security is required, add AES encryption or use SSL/TLS tunneling at the application layer. On Windows, pipe access can be restricted via ACL.
> ⚠️ **安全注意**：NamedPipe 无内置加密，如有安全需求，建议在上层自行添加 AES 加密或使用 SSL/TLS 隧道。Windows 上可通过 ACL 限制管道访问。

---

## 5. File System Tools (Available in v10.5.0-rc.1) / 文件系统工具（v10.5.0-rc.1 可用）

### BlackList / 黑名单

`UpdateRequest` supports excluding files via the following properties:
`UpdateRequest` 支持通过以下属性排除文件：

```csharp
var config = new UpdateRequest
{
    // ...
    Files = new List<string> { "*.log", "*.tmp" },
    Formats = new List<string> { ".pdb", ".vshost.exe" },
    Directories = new List<string> { "logs", "cache", "temp" },
};
```

The blacklist is internally converted to a `BlackPolicy` record via `ToBlackPolicy()`.
黑名单内部通过 `ToBlackPolicy()` 转换为 `BlackPolicy` 记录。

### FileTree (File Tree Comparison) / FileTree（文件树对比）

```csharp
using GeneralUpdate.Core.FileSystem;

// Scan directory to generate file tree snapshot / 扫描目录生成文件树快照
var enumerator = new FileTreeEnumerator(rootPath, blackMatcher: null);
var snapshot = FileTreeSnapshot.FromEnumerator(rootPath, enumerator);

// Compare file trees of two versions / 对比两个版本的文件树
var diff = FileTreeComparer.Compare(oldSnapshot, updatedSnapshot);
```

> `FileTree` is a BST implementation used for internal file sorting and comparison. `FileTreeSnapshot` + `FileTreeEnumerator` + `FileTreeComparer` provide complete file tree snapshot and diff comparison functionality.
> `FileTree` 是 BST 实现，用于内部文件排序和对比。`FileTreeSnapshot` + `FileTreeEnumerator` + `FileTreeComparer` 提供了完整的文件树快照和差异对比功能。

---

## 6. Differential Engine (Available in v10.5.0-rc.1, no extra package needed) / 差分引擎（v10.5.0-rc.1 可用，无需额外安装包）

Differential types are embedded in `GeneralUpdate.Core`, **no need** to separately install the `GeneralUpdate.Differential` package.
差分类型已内嵌在 `GeneralUpdate.Core` 中，**无需额外**安装 `GeneralUpdate.Differential` 包。

### DiffPipelineBuilder Approach (Recommended) / DiffPipelineBuilder 方式（推荐）

```csharp
using GeneralUpdate.Core.Pipeline;

var pipeline = new DiffPipelineBuilder()
    .UseDiffer(new StreamingHdiffDiffer())     // Diff algorithm / 差分算法
    .UseCleanMatcher(new DefaultCleanMatcher()) // File matcher (server side) / 文件匹配器（服务端）
    .UseDirtyMatcher(new DefaultDirtyMatcher()) // File matcher (client side) / 文件匹配器（客户端）
    .WithParallelism(4)
    .WithStopOnFirstError(true)
    .WithProgress(new Progress<DiffProgress>(p =>
        Console.WriteLine($"[{p.Completed}/{p.Total}] {p.FileName}")))
    .Build();

// Server side: generate patches / 服务端：生成补丁
await pipeline.CleanAsync(oldDir, newDir, patchDir);

// Client side: apply patches / 客户端：应用补丁
await pipeline.DirtyAsync(appDir, patchDir);
```

### Bootstrap Integration Approach / Bootstrap 集成方式

```csharp
new GeneralUpdateBootstrap()
    .SetConfig(config)
    .UseDiffPipeline(pipeline =>
    {
        pipeline.WithParallelism(2)
                .WithStopOnFirstError(true);
    })
    .LaunchAsync();
```

### Custom Matchers / 自定义匹配器

```csharp
using GeneralUpdate.Differential.Matchers;

// Custom clean matcher / 自定义清理匹配器
var cleanMatcher = new DefaultCleanMatcher();  // or implement ICleanMatcher / 或实现 ICleanMatcher
var dirtyMatcher = new DefaultDirtyMatcher();  // or implement IDirtyMatcher / 或实现 IDirtyMatcher
```

---

## 7. AOT / NativeAOT Compatibility / AOT / NativeAOT 兼容性

GeneralUpdate.Core v10.5.0-rc.1 supports .NET Native AOT (`net8.0` and `net10.0`):
GeneralUpdate.Core v10.5.0-rc.1 支持 .NET Native AOT（`net8.0` 和 `net10.0`）：

```xml
<PropertyGroup>
  <IsAotCompatible>true</IsAotCompatible>
  <EnableTrimAnalyzer>true</EnableTrimAnalyzer>
</PropertyGroup>
```

JSON serialization context (reduces AOT size) / JSON 序列化上下文（减少 AOT 大小）：

```csharp
using GeneralUpdate.Core.JsonContext;

// Use built-in JsonSerializerContext / 使用内置的 JsonSerializerContext
// VersionRespJsonContext, ProcessContractJsonContext, HttpParameterJsonContext, etc.
// VersionRespJsonContext, ProcessContractJsonContext, HttpParameterJsonContext 等
```

---

## 8. Drivelution (Windows Driver Updates) / Drivelution（Windows 驱动更新）

The `GeneralUpdate.Drivelution` package provides Windows driver management:
`GeneralUpdate.Drivelution` 包提供 Windows 驱动管理：

```csharp
using GeneralUpdate.Drivelution;

// Scan driver directory / 扫描驱动目录
var allDrivers = GeneralDrivelution.ScanDirectory(driverDir);

// Validate driver / 验证驱动
var isValid = GeneralDrivelution.ValidateDriver(driverPath);

// Install driver (DIFx → SetupAPI → PnPUtil cascade) / 安装驱动（DIFx → SetupAPI → PnPUtil 级联）
var result = GeneralDrivelution.InstallDriver(driverPath);
```

---

## Content Index / 内容索引

| Topic / 主题 | Availability / 可用性 | Reference / 参考 |
|------|--------|------|
| Pipeline | ✅ v10.5.0-rc.1 | `GeneralUpdate.Core.Pipeline` |
| Strategy System / 策略系统 | ✅ v10.5.0-rc.1 | `GeneralUpdate.Core.Strategy` |
| FileTree | ✅ v10.5.0-rc.1 | `GeneralUpdate.Core.FileSystem` |
| BlackList / 黑名单 | ✅ v10.5.0-rc.1 | `UpdateRequest.Files/Formats/Directories` → `ToBlackPolicy()` |
| Differential Engine / 差分引擎 | ✅ Embedded in Core / 内嵌 Core | `DiffPipelineBuilder` / `DiffPipeline` |
| AOT | ✅ v10.5.0-rc.1 | `JsonSerializerContext` subclasses / 子类 |
| EventManager | ✅ v10.5.0-rc.1 | `GeneralUpdate.Core.Event` |
| Bowl Crash Daemon / Bowl 崩溃守护 | ⚠️ Basic types / 基础类型 | `GeneralUpdate.Bowl.Bowl` |
| IUpdateHooks | ✅ v10.5.0-rc.1 | `GeneralUpdate.Core.Hooks` — `Hooks<T>()` |
| Custom Strategy Injection / 自定义 Strategy 注入 | ✅ v10.5.0-rc.1 | `Strategy<T>()` |
| IPC Replacement Interface / IPC 替换接口 | ❌ Not yet supported / 暂不支持 | Use NamedPipe alternative / 使用 NamedPipe 替代方案 |
| SilentPollOrchestrator | ✅ v10.5.0-rc.1 | `Option.Silent` + `SetOption()` |
| Option System / Option 系统 | ✅ v10.5.0-rc.1 | `SetOption<T>(Option<T>, T)` |

---

## Advanced Customization Verification Checklist / 高级定制验证清单

### Bowl Crash Daemon / Bowl 崩溃守护
- [ ] If using Bowl: reference both `GeneralUpdate.Core` and `GeneralUpdate.Bowl` in the project (no conflicts in v10.5.0-rc.1)
      如果用 Bowl：项目中同时引用 `GeneralUpdate.Core` 和 `GeneralUpdate.Bowl`（v10.5.0-rc.1 无冲突）
- [ ] `MonitorParameter.ProcessNameOrId` matches the actual process name / `MonitorParameter` 的 `ProcessNameOrId` 与实际进程名匹配
- [ ] `TargetPath` set to app installation root directory, not a subdirectory / `TargetPath` 设置为应用安装根目录，非子目录
- [ ] `WorkModel` correctly chosen by scenario (Normal = main process / Upgrade = upgrade process)
      `WorkModel` 根据场景正确选择（Normal=主进程 / Upgrade=升级进程）
- [ ] `FailDirectory` has write permission / `FailDirectory` 有写入权限
- [ ] Linux/macOS do not have this feature (Bowl is Windows only) / Linux/macOS 无此功能（Bowl 仅 Windows）

### Pipeline Customization / Pipeline 定制
- [ ] Key names in `PipelineContext` are spelled correctly using string constants ("ZipFilePath", "Hash", "Format", "Encoding", "SourcePath", "PatchEnabled")
      `PipelineContext` 中的 Key 名称使用字符串常量拼写正确（"ZipFilePath", "Hash", "Format", "Encoding", "SourcePath", "PatchEnabled"）
- [ ] Middleware registration order is correct: Hash → Compress → Patch → Drivelution
      中间件注册顺序正确：Hash → Compress → Patch → Drivelution
- [ ] `Encoding` set to `Encoding.UTF8` / `Encoding` 设置为 `Encoding.UTF8`

### AOT/NativeAOT
- [ ] `<IsAotCompatible>true</IsAotCompatible>` enabled / 启用了 `<IsAotCompatible>true</IsAotCompatible>`
- [ ] `[DynamicDependency]` or `[RequiresUnreferencedCode]` added for reflection paths
      对反射路径添加了 `[DynamicDependency]` 或 `[RequiresUnreferencedCode]`
- [ ] Built-in `JsonSerializerContext` subclasses used (reduces trimming) / 使用了内置的 `JsonSerializerContext` 子类（减少裁剪）
- [ ] No AOT trim warnings from `dotnet build` / 通过 `dotnet build` 无 AOT 裁剪警告

### IPC Replacement / IPC 替换
- [ ] Replacement solution is available on the target platform (Linux has no NamedPipe server, but has client)
      替换方案在目标平台上可用（Linux 无 NamedPipe 服务端，但有客户端）
- [ ] Encryption scheme is consistent on both Client and Upgrade sides
      加密方案与 Client/Upgrade 两端一致
- [ ] IPC data length has upper bound protection (prevents memory overflow) / IPC 数据长度有上限保护（防止内存溢出）

---

## Anti-Pattern Checklist (Advanced Customization Specific) / 反模式清单（高级定制特有）

| # | Anti-Pattern / 反模式 | Consequence / 后果 | Correct Approach / 正确做法 |
|---|--------|------|---------|
| 1 | **Using dev branch APIs (IUpdateHooks, etc.) on v10.4.6 stable / 在 v10.4.6 稳定版上使用开发分支 API（IUpdateHooks 等）** | Build failure / MissingMethodException at runtime / 编译失败 / 运行时 MissingMethodException | Check the API availability table / 检查 API 可用性表 |
| 2 | **PipelineContext Key typos (e.g., ZipFilePath typed as ZipFilePatch) / PipeLineContext Key 拼写错误（如 ZipFilePath 写成 ZipFilePatch）** | Pipeline runs abnormally, values not passed / Pipeline 运行异常，值未传递 | Use constants exposed by the library or Key names from the docs / 使用类库公开的常量或文档中的 Key 名 |
| 3 | **Bowl WorkModel set to Upgrade but the process is the main app / Bowl 的 WorkModel 设为 Upgrade 但进程是主程序** | Incorrect monitoring logic / 监控逻辑错误 | Normal = main process, Upgrade = upgrade process / Normal=主线进程，Upgrade=升级进程 |
| 4 | **Using default encryption key for IPC on Windows / Windows 上 IPC 使用默认加密密钥** | Encryption is crackable (code audit #1) / 加密可被破解（代码审计 #1） | Use a strong key (>= 32 characters) / 使用强密钥（≥ 32 字符） |
| 5 | **Using different source file structures when generating diff packages / 差分包生成时使用不同版本的源文件结构** | Patch application fails, file not found / 补丁应用失败，文件找不到 | Source and target version file structures must be consistent / 源和目标版本的文件结构必须一致 |
| 6 | **Heavy reflection in AOT project without DynamicDependency annotation / AOT 项目中使用了大量反射且未标记 DynamicDependency** | Runtime TypeLoadException / trimmed away / 运行时 TypeLoadException / 被剪裁 | Use source generators or explicitly mark for retention / 使用源代码生成器或显式标记保留 |
| 7 | **PatchMiddleware placed before CompressMiddleware in Pipeline / Pipeline 中 PatchMiddleware 排在 CompressMiddleware 前面** | Attempting to patch before decompressing / 未解压就试图打补丁 | Order must be Compress → Patch / 顺序必须是 Compress→Patch |
| 8 | **Custom Strategy directly manipulates private methods / 自定义 Strategy 直接操作 private 方法** | API compatibility breaks on downstream version updates / 下游版本更新后 API 兼容性破裂 | Extend through protected abstract methods / 通过受保护的抽象方法扩展 |

---

## Related Skills / 相关技能

- `/generalupdate-init` — Bootstrap configuration / Bootstrap 配置
- `/generalupdate-strategy` — Update strategy selection / 更新策略选择
- `/generalupdate-troubleshoot` — Issue diagnosis / 问题诊断
- `/generalupdate-mobile` — Mobile auto-update integration / 移动端自动更新集成
- `/generalupdate-security-audit` — Security audit / 安全审计
