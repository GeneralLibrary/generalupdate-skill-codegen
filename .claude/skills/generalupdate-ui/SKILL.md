---
name: generalupdate-ui
description: |
  Generate a complete update UI window for ANY .NET UI framework — no UI coding required.
  Automatically detects WPF (LayUI.Wpf, WPFDevelopers, native), WinForms (AntdUI, native),
  Avalonia (SemiUrsa), MAUI, or console apps. Generates fully wired update windows with
  REAL GeneralUpdate.Core event bindings that cover ALL states: checking, downloading,
  error/retry, paused, completed, upgrade-in-progress, already-latest, forced-update,
  rollback. Generates IDownloadService bridge to replace MockDownloadService.
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
  - User currently uses MockDownloadService and wants real update binding
  - User wants pre-built ViewModels, Windows, Styles for update flows
  - User already has basic update integration working and wants a UI for it
allowed-tools: "Read, Write, Edit, Glob, Grep"
---

# 🎨 GeneralUpdate 更新界面生成 — 全状态覆盖

自动检测开发者的 UI 框架类型，生成带真实 GeneralUpdate.Core 事件绑定的完整更新窗口代码。
覆盖所有 UI 状态、错误处理、动画和 MVVM 绑定。

---

## UI 状态机（所有模板覆盖以下状态）

```
                   ┌─────────────┐
                   │    Idle     │ ← 初始状态
                   └──────┬──────┘
                          │ 自动/手动触发
                          ▼
                   ┌─────────────┐
            ┌─────│  Checking    │ ← "正在检查更新..."  indeterminate 动画
            │     └──────┬──────┘
            │            │
            │     ┌──────┴──────┐
            │     ▼             ▼
            │  ┌────────┐  ┌──────────┐
            │  │ Latest │  │  Found!  │ ← 显示版本号/大小/更新说明
            │  │(已最新)│  └────┬─────┘
            │  └────────┘       │ 用户点击"开始更新"
            │                   ▼
            │            ┌──────────────┐
            │      ┌─────│ Downloading  │ ← 进度条/速度/剩余时间/动画
            │      │     └──────┬───────┘
            │      │            │
            │      │     ┌──────┴──────┐
            │      │     ▼             ▼
            │      │  ┌────────┐  ┌──────────┐
            │      │  │ Paused │  │  Error   │ ← 显示错误信息 + "重试"按钮
            │      │  └───┬────┘  └────┬─────┘
            │      │      │ 继续        │ 重试
            │      │      ▼             ▼
            │      │  ┌──────────────┐
            │      │  │ Downloading  │ ← 回到下载状态
            │      │  └──────────────┘
            │      │
            │      │     ┌──────────────┐
            │      └────→│  Applying    │ ← "正在安装更新..."  (Upgrade 进程)
            │             └──────┬───────┘
            │                    │
            │             ┌──────┴──────┐
            │             ▼             ▼
            │       ┌─────────┐  ┌──────────┐
            │       │ Success │  │  Failed  │ ← 显示失败原因 + 回滚提示
            │       └────┬────┘  └────┬─────┘
            │            │            │
            │            ▼            ▼
            │       ┌──────────┐  ┌──────────┐
            │       │ Restart  │  │ Rollback │ ← "正在回滚到上一个版本"
            │       │(重启应用) │  └──────────┘
            │       └──────────┘
            │
            └── 回到 Idle（无需更新时）
```

---

## 工作流程

```
1. 框架探测
   ├── 扫描 .csproj → PackageReference 识别 UI 库
   │   ├── Semi.Avalonia / Ursa → Avalonia + SemiUrsa
   │   ├── LayUI.Wpf → WPF + LayUI
   │   ├── WPFDevelopers → WPF + WPFDevelopers
   │   ├── AntdUI → WinForms + AntdUI
   │   ├── Microsoft.Maui → MAUI
   │   └── 无 → 探测 .xaml / .Designer.cs → 原生 WPF/WinForms
   ├── 如果无法识别 → 询问用户使用的框架
   └── 如果无 UI 框架 → 控制台进度条

2. 状态代码生成
   ├── IDownloadService 增强版接口（覆盖所有状态）
   ├── RealDownloadService 桥接代码（GeneralUpdate.Core → IDownloadService）
   ├── ViewModel（MVVM）或 Code-Behind
   └── 窗口/页面 XAML（各框架特有）

3. 集成指导
   ├── 如何替换 MockDownloadService → RealDownloadService
   ├── DI 注册（或直接实例化）
   └── Bootstrap 配置（与 generalupdate-init 配合）
```

