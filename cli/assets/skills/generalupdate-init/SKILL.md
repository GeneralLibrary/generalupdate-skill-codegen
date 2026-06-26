---
name: generalupdate-init
description: |
  Integrate GeneralUpdate auto-update into any .NET application. Generates Bootstrap
  configuration code, manifest files, and dual-project (Client+Upgrade) scaffolding.
  Covers 4 update scenes, UpdateRequest configuration, appsettings.json, HTTP auth (HMAC/Basic/Bearer),
  and complete deployment checklist. Triggers on: "add auto update", "integrate GeneralUpdate",
  "configure bootstrap", "我需要自动更新", "配置更新", "初始化GeneralUpdate", "添加更新功能",
  "接入更新", "升级框架". Also triggers when user mentions their project type + update.
  Always pair with generalupdate-ui if a UI framework is detected, and with
  generalupdate-strategy if the user asks about different update approaches.
when_to_use: |
  - First-time integration of GeneralUpdate into a .NET project
  - User wants Bootstrap configuration code (Minimal or Full)
  - User needs the Client + Upgrade dual-project structure explained
  - User asks about manifest.json, UpdateRequest, or generalupdate.manifest.json
  - User mentions their specific .NET framework (WPF/WinForms/Avalonia/MAUI/console)
  - User asks about deployment considerations or CI/CD integration
  - Best used as the entry point; guide to other skills as needed
allowed-tools: "Bash, Read, Write, Edit, Glob, Grep, WebSearch"
---

# GeneralUpdate Integration Complete Guide / GeneralUpdate 集成完全指南

A complete guide to integrating GeneralUpdate auto-update into any .NET application. Covers all configuration methods, deployment scenarios, and production considerations from scratch.

帮助开发者在任意 .NET 应用中集成 GeneralUpdate 自动更新。从零开始，覆盖所有配置方式、部署场景和生产环境考量。

> **Targeting NuGet v10.5.0-rc.1**. `Configinfo` has been replaced by `UpdateRequest`, and namespaces have moved to `GeneralUpdate.Core.Configuration`.
> ⚠️ **针对 NuGet v10.5.0-rc.1**。`Configinfo` 已被 `UpdateRequest` 替代，命名空间已移至 `GeneralUpdate.Core.Configuration`。

---

## Requirements Extraction / 用户需求提取

Before generating code, you must extract the following information. **If uncertain, ask the user:**

在生成代码前，必须先提取以下信息。**不确定的必须追问：**

```
### Project Status / 项目状态
- Existing project type / 现有项目类型: ______ (New / Existing / Migrating from legacy / 
  新项目 / 已有项目 / 从旧版迁移)
- .NET version / .NET 版本: ______
- UI framework / UI 框架: ______ (WPF / WinForms / Avalonia / MAUI / Console / None /
  WPF/WinForms/Avalonia/MAUI/控制台/无)
- Target platform / 目标平台: ______ (Windows / Linux / macOS / Multi-platform /
  Windows/Linux/macOS/多平台)

### Update Requirements / 更新需求
- Show progress UI / 是否需要显示进度 UI: ______ (Yes/No / 是/否)
- Has backend service / 是否有后端服务: ______ (Yes/No / 是/否)
- Update strategy preference / 更新策略倾向: ______ (Standard / OSS / Silent / Differential / 
  Cross-version / Push / 标准/OSS/静默/差分/跨版本/推送)
- Need Bowl crash daemon / 是否需要崩溃守护 Bowl: ______ (Yes/No / 是/否)

### Existing Configuration (if any) / 已有配置（如果存在）
- NuGet already installed / 是否已安装 NuGet: ______ (Yes/No, version / 是/否，版本号)
- UpdateRequest already configured / 是否已有 UpdateRequest 配置: ______ (Yes/No / 是/否)
- manifest.json already exists / 是否已有 manifest.json: ______ (Yes/No / 是/否)
```

---

## Workflow (Execute in Order) / 工作流程（按顺序执行）

### Step 1: Detect Project State / 探测项目状态

```
├── Check .csproj → Target framework, UI type, NuGet references
├── Check for generalupdate.manifest.json
├── Check for UpdateRequest/Bootstrap configuration code
└── Check project structure → Whether an independent Upgrade project exists
```

