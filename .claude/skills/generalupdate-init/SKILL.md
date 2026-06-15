---
name: generalupdate-init
description: |
  Integrate GeneralUpdate auto-update into any .NET application. Generates Bootstrap
  configuration code, manifest files, and dual-project (Client+Upgrade) scaffolding.
  Covers 4 update scenes, 3 configuration methods, appsettings.json, HTTP auth (HMAC/Basic/Bearer),
  and complete deployment checklist. Triggers on: "add auto update", "integrate GeneralUpdate",
  "configure bootstrap", "我需要自动更新", "配置更新", "初始化GeneralUpdate", "添加更新功能",
  "接入更新", "升级框架". Also triggers when user mentions their project type + update.
  Always pair with generalupdate-ui if a UI framework is detected, and with
  generalupdate-strategy if the user asks about different update approaches.
when_to_use: |
  - First-time integration of GeneralUpdate into a .NET project
  - User wants Bootstrap configuration code (Minimal or Full)
  - User needs the Client + Upgrade dual-project structure explained
  - User asks about manifest.json, UpdateRequest, or Option configuration
  - User mentions their specific .NET framework (WPF/WinForms/Avalonia/MAUI/console)
  - User asks about deployment considerations or CI/CD integration
  - Best used as the entry point; guide to other skills as needed
allowed-tools: "Bash, Read, Write, Edit, Glob, Grep, WebSearch"
---

# 🚀 GeneralUpdate 集成完全指南

帮助开发者在任意 .NET 应用中集成 GeneralUpdate 自动更新。从零开始，覆盖所有配置方式、部署场景和生产环境考量。

---

## 工作流程

```
1. 探测项目状态
   ├── 检查 .csproj → 目标框架、UI 类型
   └── 检查现有配置 → 已安装 NuGet？已有 manifest？
   
2. 选择集成模式
   ├── [Minimal] （3行代码 + manifest.json）— 推荐新用户
   ├── [Standard]（UpdateRequest + Option + 事件）— 需要精细控制
   ├── [appsettings]（LoadFromConfiguration）— 配置与代码分离
   └── [Scaffold]（完整双项目结构）— 从零开始的团队项目

3. 生成输出
   ├── NuGet 安装命令 + 包选择
   ├── Bootstrap 配置代码
   ├── manifest.json 模板
   └── 部署检查清单

4. 后续引导
   ├── 需要 UI → generalupdate-ui
   ├── 选择策略 → generalupdate-strategy
   └── 遇到问题 → generalupdate-troubleshoot
```

---

## 配置方式对比

| 方式 | 代码量 | 灵活性 | 适用场景 |
|------|--------|--------|---------|
| `SetSource(url, key)` | 3行 | 低 | 快速原型、有 manifest.json 的场景 |
| `SetConfig(request)` + Option | 10-30行 | 高 | 生产环境、需要精细控制 |
| `LoadFromConfiguration(IConfiguration)` | 1行 | 中 | aspnetcore 风格配置分离 |
| 双项目 Scaffold | 完整项目 | 最高 | 团队新项目、CI/CD 集成 |

---

## 核心概念：4 大更新场景（必须理解）

GeneralUpdate 根据服务端返回的包类型决定更新策略。这是最常见的误解来源（Issue #465、#475）：

| 场景 | `HasMainUpdate` | `HasUpgradeUpdate` | 行为 |
|------|----------------|-------------------|------|
| **None** | false | false | 无需更新，直接启动主程序 |
| **UpgradeOnly** | false | true | **只更新升级程序自身**：Client 原地解压 Upgrade 包，不启动升级进程 |
| **MainOnly** | true | false | **只更新主程序**：Client → IPC → 启动 Upgrade 进程替换主程序文件 |
| **Both** | true | true | **两者都更新**：先原地更新 Upgrade.exe → IPC → 升级进程替换主程序 |

**⚠️ 关键规则**：场景判断由 `ClientStrategy` 根据服务端响应自动进行。如果服务端返回了 `AppType=Client` 的包则 `HasMainUpdate=true`，返回 `AppType=Upgrade` 的包则 `HasUpgradeUpdate=true`。

