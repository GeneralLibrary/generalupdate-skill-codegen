---
name: generalupdate-init
description: |
  Integrate GeneralUpdate auto-update into a .NET application in minutes.
  Triggers on: "add auto update", "integrate GeneralUpdate", "configure bootstrap",
  "add update capability", "set up auto-updater", "我需要自动更新", "配置更新",
  "初始化GeneralUpdate", "添加更新功能", "升级框架接入".
  Also triggers when user mentions their project is WPF/WinForms/Avalonia/MAUI/console
  AND wants auto-update — always pair with generalupdate-ui if UI framework detected.
when_to_use: |
  - User wants to add auto-update to their .NET application for the first time
  - User wants to configure GeneralUpdateBootstrap with minimal code
  - User needs both Client and Upgrade projects set up
  - User mentions their project type (WPF/WinForms/Avalonia/MAUI/console) + update
allowed-tools: "Bash, Read, Write, Edit, Glob, Grep, WebSearch"
---

# 🚀 GeneralUpdate 快速集成

帮助开发者将 GeneralUpdate 自动更新功能集成到 .NET 应用中。生成双项目结构（Client + Upgrade）的完整配置代码。

## 工作流程

1. **探测项目状态** — 检查当前目录是否存在 .csproj 文件、目标框架、UI 类型
2. **选择集成模式** — 按用户需求选择 Minimal（3行代码）或 Full（完整配置）
3. **生成配置代码** — 输出 NuGet 安装命令 + Bootstrap 配置代码
4. **可选**：如果用户需要 UI，引导到 `generalupdate-ui`；如果问策略，引导到 `generalupdate-strategy`

## 核心 API 速查

### NuGet 包

```xml
<!-- 必需 -->
<PackageReference Include="GeneralUpdate.Core" Version="x.x.x" />

<!-- 可选（按需添加） -->
<PackageReference Include="GeneralUpdate.Differential" Version="x.x.x" />  <!-- 差分更新 -->
<PackageReference Include="GeneralUpdate.Bowl" Version="x.x.x" />           <!-- 崩溃守护 -->
<PackageReference Include="GeneralUpdate.Extension" Version="x.x.x" />      <!-- 扩展管理 -->
```

### 零配置集成（推荐 — 3 行代码）

从 v5.0 开始，`SetSource()` 自动从 `generalupdate.manifest.json` 发现应用元数据：

```csharp:Program.cs
using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;

var result = await new GeneralUpdateBootstrap()
    .SetSource("https://your-server.com/api", "your-secret-key")
    .SetOption(Option.AppType, AppType.Client)
    .LaunchAsync();

// result: true = 更新完成并重启; false = 无需更新
```

**前提**：需要在发布目录放置 `generalupdate.manifest.json`：
```json
{
  "MainAppName": "MyApp.exe",
  "UpdateAppName": "UpgradeApp.exe",
  "ProductId": "your-product-id",
  "InstallPath": ".",
  "UpdatePath": "update",
  "ClientVersion": "1.0.0.0",
  "UpgradeClientVersion": "1.0.0.0"
}
```

### 完整配置集成

```csharp:Program.cs
using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;
using GeneralUpdate.Core.Enum;

var request = new UpdateRequest
{
    // === 必需 ===
    UpdateUrl = "https://your-server.com/Upgrade/Verification",
    AppSecretKey = "your-hmac-secret-key",

    // === 自动发现（可从 manifest 读取，也可显式指定）===
    InstallPath = AppDomain.CurrentDomain.BaseDirectory,     // 应用安装目录
    UpdatePath = Path.Combine("update"),                     // Upgrade.exe 路径
    ClientVersion = "1.0.0.0",                              // 当前版本
    MainAppName = "MyApp.exe",                              // 主程序名
    UpdateAppName = "UpgradeApp.exe",                       // 升级程序名
    ProductId = "my-product-001",                           // 产品标识

    // === 可选 ===
    ReportUrl = "https://your-server.com/Upgrade/Report",   // 状态上报
    BasicUsername = "", BasicPassword = "",                  // Basic Auth
};

var bootstrap = new GeneralUpdateBootstrap()
    .SetConfig(request)
    .SetOption(Option.AppType, AppType.Client)
    .SetOption(Option.MaxConcurrency, 3)                     // 并行下载数
    .SetOption(Option.PatchEnabled, false)                   // 是否启用差分
    .SetOption(Option.BackupEnabled, false)                  // 是否启用备份
    .SetOption(Option.Silent, false)                         // 是否静默更新

    // 事件监听
    .AddListenerUpdateInfo((_, e) => {
        Console.WriteLine($"发现新版本: {e.Version}");
    })
    .AddListenerMultiDownloadStatistics((_, e) => {
        Console.WriteLine($"下载进度: {e.ProgressValue}% | {e.Speed}/s");
    })
    .AddListenerMultiDownloadCompleted((_, e) => {
        Console.WriteLine($"下载完成: {e.Versions?.LastOrDefault()?.Version}");
    })
    .AddListenerException((_, e) => {
        Console.WriteLine($"更新异常: {e.Message}");
    });

var result = await bootstrap.LaunchAsync();
```

### ⚠️ 关键注意事项（来自真实用户 Issue）

1. **升级进程必须随主程序一起发布** — GeneralUpdate 不能自己下载自己，`UpgradeApp.exe` 必须从第一个版本就存在
2. **双进程必须使用相同 AppSecretKey** — 因为 IPC 加密基于该 Key
3. **版本号必须是 4 段** — `"1.0.0.0"` 有效，`"1.0"` 无效
4. **manifest 文件必须放在安装目录根目录** — 文件名固定为 `generalupdate.manifest.json`
5. **默认备份已禁用** (v5.0+) — 如需回滚能力，显式设置 `BackupEnabled = true`

## 输出

根据用户的项目类型和需求，输出以下内容组合：
- ✅ NuGet 包安装命令
- ✅ `GeneralUpdateBootstrap` 配置代码（可粘贴到 `Program.cs`）
- ✅ `generalupdate.manifest.json` 模板
- ✅ （可选）完整 Client + Upgrade 双项目结构

## 相关技能

- `/generalupdate-ui` — 生成带进度显示的更新界面（如果用户需要 UI）
- `/generalupdate-strategy` — 配置特定的更新策略（如果问策略相关）
- `/generalupdate-troubleshoot` — 如果集成后遇到问题