```
├── 检查 .csproj → 目标框架、UI 类型、是否有 NuGet 引用
├── 检查是否存在 generalupdate.manifest.json
├── 检查是否存在 UpdateRequest/Bootstrap 配置代码
└── 检查项目结构 → 是否已有独立的 Upgrade 项目
```

### Step 2: Choose Integration Mode / 选择集成模式

Based on the requirements extraction results, choose one of the following modes:

基于需求提取结果，选择以下模式之一：

| Mode | Scenario | Output |
|------|---------|--------|
| **[Minimal]** | Quick start for new users, console/service apps / 新用户快速上手，控制台/服务应用 | 3-line Bootstrap code / 3 行 Bootstrap 代码 |
| **[Standard]** | Need precise control over the update process / 需要精确控制更新过程 | UpdateRequest + full event listeners / UpdateRequest + 完整事件监听 |
| **[Scaffold]** | Team projects, starting from scratch / 团队项目，从零开始 | Full Client + Upgrade dual-project structure / 完整 Client + Upgrade 双项目结构 |

### Step 3: Generate Output / 生成输出

```
├── NuGet install commands (choose Core/Bowl per platform)
├── Bootstrap configuration code (per mode)
├── manifest.json template
├── Deployment checklist
└── Known issue warnings (for your specific configuration combination)
```

```
├── NuGet 安装命令（按平台选 Core/Bowl）
├── Bootstrap 配置代码（按模式）
├── manifest.json 模板
├── 部署检查清单
└── 已知问题预警（针对你的配置组合）
```

### Step 4: Guide Next Steps / 引导下一步

```
├── Need UI → /generalupdate-ui
├── Choose strategy → /generalupdate-strategy
├── Need Bowl daemon → /generalupdate-advanced
└── Encounter issues → /generalupdate-troubleshoot
```

```
├── 需要 UI → /generalupdate-ui
├── 选择策略 → /generalupdate-strategy
├── 需要 Bowl 守护 → /generalupdate-advanced
└── 遇到问题 → /generalupdate-troubleshoot
```

---

## Core Concept: 4 Update Scenarios / 核心概念：4 大更新场景

GeneralUpdate determines the update strategy based on the package type returned by the server:

GeneralUpdate 根据服务端返回的包类型决定更新策略：

| Scenario | Behavior |
|----------|----------|
| **None** | No update needed, launch main app directly / 无需更新，直接启动主程序 |
| **UpgradeOnly** | Update only the upgrade program itself: Client extracts the Upgrade package in-place / 只更新升级程序自身：Client 原地解压 Upgrade 包 |
| **MainOnly** | Update only the main app: Client → IPC → starts Upgrade process / 只更新主程序：Client → IPC → 启动 Upgrade 进程 |
| **Both** | Update both / 两者都更新 |

**Dual-Process Architecture / 双进程架构**：
```
App.exe (Client) is responsible for / App.exe (Client) 负责:
  ├── Version verification (HTTP request to server) / 版本验证（HTTP 请求服务端）
  ├── Downloading all update packages / 下载所有更新包
  ├── IPC write (encrypted file to pass parameters to Upgrade) / IPC 写入（加密文件传递参数给 Upgrade）
  └── Starting Upgrade.exe then exiting itself / 启动 Upgrade.exe 然后自己退出

Upgrade.exe (Upgrade process) is responsible for / Upgrade.exe (Upgrade 进程) 负责:
  ├── Reading IPC file / 读取 IPC 文件
  ├── Applying updates (extract/patch/replace files) / 应用更新（解压/补丁/替换文件）
  └── Starting the main app then exiting itself / 启动主程序然后自己退出
```

---

## UpdateRequest Configuration Details / UpdateRequest 配置详解

### UpdateRequest Full Properties / UpdateRequest 完整属性

