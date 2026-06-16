---
description: "GeneralUpdate auto-update integration for .NET — Bootstrap, manifest, dual-project scaffold"
globs: ["**/*.cs", "**/*.csproj", "**/*.json"]
tags: ["dotnet", "update", "generalupdate"]
---

# GeneralUpdate 快速集成 (v10.4.6 稳定版 API)

## NuGet: GeneralUpdate.Core(必需), Bowl(可选, 与Core互斥), Differential(嵌入Core无需额外引用)

## 最小集成
```csharp
new Configinfo { ... }.SetConfig().LaunchAsync()
```

## 核心 API（非 dev 分支的 SetSource/SetOption）
- Configinfo 属性: UpdateUrl, AppSecretKey, InstallPath, ClientVersion, MainAppName, UpdateAppName, ProductId
- AppType 是 class（非 enum），`AppType.ClientApp = 1`, `AppType.UpgradeApp = 2`
- `LaunchAsync()` 返回 `Task<GeneralUpdateBootstrap>`（非 `Task<bool>`）
- ❌ 无 `SetSource()`, `SetOption()`, `Hooks<T>()`, `Strategy<T>()`, `SilentOrchestrator`

## 4 大场景: None/UpgradeOnly/MainOnly/Both

## generalupdate.manifest.json 字段
MainAppName, UpdateAppName, ProductId, InstallPath, UpdatePath, ClientVersion, UpgradeClientVersion

## 关键: UpgradeApp须随首个版本发布, 双进程AppSecretKey一致, 版本4段式

## Bowl 引用规则: 只用 `GeneralUpdate.Bowl`（传递依赖 Core，两者不能同时引用）