### 双进程架构（开发者需要理解的核心理念）

```
App.exe (Client) 负责:
  ├── 版本验证（HTTP 请求服务端）
  ├── 下载所有更新包
  ├── 备份当前安装
  ├── IPC 写入（加密文件传递参数给 Upgrade）
  └── 启动 Upgrade.exe 然后自己退出

Upgrade.exe (Upgrade 进程) 负责:
  ├── 读取 IPC 文件（不从网络获取任何数据）
  ├── 应用更新（解压/补丁/替换文件）
  ├── 版本号回写到 manifest.json
  └── 启动主程序（App.exe）然后自己退出
```

**为什么需要两个进程？** — 运行中的 .exe 无法替换自身文件。

---

## API 配置详解

### AppType 枚举

```csharp
public enum AppType
{
    Client = 0,      // 标准客户端（主程序）
    Upgrade = 1,     // 标准升级程序
    OssClient = 2,   // OSS 存储客户端
    OssUpgrade = 3   // OSS 存储升级端
}
```

### Option 完整列表

```csharp
.SetOption(Option.AppType, AppType.Client)           // [必需] 应用角色
.SetOption(Option.MaxConcurrency, 3)                 // 并行下载数（默认 3）
.SetOption(Option.PatchEnabled, false)               // 差分更新（需安装 Differential 包）
.SetOption(Option.BackupEnabled, false)              // 更新前备份（v5+ 默认关闭）
.SetOption(Option.Silent, false)                     // 静默后台轮询
.SetOption(Option.SilentPollIntervalMinutes, 60)     // 静默轮询间隔（分钟）
.SetOption(Option.Format, CompressionFormat.Zip)     // 压缩格式（Zip/Brotli/GZip）
.SetOption(Option.Encoding, CompressionEncoding.Default) // 压缩编码
.SetOption(Option.DownloadTimeout, 60)               // 下载超时（秒）
.SetOption(Option.MaxRetryCount, 3)                  // 下载重试次数
.SetOption(Option.EnableResume, true)                // 断点续传
.SetOption(Option.VerifyChecksum, true)              // 下载后校验 SHA256
```

### 5 种 HTTP 认证方式

```csharp
// 1. HMAC 认证（默认，与 GeneralSpacestation 服务端配合）
//    UpdateRequest 中设置了 AppSecretKey 即自动生效
new UpdateRequest { AppSecretKey = "your-key" };

// 2. Basic 认证
new UpdateRequest { BasicUsername = "user", BasicPassword = "pass" };

// 3. Bearer Token（通过自定义 HttpAuthProvider）
bootstrap.HttpAuthProvider<BearerAuthProvider>();

// 4. API Key Header
bootstrap.HttpAuthProvider<ApiKeyAuthProvider>();

// 5. 自定义认证
bootstrap.HttpAuthProvider<MyCustomAuthProvider>();
```

### 事件监听器完整清单

```csharp
// ===== Client 进程可用事件 =====
.AddListenerUpdateInfo((_, e) => { /* 收到版本验证结果 */ })
.AddListenerMultiDownloadStatistics((_, e) => { /* 批量下载进度 */ })
.AddListenerMultiDownloadCompleted((_, e) => { /* 每版本下载完成 */ })
.AddListenerMultiDownloadError((_, e) => { /* 单文件下载错误 */ })
.AddListenerMultiAllDownloadCompleted((_, e) => { /* 全部下载完成 */ })
.AddListenerProgress((_, e) => { /* 单文件处理进度（含解压/补丁） */ })
.AddListenerException((_, e) => { /* 更新异常 */ })
.AddListenerCheckFailed((_, e) => { /* 版本检查失败 */ })

// ===== Upgrade 进程可用事件 =====
.AddListenerProgress((_, e) => { /* 文件处理进度 */ })
.AddListenerException((_, e) => { /* 处理异常 */ })
```

---

## 3 种集成方式的完整代码

### 方式 A：Minimal — 3行代码 + manifest.json