```csharp
// Method A: Direct UpdateRequest construction (recommended)
// 方式 A：直接构造 UpdateRequest（推荐）
var config = new UpdateRequest
{
    // === Required / 必需 ===
    UpdateUrl = "https://your-server.com/Upgrade/Verification",
    AppSecretKey = "your-secret-key",
    MainAppName = "MyApp.exe",
    ClientVersion = "1.0.0.0",
    ProductId = "my-product-001",
    InstallPath = ".",
    
    // === Optional / 可选 ===
    ReportUrl = "https://your-server.com/Upgrade/Report",
    UpdateLogUrl = "https://your-server.com/Upgrade/Log",
    UpgradeClientVersion = "1.0.0.0",
    
    // === Security / Authentication / 安全认证 ===
    AuthScheme = AuthScheme.Hmac,  // Hmac / Bearer / ApiKey / Basic
    Token = "your-token",
    BasicUsername = "user",
    BasicPassword = "pass",
    
    // === Blacklist (excluded during backup/copy) / 黑名单（备份/复制时排除）===
    Files = new List<string> { "*.log", "*.tmp" },
    Formats = new List<string> { ".pdb" },
    Directories = new List<string> { "logs", "cache" },
};

// Method B: Builder pattern / 方式 B：使用建造者模式
var config = UpdateRequestBuilder.Create()
    .SetUpdateUrl("https://your-server.com/api")
    .SetAppSecretKey("your-secret-key")
    .SetMainAppName("MyApp.exe")
    .SetClientVersion("1.0.0.0")
    .SetProductId("my-product-001")
    .SetInstallPath(".")
    .Build();

// Method C: Zero-config — auto-discover from manifest.json
// 方式 C：零配置 — 从 manifest.json 自动发现
await new GeneralUpdateBootstrap()
    .SetSource(
        updateUrl: "https://your-server.com/api",
        appSecretKey: "your-secret-key")
    .AddListenerUpdateInfo(...)
    .LaunchAsync();
```

### Application Role (AppType) / 应用角色（AppType）

`AppType` is an enum (v10.5.0-rc.1):

`AppType` 是一个 enum（v10.5.0-rc.1）：

| Value | Name | Description |
|-------|------|-------------|
| 1 | `AppType.Client` | Standard client (main application) / 标准客户端（主程序） |
| 2 | `AppType.Upgrade` | Standard upgrade program / 标准升级程序 |
| 3 | `AppType.OssClient` | OSS client mode (silent) / OSS 客户端模式（静默） |
| 4 | `AppType.OssUpgrade` | OSS upgrade mode / OSS 升级模式 |

### Complete Event Listener Reference / 事件监听器完整清单

```csharp
// All 7 events / 全部 7 个事件
.AddListenerUpdateInfo((_, e) => {
    /* Version verification result (e.Info?.Body contains VersionEntry list)
       版本验证结果（e.Info?.Body 含 VersionEntry 列表） */
})
.AddListenerMultiDownloadStatistics((_, e) => {
    /* Batch download progress (e.ProgressPercentage, e.Speed, e.Remaining)
       批量下载进度（e.ProgressPercentage, e.Speed, e.Remaining） */
})
.AddListenerMultiDownloadCompleted((_, e) => {
    /* Per-version download complete (e.Version, e.IsCompleted)
       每版本下载完成（e.Version, e.IsCompleted） */
})
.AddListenerMultiDownloadError((_, e) => {
    /* Download error (e.Exception, e.Version)
       下载错误（e.Exception, e.Version） */
})
.AddListenerMultiAllDownloadCompleted((_, e) => {
    /* All downloads complete (e.IsAllDownloadCompleted, e.FailedVersions)
       全部下载完成（e.IsAllDownloadCompleted, e.FailedVersions） */
})
.AddListenerException((_, e) => {
    /* Exception (e.Message, e.Exception)
       异常（e.Message, e.Exception） */
})
.AddListenerProgress((_, e) => {
    /* Progress (e.Progress or e.DiffProgress, v10.5+)
       进度（e.Progress 或 e.DiffProgress，v10.5+） */
})
```

---

## Complete Integration Code / 集成方式的完整代码

### Method A: Minimal — Using UpdateRequest / 方式 A：Minimal — 使用 UpdateRequest

```csharp
using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;

var config = new UpdateRequest
{
    UpdateUrl = "https://your-server.com/api",
    AppSecretKey = "your-32-char-secret-key-here!",
    MainAppName = "MyApp.exe",
    ClientVersion = "1.0.0.0",
    ProductId = "my-product-001",
    InstallPath = "."
};

await new GeneralUpdateBootstrap()
    .SetConfig(config)
    .LaunchAsync();
```