---

## 核心桥接：RealDownloadService

所有 UI 模板共享这个桥接类，将 GeneralUpdate.Core 的全部事件映射到 `IDownloadService` 接口。

### 增强版 IDownloadService 接口（覆盖所有状态）

```csharp
public enum DownloadStatus
{
    Idle,               // 初始状态，暂无操作
    Checking,           // 正在检查服务器版本
    FoundUpdate,        // 已发现新版本（等待用户确认）
    AlreadyLatest,      // 已是最新版本
    Downloading,        // 正在下载更新包
    Paused,             // 下载已暂停
    DownloadError,      // 下载出错，可重试
    Applying,           // 正在应用更新（解压/补丁）
    UpgradeProgress,    // Upgrade 进程正在执行
    Success,            // 更新成功，等待重启
    Failed,             // 更新失败，可能需要回滚
    RollingBack         // 正在回滚到上一个版本
}

public interface IDownloadService
{
    // === 事件 ===
    event Action<DownloadStatistics>? StatisticsChanged;  // 任何状态/统计变化
    event Action<DownloadStatus>? StatusChanged;          // 状态变更
    event Action<string>? ErrorOccurred;                  // 错误信息
    event Action? UpdateCompleted;                        // 更新完成

    // === 属性 ===
    DownloadStatistics CurrentStatistics { get; }
    DownloadStatus Status { get; }
    bool CanStart { get; }
    bool CanPause { get; }
    bool CanRetry { get; }

    // === 方法 ===
    void CheckForUpdates();            // 检查更新
    void StartDownload();              // 开始下载
    void Pause();                      // 暂停
    void Retry();                      // 重试（从当前状态恢复）
    void Cancel();                     // 取消
    void Restart();                    // 完全重新开始
}
```

### RealDownloadService 桥接逻辑

```csharp
// 映射 GeneralUpdate.Core 事件到 DownloadStatus 状态机：

Bootstrap 事件                  → 状态转换
──────────────────────────────────────────────────
LaunchAsync 开始                → Checking
UpdateInfo 收到                 → FoundUpdate / AlreadyLatest
MultiDownloadStatistics 收到    → Downloading
MultiDownloadError 收到         → DownloadError (自动重试N次后)
MultiDownloadCompleted 收到     → Applying
MultiAllDownloadCompleted 收到  → UpgradeProgress → Success
Exception 收到                  → Failed
```

---

## UI 框架模板清单

| 模板文件 | 适用框架 | 包含特性 |
|---------|---------|---------|
| `SemiUrsaClientView.axaml` + `.cs` | Avalonia + SemiUrsa | 全状态机、暗黑切换、通知、进度条动画 |
| `SemiUrsaUpgradeView.axaml` + `.cs` | Avalonia + SemiUrsa (Upgrade) | 等待中 UI、indeterminate 进度、过渡动画 |
| `LayUIStyle.xaml` + `.cs` | WPF + LayUI.Wpf | 玻璃效果、弹窗对话框、进度条 |
| `WPFDevelopersStyle.xaml` + `.cs` | WPF + WPFDevelopers | 圆形进度、呼吸灯动画、通知图标 |
| `AntdUIStyle.cs` | WinForms + AntdUI | 暗黑主题、本地化、波浪进度按钮、取消 |
| `NativeWpfWindow.xaml` + `.cs` | 原生 WPF（无皮肤） | 简洁窗口、进度条、状态文本 |
| `NativeWinForms.cs` | 原生 WinForms | 简单表单、进度条、取消 |
| `MauiUpdatePage.xaml` + `.cs` | MAUI | 跨平台、深色模式、AppThemeBinding |
| `ConsoleProgress.cs` | 控制台应用 | ANSI 进度条、状态文本 |
| `DownloadViewModels.cs` | 所有框架共用 | 完整 ViewModel + DownloadStatistics |
| `RealDownloadService.cs` | 所有框架共用 | **核心桥接**：GeneralUpdate → IDownloadService |

---

## 输出

根据用户框架和需求，输出以下内容（按优先级排列）：
- ✅ `RealDownloadService.cs` — 核心桥接代码（替换 MockDownloadService）
- ✅ `DownloadViewModels.cs` — 完整 MVVM ViewModel（全状态覆盖）
- ✅ 目标框架的窗口/页面 XAML + Code-Behind
- ✅ 集成步骤说明（DI 注册 / 文件替换 / Navigation）

## 相关技能

- `/generalupdate-init` — 如果还未配置 Bootstrap
- `/generalupdate-troubleshoot` — 如果 UI 显示异常
