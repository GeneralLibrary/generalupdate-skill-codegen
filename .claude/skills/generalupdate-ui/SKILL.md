---
name: generalupdate-ui
description: |
  Generate a complete update UI window for ANY .NET UI framework — no UI coding required.
  Automatically detects WPF (LayUI.Wpf, WPFDevelopers, native), WinForms (AntdUI, native),
  Avalonia (SemiUrsa), MAUI, or console apps. Generates fully wired update windows with
  REAL GeneralUpdate.Core event bindings.
  Triggers on: "update UI", "progress bar", "update window", "show progress",
  "update界面", "进度显示", "更新窗口", "好看点", "UI样式",
  "how to show update progress", "need a progress UI", "update form",
  "beautiful update UI", "professional update appearance".
  ALWAYS load this skill when the user asks for auto-update + UI together.
  Pairs with generalupdate-init for complete integration.
  Pairs with generalupdate-troubleshoot if UI states show wrong values.
when_to_use: |
  - User wants a visual update progress interface (any framework)
  - User asks about showing download progress, speed, remaining time
  - User mentions their UI framework (WPF/WinForms/Avalonia/MAUI) in context of updates
  - User wants a "beautiful" or "professional looking" update experience
  - User already has basic update integration working and wants a UI for it
allowed-tools: "Read, Write, Edit, Glob, Grep"
---

# 🎨 GeneralUpdate 更新界面生成 — 全状态覆盖

自动检测开发者的 UI 框架类型，生成带真实 GeneralUpdate.Core 事件绑定的完整更新窗口代码。

> ⚠️ 针对 NuGet v10.5.0-beta.4。`RealDownloadService.cs` 已使用 `UpdateRequest` 和正确的命名空间。

---

## UI 状态机（所有模板覆盖以下状态）

```
                   ┌─────────────┐
                   │    Idle     │ ← 初始状态
                   └──────┬──────┘
                          │ 自动/手动触发
                          ▼
                   ┌─────────────┐
            ┌─────│  Checking    │ ← "正在检查更新..."
            │     └──────┬──────┘
            │            │
            │     ┌──────┴──────┐
            │     ▼             ▼
            │  ┌────────┐  ┌──────────┐
            │  │ Latest │  │  Found!  │ ← 显示版本号/大小
            │  └────────┘  └────┬─────┘
            │                   │ 用户点击"开始更新"
            │                   ▼
            │            ┌──────────────┐
            │      ┌─────│ Downloading  │ ← 进度条/速度/剩余时间
            │      │     └──────┬───────┘
            │      │            │
            │      │     ┌──────┴──────┐
            │      │     ▼             ▼
            │      │  ┌────────┐  ┌──────────┐
            │      │  │ Paused │  │  Error   │ ← 显示错误 + "重试"
            │      │  └───┬────┘  └────┬─────┘
            │      │      │ 继续        │ 重试
            │      │      ▼             ▼
            │      │  ┌──────────────┐
            │      │  │ Downloading  │
            │      │  └──────────────┘
            │      │
            │      │     ┌──────────────┐
            │      └────→│  Applying    │ ← "正在安装更新..."
            │             └──────┬───────┘
            │                    │
            │             ┌──────┴──────┐
            │             ▼             ▼
            │       ┌─────────┐  ┌──────────┐
            │       │ Success │  │  Failed  │
            │       └────┬────┘  └──────────┘
            │            │
            │            ▼
            │       ┌──────────┐
            │       │ Restart  │ ← 重启应用
            │       └──────────┘
            │
            └── 回到 Idle
```

---

## 工作流程

```
1. 框架探测
   ├── 扫描 .csproj → PackageReference 识别 UI 库
   ├── 如果无法识别 → 询问用户
   └── 如果无 UI 框架 → 控制台进度条

2. 状态代码生成
   ├── IDownloadService 桥接接口
   ├── RealDownloadService 桥接代码（手动适配 GeneralUpdate.Core 事件）
   ├── ViewModel（MVVM）或 Code-Behind
   └── 窗口/页面 XAML

3. 集成指导
   ├── 如何引入 GeneralUpdateBootstrap
   └── Bootstrap 配置（与 generalupdate-init 配合）
```

---

## 核心桥接：RealDownloadService

所有 UI 模板共享这个桥接类，将 GeneralUpdate.Core 的事件映射到 `IDownloadService` 接口。

### 桥接逻辑（v10.5.0-beta.4）

```csharp
// GeneralUpdate.Core 事件 → DownloadStatus 状态机映射：

GeneralUpdateBootstrap.AddListenerMultiDownloadStatistics
    → Downloading（更新 ProgressPercentage/Speed/Remaining）

GeneralUpdateBootstrap.AddListenerMultiDownloadCompleted
    → 文件处理中（解压/校验）

GeneralUpdateBootstrap.AddListenerMultiAllDownloadCompleted
    → Applying → Success

GeneralUpdateBootstrap.AddListenerMultiDownloadError
    → DownloadError（自动重试 N 次后）

GeneralUpdateBootstrap.AddListenerException
    → Failed（非致命异常不改变状态）
```

---

## UI 框架模板清单

| 模板文件 | 适用框架 | 包含特性 |
|---------|---------|---------|
| `SemiUrsaClientView.axaml` + `.cs` | Avalonia + SemiUrsa | 全状态机、暗黑切换、动画 |
| `SemiUrsaUpgradeView.axaml` + `.cs` | Avalonia + SemiUrsa (Upgrade) | 等待中 UI |
| `LayUIStyle.xaml` + `.cs` | WPF + LayUI.Wpf | 玻璃效果、进度条 |
| `WPFDevelopersStyle.xaml` + `.cs` | WPF + WPFDevelopers | 圆形进度、呼吸灯动画 |
| `AntdUIStyle.cs` | WinForms + AntdUI | 暗黑主题、波浪进度按钮 |
| `MauiUpdatePage.xaml` + `.cs` | MAUI | 深色模式、AppThemeBinding |
| `DownloadViewModels.cs` | 所有框架共用 | MVVM ViewModel |
| `RealDownloadService.cs` | 所有框架共用 | **核心桥接** |

---

## 相关技能

- `/generalupdate-init` — 如果还未配置 Bootstrap
- `/generalupdate-troubleshoot` — 如果 UI 显示异常