### Method B: Standard — UpdateRequest + Event Listeners / 方式 B：Standard — UpdateRequest + 事件监听

```csharp
using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Download;

var config = new UpdateRequest
{
    UpdateUrl = "https://your-server.com/Upgrade/Verification",
    AppSecretKey = "your-secret-key",
    MainAppName = "MyApp.exe",
    ClientVersion = "1.0.0.0",
    ProductId = "my-product-001",
    InstallPath = AppDomain.CurrentDomain.BaseDirectory,
    ReportUrl = "https://your-server.com/Upgrade/Report",
};

await new GeneralUpdateBootstrap()
    .SetConfig(config)
    .AddListenerUpdateInfo((_, e) =>
    {
        Console.WriteLine($"Found {e.Info?.Body?.Count ?? 0} versions / 发现 {e.Info?.Body?.Count ?? 0} 个版本");
    })
    .AddListenerMultiDownloadStatistics((_, e) =>
    {
        Console.WriteLine($"Progress / 进度: {e.ProgressPercentage}% | {e.Speed}");
    })
    .AddListenerMultiDownloadCompleted((_, e) =>
    {
        Console.WriteLine($"Version {e.Version} download complete / 版本 {e.Version} 下载完成");
    })
    .AddListenerMultiAllDownloadCompleted((_, e) =>
    {
        Console.WriteLine($"All complete (IsAllDownloadCompleted={e.IsAllDownloadCompleted}) / 全部完成 (IsAllDownloadCompleted={e.IsAllDownloadCompleted})");
    })
    .AddListenerMultiDownloadError((_, e) =>
    {
        Console.WriteLine($"Download failed / 下载失败: Version / 版本 {e.Version} — {e.Exception?.Message}");
    })
    .AddListenerException((_, e) =>
    {
        Console.WriteLine($"Exception / 异常: {e.Message}");
    })
    .LaunchAsync();
```

### Upgrade Process Configuration / Upgrade 进程配置

```csharp
using GeneralUpdate.Core;

// Upgrade mode reads configuration from IPC file, no SetConfig needed
// Upgrade 模式从 IPC 文件读取配置，无需 SetConfig
await new GeneralUpdateBootstrap()
    .AddListenerException((_, e) =>
        Console.WriteLine($"Error / 错误: {e.Message}"))
    .LaunchAsync();
```

---

## Production Deployment Checklist / 生产环境部署检查清单

### Publish Directory Structure / 发布目录结构

```
publish/
├── MyApp.exe                  ← MainAppName (main program / 主程序)
├── generalupdate.manifest.json
└── update/
    └── UpgradeApp.exe         ← Upgrade program, must ship with first release
                                 升级程序，必须随首个版本发布
```

### Dual-Process Verification / 双进程验证

| Check Item | Description |
|------------|-------------|
| UpgradeApp.exe present in publish directory / UpgradeApp.exe 存在于发布目录 | Must exist even in first release / 首个版本就必须有 |
| Client and Upgrade use the same AppSecretKey / Client 和 Upgrade 使用相同 AppSecretKey | IPC encrypted communication depends on this key / IPC 加密通信依赖此 Key |
| Client and Upgrade use the same NuGet version / Client 和 Upgrade 使用相同 NuGet 版本号 | Version mismatch causes "Method not found" / 版本不一致导致 "Method not found" |
| Upgrade process does not need network / Upgrade 进程不需要网络 | All data is pre-downloaded by Client / 所有数据由 Client 预下载 |

---

## Known Issues / 已知问题

### NuGet Notes (v10.5.0-rc.1) / NuGet 注意事项（v10.5.0-rc.1）
`GeneralUpdate.Core` and `GeneralUpdate.Bowl` **can be referenced together** (no CS0433 conflict in v10.5.0-rc.1).

`GeneralUpdate.Core` 和 `GeneralUpdate.Bowl` **可以同时引用**（v10.5.0-rc.1 中无 CS0433 冲突）。

