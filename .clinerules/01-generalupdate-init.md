---
description: "GeneralUpdate auto-update integration for .NET — Bootstrap, manifest, dual-project scaffold"
globs: ["**/*.cs", "**/*.csproj", "**/*.json"]
tags: ["dotnet", "update", "generalupdate"]
---

# GeneralUpdate 快速集成

## NuGet: GeneralUpdate.Core(必需), Differential(可选), Bowl(可选)

## 最小集成
SetSource(url, key) -> SetOption(AppType.Client) -> LaunchAsync()

## 4 大场景: None/UpgradeOnly/MainOnly/Both

## manifest.json 字段
MainAppName, UpdateAppName, ProductId, InstallPath, UpdatePath, ClientVersion, UpgradeClientVersion

## 关键: UpgradeApp须随首个版本发布, 双进程AppSecretKey一致, 版本4段式

## AppType枚举: Client=1, Upgrade=2, OssClient=3, OssUpgrade=4
