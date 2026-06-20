---
description: "GeneralUpdate auto-update integration for .NET — Bootstrap, manifest, dual-project scaffold"
globs: ["**/*.cs", "**/*.csproj", "**/*.json"]
tags: ["dotnet", "update", "generalupdate"]
---

# GeneralUpdate 快速集成 (v10.5.0-beta.6 API)

## NuGet: GeneralUpdate.Core(必需), Bowl(可选, 与Core互斥), Differential(嵌入Core无需额外引用)

## 最小集成
```csharp
new UpdateRequest { ... }.SetConfig().LaunchAsync()
```

## 核心 API（v10.5.0-beta.6）
- UpdateRequest 属性: UpdateUrl, AppSecretKey, InstallPath, ClientVersion, MainAppName, ProductId
- 零配置入口: `SetSource(updateUrl, appSecretKey)`
- 可编程选项: `SetOption(Option.Silent, true)`, `SetOption(Option.MaxConcurrency, 4)`
- AppType 是 enum: `AppType.Client = 1`, `AppType.Upgrade = 2`, `AppType.OssClient = 3`, `AppType.OssUpgrade = 4`
- `LaunchAsync()` 返回 `Task<GeneralUpdateBootstrap>`
- ✅ `SetSource()`, `SetOption()`, `Hooks<T>()`, `Strategy<T>()`, `UseDiffPipeline()`

## 4 大场景: None/UpgradeOnly/MainOnly/Both

## 命名空间速查
- `GeneralUpdate.Core` — GeneralUpdateBootstrap
- `GeneralUpdate.Core.Configuration` — UpdateRequest, AppType, Option
- `GeneralUpdate.Core.Download` — 事件参数 (UpdateInfoEventArgs, MultiDownloadStatisticsEventArgs 等)
- `GeneralUpdate.Core.Event` — ExceptionEventArgs, ProgressEventArgs
- `GeneralUpdate.Core.Hooks` — IUpdateHooks, HookContext
- `GeneralUpdate.Core.Strategy` — IStrategy

## generalupdate.manifest.json 字段
MainAppName, UpdateAppName, ProductId, InstallPath, UpdatePath, ClientVersion, UpgradeClientVersion

## 关键: UpgradeApp须随首个版本发布, 双进程AppSecretKey一致, 版本4段式

## Bowl 引用规则: 只用 `GeneralUpdate.Bowl`（传递依赖 Core，两者不能同时引用）