- Using Core: `dotnet add package GeneralUpdate.Core`
- Using Bowl: `dotnet add package GeneralUpdate.Bowl` (it does **not** transitively depend on Core; reference Core separately / 它**不**传递依赖 Core，需要同时引用 Core)
- Differential types are embedded in Core; **no need** for extra `GeneralUpdate.Differential` package / 差分类型已内嵌在 Core，**无需额外** `GeneralUpdate.Differential` 包

### Stable Release Feature Enhancements / 稳定版功能增强
New features in v10.5.0-rc.1:

v10.5.0-rc.1 新增以下功能：

- `IUpdateHooks` lifecycle hooks — `Hooks<T>()` / `IUpdateHooks` 生命周期钩子 — `Hooks<T>()`
- Programmable `Option` system — `SetOption(Option.Silent, true)` / 可编程 `Option` 系统 — `SetOption(Option.Silent, true)`
- `SilentPollOrchestrator` silent polling / `SilentPollOrchestrator` 静默轮询
- `SetSource()` zero-config entry / `SetSource()` 零配置入口
- `UseDiffPipeline()` differential pipeline configuration / `UseDiffPipeline()` 差分管道配置
- `AddListenerProgress()` 7th event / `AddListenerProgress()` 第 7 个事件
- `IStrategy` custom strategy injection — `Strategy<T>()` / `IStrategy` 自定义策略注入 — `Strategy<T>()`
- `IUpdateReporter` / `IHttpAuthProvider` and other extension points / `IUpdateReporter` / `IHttpAuthProvider` 等扩展点

---

## Integration Verification Checklist (Check Each Item Before Delivery) / 集成验证清单（交付前逐项检查）

### Bootstrap Configuration / Bootstrap 配置
- [ ] All 6 required fields of `UpdateRequest` are set (UpdateUrl, AppSecretKey, MainAppName, ClientVersion, ProductId, InstallPath) / `UpdateRequest` 的 6 个必填字段都已设置（UpdateUrl, AppSecretKey, MainAppName, ClientVersion, ProductId, InstallPath）
- [ ] `UpdateUrl` points to a server API that correctly returns version information / `UpdateUrl` 指向的服务端 API 可正常返回版本信息
- [ ] `AppSecretKey` length >= 16 characters, consistent with server / `AppSecretKey` 长度 ≥ 16 字符，与服务端一致
- [ ] `AppType` set correctly (Client = 1, Upgrade = 2) / `AppType` 设置正确（Client = 1, Upgrade = 2）
- [ ] Production uses `AppDomain.CurrentDomain.BaseDirectory` as InstallPath / 生产环境使用 `AppDomain.CurrentDomain.BaseDirectory` 作为 InstallPath

### NuGet & Compilation / NuGet & 编译
- [ ] Client and Upgrade projects use the **exact same** GeneralUpdate NuGet version / Client 和 Upgrade 项目使用**完全相同**的 GeneralUpdate NuGet 版本
- [ ] If using Bowl: reference both `GeneralUpdate.Core` and `GeneralUpdate.Bowl` in the project (no conflict in v10.5.0-rc.1) / 如果用 Bowl：项目中同时引用 `GeneralUpdate.Core` 和 `GeneralUpdate.Bowl`（v10.5.0-rc.1 无冲突）
- [ ] Project builds with `dotnet build` (0 errors) / 项目能正常 `dotnet build`（0 errors）
- [ ] No extra reference to `GeneralUpdate.Differential` needed (embedded in Core) / 无需额外引用 `GeneralUpdate.Differential`（已嵌入 Core）

### Deployment Structure / 部署结构
- [ ] UpgradeApp.exe present in publish directory under `update/` subdirectory (must exist even in first release) / UpgradeApp.exe 存在于发布目录 update/ 子目录中（首个版本就必须有）
- [ ] `generalupdate.manifest.json` `UpdateAppName` includes `.exe` / `generalupdate.manifest.json` 的 `UpdateAppName` 包含 `.exe`
- [ ] IPC file (`UpdateInfo.msg`) path is consistent between Client and Upgrade / IPC 文件（`UpdateInfo.msg`）路径在 Client/Upgrade 间一致
- [ ] `Encoding` set to `Encoding.UTF8` (prevents garbled Chinese characters on Linux/macOS) / `Encoding` 设置为 `Encoding.UTF8`（防止 Linux/macOS 中文乱码）