```csharp
using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Enum;

var result = await new GeneralUpdateBootstrap()
    .SetSource("https://your-server.com/api/", "your-32-char-secret-key-here!")
    .SetOption(Option.AppType, AppType.Client)
    .LaunchAsync();
```

**对应的 `generalupdate.manifest.json`**：
```json
{
  "MainAppName": "MyApp.exe",
  "UpdateAppName": "UpgradeApp.exe",
  "ProductId": "my-product-001",
  "InstallPath": ".",
  "UpdatePath": "update",
  "ClientVersion": "1.0.0.0",
  "UpgradeClientVersion": "1.0.0.0"
}
```

**⚠️ manifest.json 关键要求**（来自 Issue #484、#475、#501）：
- `ClientVersion` 和 `UpgradeClientVersion` **必须填写实际版本号**，不能为空（空 = "0.0.0.0" 导致无限升级循环）
- `MainAppName` 必须包含扩展名（如 `MyApp.exe`）
- `InstallPath` 为 `.` 或绝对路径
- 文件必须放在应用程序根目录，文件名不能改变

### 方式 B：Standard — UpdateRequest + Option + 事件

```csharp
using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Enum;

var request = new UpdateRequest
{
    // === 必需 ===
    UpdateUrl = "https://your-server.com/Upgrade/Verification",
    AppSecretKey = "your-secret-key",

    // === 自动发现字段（也可从 manifest.json 读取）===
    InstallPath = AppDomain.CurrentDomain.BaseDirectory,
    UpdatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "update"),
    ClientVersion = "1.0.0.0",
    MainAppName = "MyApp.exe",
    UpdateAppName = "UpgradeApp.exe",
    ProductId = "my-product-001",

    // === 可选 ===
    ReportUrl = "https://your-server.com/Upgrade/Report",      // 状态上报
    UpdateLogUrl = "https://your-server.com/Upgrade/Log",      // 更新日志
};

var bootstrap = new GeneralUpdateBootstrap()
    .SetConfig(request)
    .SetOption(Option.AppType, AppType.Client)
    .SetOption(Option.MaxConcurrency, 3)
    .SetOption(Option.PatchEnabled, false)
    .SetOption(Option.BackupEnabled, false)
    .SetOption(Option.DownloadTimeout, 60)

    // 事件：版本发现
    .AddListenerUpdateInfo((_, e) =>
    {
        Console.WriteLine($"发现版本: {e.Version}");
        Console.WriteLine($"文件数: {e.FileCount}, 大小: {e.Size}");
    })
    // 事件：批量下载进度
    .AddListenerMultiDownloadStatistics((_, e) =>
    {
        Console.WriteLine($"进度: {e.ProgressPercentage}% | {e.Speed}/s | 剩余: {e.Remaining}");
    })
    // 事件：每版本下载完成
    .AddListenerMultiDownloadCompleted((_, e) =>
    {
        Console.WriteLine($"版本 {e.Versions?.LastOrDefault()?.Version} 下载完成");
    })
    // 事件：全部下载完成
    .AddListenerMultiAllDownloadCompleted((_, e) =>
    {
        Console.WriteLine("全部下载完成，启动升级程序");
    })
    // 事件：单文件下载错误
    .AddListenerMultiDownloadError((_, e) =>
    {
        Console.WriteLine($"下载失败: {e.FileName} — {e.Exception?.Message}");
    })
    // 事件：单文件处理进度
    .AddListenerProgress((_, e) =>
    {
        Console.WriteLine($"处理 {e.FileName}: {e.ProgressValue}% ({e.Type})");
    })
    // 事件：异常
    .AddListenerException((_, e) =>
    {
        Console.WriteLine($"异常: {e.Message}");
    });

var result = await bootstrap.LaunchAsync();
// true → 更新完成，进程重启中（不会执行到这里）
// false → 已是最新版本
```

### 方式 C：appsettings.json 配置分离