### Migration Scenario (Upgrading from v9.x) / 迁移场景（从 v9.x 升级）
- [ ] Check old code for non-existent methods like `SetSource()`, `SetOption()`, `Hooks<T>()` / 检查旧代码中是否有 `SetSource()` / `SetOption()` / `Hooks<T>()` 等不存在的方法
- [ ] `AppType` was originally a class in v10.4.6, not an enum: `ClientApp = 1`, `UpgradeApp = 2` / `AppType` 原来是 enum 吗？v10.4.6 中是 class，`ClientApp = 1`, `UpgradeApp = 2`
- [ ] `LaunchAsync()` returns `Task<GeneralUpdateBootstrap>` in v10.4.6 (not `Task<bool>`) / `LaunchAsync()` 在 v10.4.6 中返回 `Task<GeneralUpdateBootstrap>`（不是 `Task<bool>`）
- [ ] Remove `OssClient`-related references (not supported in v10.4.6) / 删除 `OssClient` 相关引用（v10.4.6 不支持）

---

## Anti-Pattern Checklist / 反模式清单

| # | Anti-Pattern | Consequence | Correct Approach |
|---|-------------|-------------|-----------------|
| 1 | **Core and Bowl NuGet versions differ / Core 和 Bowl NuGet 版本不一致** | Runtime MethodNotFoundException | Use the same NuGet version / 使用相同 NuGet 版本 |
| 2 | **Bowl missing `GeneralUpdate.Core` reference / Bowl 缺少 `GeneralUpdate.Core` 引用** | Compilation fails, missing Core types / 编译失败，缺少 Core 类型 | Bowl does not transitively depend on Core; reference Core separately / Bowl 不传递依赖 Core，需同时引用 Core |
| 3 | **Misunderstanding that Bowl transitively depends on Core / Bowl 传递依赖 Core 的误解** | Compilation fails / 编译失败 | In v10.5.0-rc.1, Bowl is an independent package; reference Core separately / v10.5.0-rc.1 中 Bowl 是独立包，需单独引用 Core |
| 4 | **Client/Upgrade NuGet version mismatch / Client/Upgrade NuGet 版本号不一致** | Runtime MethodNotFoundException | Lock to the exact same version / 锁定完全相同版本 |
| 5 | **Blocking operations in event listeners (network IO / disk IO) / 事件监听中做耗时操作（网络 IO / 磁盘 IO）** | Upgrade process UI freezes, times out and gets killed / Update 进程 UI 卡死，超时被 Kill | Only update UI state; async for blocking operations / 仅更新 UI 状态，耗时操作异步 |
| 6 | **IPC file encoding not set to UTF-8 / IPC 文件编码未设置 UTF-8** | Garbled Chinese characters on Linux/macOS / Linux/macOS 中文乱码 | `Encoding.UTF8` |
| 7 | **Version not in 4-part format (e.g., 1.0.0.0) / 版本号不是 4 段式（如 1.0.0.0）** | Version comparison logic breaks / 版本比较逻辑异常 | Always use `x.y.z.w` format / 始终用 `x.y.z.w` 格式 |
| 8 | **manifest.json mainAppName does not match actual process name / manifest.json 的 mainAppName 不匹配真实进程名** | Main app not found after update / 更新后主程序找不到 | Match the actual exe name / 和实际 exe 名称一致 |
| 9 | **Code written for v9.x used directly in v10 / 为 v9.x 编写的代码直接用在 v10** | API incompatibility, compilation fails / API 不兼容，编译失败 | Rewrite against v10.4.6 stable API / 对照 v10.4.6 稳定版 API 重写 |

---

## Related Skills / 相关技能

- `/generalupdate-ui` — UI framework auto-detection + update window code generation / UI 框架自动检测 + 更新窗口代码生成
- `/generalupdate-strategy` — 6 update strategies selection & configuration / 6 种更新策略选择与配置
- `/generalupdate-advanced` — Advanced customization / 高级定制
- `/generalupdate-troubleshoot` — Known issue diagnostics / 已知问题诊断
- `/generalupdate-migration` — Migration from older versions / 从旧版本迁移
- `/generalupdate-mobile` — Mobile auto-update integration / 移动端自动更新集成