```json
// appsettings.json
{
  "GeneralUpdate": {
    "UpdateUrl": "https://your-server.com/Upgrade/Verification",
    "ReportUrl": "https://your-server.com/Upgrade/Report",
    "AppSecretKey": "your-secret-key",
    "ClientVersion": "1.0.0.0",
    "MainAppName": "MyApp.exe",
    "UpdateAppName": "UpgradeApp.exe",
    "ProductId": "my-product-001",
    "MaxConcurrency": 3,
    "PatchEnabled": false,
    "BackupEnabled": true,
    "DownloadTimeout": 60
  }
}
```

```csharp
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddJsonFile("appsettings.json")
    .Build();

var bootstrap = new GeneralUpdateBootstrap()
    .LoadFromConfiguration(config.GetSection("GeneralUpdate"))
    .SetOption(Option.AppType, AppType.Client);
```

---

## 生产环境部署检查清单

部署前逐项检查：

### 📦 发布目录结构

```
publish/
├── MyApp.exe                  ← MainAppName（主程序）
├── generalupdate.manifest.json ← 必须存在
├── appsettings.json           ← 可选（配置分离时使用）
└── update/
    └── UpgradeApp.exe         ← UpdateAppName（升级程序，**必须**随首个版本发布）
```

### ✅ 双进程验证

| 检查项 | 是否通过 | 说明 |
|--------|---------|------|
| UpgradeApp.exe 存在于发布目录 | □ | 首个版本就必须有，不能自己下载自己 |
| Client 和 Upgrade 使用相同 AppSecretKey | □ | IPC 加密通信依赖此 Key |
| Client 和 Upgrade 使用相同 NuGet 版本号 | □ | 版本不一致导致 "Method not found"（#I7MCA5） |
| Upgrade 进程不需要网络 | □ | 所有数据由 Client 预下载 |
| IPC 文件路径可写入 | □ | `%TEMP%/GeneralUpdate/ipc/` 需要写权限 |

### ✅ manifest.json 检查

| 检查项 | 是否通过 |
|--------|---------|
| 所有版本号是 4 段式（如 "1.0.0.0"） | □ |
| ClientVersion 不为空 | □ |
| MainAppName 包含 .exe 扩展名 | □ |
| 文件放在应用程序根目录 | □ |

### ✅ CI/CD 集成

```
CI/CD Pipeline:
  build → 生成全量包 (.zip) → 上传到服务器 / OSS
     ↘ 更新 versions.json / 服务端数据库
     
  关键：
  - 每次构建需要打全量包（服务端会基于全量包生成差分包）
  - 包名/版本号必须与服务端预期一致
  - 发布 UpgradeApp.exe 时注意平台（Windows/Linux 使用不同二进制）
```

---

## ⚠️ 已知问题与规避（实时 Issue 驱动）

| # | 问题 | 症状 | 规避方案 |
|---|------|------|---------|
| 1 | **manifest 阻塞版本发现**（#484） | 静默模式不生效 | 确保 manifest 中版本号已填写（非空） |
| 2 | **场景判断与服务端错位**（#475） | 说"有更新"但无包可下载 | 更新到 v5.0+ |
| 3 | **IPC 加密文件路径固定**（代码审计） | 防病毒软件隔离 | 使用 NamedPipe IPC（见 advanced） |
| 4 | **NuGet 版本不匹配**（#I7MCA5） | "Method not found" | Client 和 Upgrade 使用完全相同版本 |
| 5 | **备份目录递归嵌套**（#501） | PathTooLongException | 显式指定跳过目录 |
| 6 | **无限升级循环**（#475） | 每次启动都更新 | 检查 manifest 版本号是否 WriteBack |
| 7 | **静默模式 ProcessExit 不触发**（#484） | 更新永远不执行 | 显式调用 `TryLaunchUpgrade()` |
| 8 | **Assembly.GetExecutingAssembly**（#I5O4KV） | 版本号获取错误 | 在 manifest 中手动写版本号 |

---

## 相关技能

- `/generalupdate-ui` — UI 框架自动检测 + 更新窗口代码生成
- `/generalupdate-strategy` — 6 种更新策略选择与配置
- `/generalupdate-advanced` — Hooks/Bowl/IPC/自定义策略
- `/generalupdate-troubleshoot` — 20+ 已知问题诊断
