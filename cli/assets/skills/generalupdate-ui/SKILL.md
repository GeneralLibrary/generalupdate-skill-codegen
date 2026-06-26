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

# GeneralUpdate Update UI Generation — Full State Coverage / GeneralUpdate 更新界面生成 — 全状态覆盖

Automatically detects the developer's UI framework type and generates complete update window code with real GeneralUpdate.Core event bindings.

自动检测开发者的 UI 框架类型，生成带真实 GeneralUpdate.Core 事件绑定的完整更新窗口代码。

> **Targeting NuGet v10.5.0-rc.1**. `RealDownloadService.cs` already uses `UpdateRequest` and correct namespaces.
> ⚠️ 针对 NuGet v10.5.0-rc.1。`RealDownloadService.cs` 已使用 `UpdateRequest` 和正确的命名空间。

---

## Requirements Extraction (Must Confirm Before Generating UI) / 用户需求提取（生成 UI 前必须确认）

```
### UI Framework (Required) / UI 框架（必需）
- Target framework / 目标框架: ______ (WPF / WinForms / Avalonia / MAUI / Console / 
  Unsure / 不确定)
- Preferred UI library / 偏好 UI 库: ______ (Default recommendation / LayUI.Wpf / 
  WPFDevelopers / AntdUI / SemiUrsa / Native / 默认推荐 / LayUI.Wpf / WPFDevelopers / 
  AntdUI / SemiUrsa / 原生)
- Existing project template / 是否已有项目模板: ______ (Yes/No / 是/否, if no, start 
  from generalupdate-init / 如果否，从 generalupdate-init 开始)

### Update Scenario (Required) / 更新场景（必需）
- Update window role / 更新窗口角色: ______ (Client-side / Upgrade-side / Both / 
  Client 端/ Upgrade 端/ 两端都需要)
- Manual update trigger / 是否需要手动触发更新: ______ (Yes/No, auto-check on 
  startup / 是/否，自动启动时检查)
- Dark mode support / 是否支持暗黑模式: ______ (Yes/No / 是/否)

### Advanced UI Requirements (Optional) / 高级 UI 需求（可选）
- Custom brand color/logo / 需要自定义品牌色/Logo: ______ (Yes/No / 是/否)
- Multi-language support / 需要多语言支持: ______ (Yes/No / 是/否)
- Accessibility support / 需要无障碍支持: ______ (Yes/No / 是/否)
```

---

## Workflow / 工作流程

```
1. Framework Detection / 框架探测
   ├── Scan .csproj → PackageReference to identify UI library
   │   扫描 .csproj → PackageReference 识别 UI 库
   ├── If unrecognized → ask the user / 如果无法识别 → 询问用户
   └── If no UI framework → console progress bar / 如果无 UI 框架 → 控制台进度条

2. State Code Generation / 状态代码生成
   ├── IDownloadService bridge interface / IDownloadService 桥接接口
   ├── RealDownloadService bridge code (manually adapts GeneralUpdate.Core events)
   │   RealDownloadService 桥接代码（手动适配 GeneralUpdate.Core 事件）
   ├── ViewModel (MVVM) or Code-Behind / ViewModel（MVVM）或 Code-Behind
   └── Window/Page XAML / 窗口/页面 XAML

3. Integration Guidance / 集成指导
   ├── How to wire up GeneralUpdateBootstrap / 如何引入 GeneralUpdateBootstrap
   └── Bootstrap configuration (paired with generalupdate-init)
       Bootstrap 配置（与 generalupdate-init 配合）
```

---

## UI State Machine (All Templates Cover These States) / UI 状态机（所有模板覆盖以下状态）

```
                   ┌─────────────┐
                   │    Idle     │ ← Initial state / 初始状态
                   └──────┬──────┘
                          │ Auto/manual trigger
                          │ 自动/手动触发
                          ▼
                   ┌─────────────┐
            ┌─────│  Checking    │ ← "Checking for updates..." / "正在检查更新..."
            │     └──────┬──────┘
            │            │
            │     ┌──────┴──────┐
            │     ▼             ▼
            │  ┌────────┐  ┌──────────┐
            │  │ Latest │  │  Found!  │ ← Shows version/size / 显示版本号/大小
            │  └────────┘  └────┬─────┘
            │                   │ User clicks "Start Update" / 用户点击"开始更新"
            │                   ▼
            │            ┌──────────────┐
            │      ┌─────│ Downloading  │ ← Progress bar/speed/remaining
            │      │     │               │   进度条/速度/剩余时间
            │      │     └──────┬───────┘
            │      │            │
            │      │     ┌──────┴──────┐
            │      │     ▼             ▼
            │      │  ┌────────┐  ┌──────────┐
            │      │  │ Paused │  │  Error   │ ← Shows error + "Retry"
            │      │  └───┬────┘  │           │   显示错误 + "重试"
            │      │      │ Resume│           │
            │      │      │ 继续   └────┬─────┘
            │      │      ▼             │ Retry / 重试
            │      │  ┌──────────────┐  │
            │      │  │ Downloading  │ ◄┘
            │      │  └──────────────┘
            │      │
            │      │     ┌──────────────┐
            │      └────→│  Applying    │ ← "Installing update..." / "正在安装更新..."
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
            │       │ Restart  │ ← Restart app / 重启应用
            │       └──────────┘
            │
            └── Back to Idle / 回到 Idle
```

---

## Workflow: RealDownloadService / 工作流程：RealDownloadService

All UI templates share this bridge class that maps GeneralUpdate.Core events to the `IDownloadService` interface.

所有 UI 模板共享这个桥接类，将 GeneralUpdate.Core 的事件映射到 `IDownloadService` 接口。

### Bridge Logic (v10.5.0-rc.1) / 桥接逻辑（v10.5.0-rc.1）

```csharp
// GeneralUpdate.Core events → DownloadStatus state machine mapping:
// GeneralUpdate.Core 事件 → DownloadStatus 状态机映射：

GeneralUpdateBootstrap.AddListenerMultiDownloadStatistics
    → Downloading (updates ProgressPercentage/Speed/Remaining)
      Downloading（更新 ProgressPercentage/Speed/Remaining）

GeneralUpdateBootstrap.AddListenerMultiDownloadCompleted
    → File processing (extract/verify) / 文件处理中（解压/校验）

GeneralUpdateBootstrap.AddListenerMultiAllDownloadCompleted
    → Applying → Success

GeneralUpdateBootstrap.AddListenerMultiDownloadError
    → DownloadError (after N automatic retries) / DownloadError（自动重试 N 次后）

GeneralUpdateBootstrap.AddListenerException
    → Failed (non-fatal exceptions do not change state)
      Failed（非致命异常不改变状态）
```

---

## UI Framework Template Inventory / UI 框架模板清单

| Template File | Applicable Framework | Included Features |
|--------------|---------------------|-------------------|
| `SemiUrsaClientView.axaml` + `.cs` | Avalonia + SemiUrsa | Full state machine, dark mode toggle, animations / 全状态机、暗黑切换、动画 |
| `SemiUrsaUpgradeView.axaml` + `.cs` | Avalonia + SemiUrsa (Upgrade) | Waiting UI / 等待中 UI |
| `LayUIStyle.xaml` + `.cs` | WPF + LayUI.Wpf | Glass effect, progress bar / 玻璃效果、进度条 |
| `WPFDevelopersStyle.xaml` + `.cs` | WPF + WPFDevelopers | Circular progress, breathing light animation / 圆形进度、呼吸灯动画 |
| `AntdUIStyle.cs` | WinForms + AntdUI | Dark theme, wave progress button / 暗黑主题、波浪进度按钮 |
| `MauiUpdatePage.xaml` + `.cs` | MAUI | Dark mode, AppThemeBinding / 深色模式、AppThemeBinding |
| `DownloadViewModels.cs` | Shared across all frameworks / 所有框架共用 | MVVM ViewModel |
| `RealDownloadService.cs` | Shared across all frameworks / 所有框架共用 | **Core bridge / 核心桥接** |

---

## Integration Verification Checklist (Check Each Item Before Delivery) / 集成验证清单（交付前逐项检查）

### Event Bridging / 事件桥接
- [ ] All 6 events are bound (UpdateInfo, MultiDownloadStatistics, MultiDownloadCompleted, MultiDownloadError, MultiAllDownloadCompleted, Exception) / 所有 6 个事件都已绑定（UpdateInfo, MultiDownloadStatistics, MultiDownloadCompleted, MultiDownloadError, MultiAllDownloadCompleted, Exception）
- [ ] Bridge code uses correct EventArgs types (check namespace `GeneralUpdate.Core.Download` / `GeneralUpdate.Core.Event`) / 桥接代码使用正确的 EventArgs 类型（检查命名空间 `GeneralUpdate.Core.Download` / `GeneralUpdate.Core.Event`）
- [ ] `IsCompleted` property name is correct (v10.5.0-rc.1 uses `IsCompleted`) / `IsCompleted` 属性名正确（v10.5.0-rc.1 使用 `IsCompleted`）

### Thread Safety / 线程安全
- [ ] UI update operations execute on the correct thread (WPF/Avalonia uses `Dispatcher`, WinForms uses `Invoke`, MAUI uses `MainThread`) / UI 更新操作在正确的线程上执行（WPF/Avalonia 用 `Dispatcher`，WinForms 用 `Invoke`，MAUI 用 `MainThread`）
- [ ] No blocking operations inside `MultiDownloadStatistics` event (UI updates only) / `MultiDownloadStatistics` 事件中不执行耗时操作（仅更新 UI）
- [ ] "Applying" state after download has timeout protection (recommend > 30 sec with progress tip) / 下载完成后的"正在应用"状态有超时保护（建议 > 30 秒显示进度提示）

### State Machine Coverage / 状态机覆盖
- [ ] All 11 states are implemented (Idle → Checking → Latest/Found → Downloading → Paused → Error → Retrying → Applying → Success/Failed → Restart) / 所有 11 个状态都已实现（Idle → Checking → Latest/Found → Downloading → Paused → Error → Retrying → Applying → Success/Failed → Restart）
- [ ] Auto-retry count for download errors is limited (no more than 3 times) / 下载错误的自动重试次数有限制（不超过 3 次）
- [ ] User can cancel the update operation / 用户可取消更新操作

### Framework-Specific Checks / 框架特定检查
- [ ] **Avalonia**: ViewModel implements `INotifyPropertyChanged`, bindings use `{Binding}` / **Avalonia**: ViewModel 实现 `INotifyPropertyChanged`，绑定使用 `{Binding}`
- [ ] **WPF**: Uses `Dispatcher.Invoke` to update bound properties / **WPF**: 使用 `Dispatcher.Invoke` 更新绑定的属性
- [ ] **WinForms AntdUI**: Uses `Control.Invoke` for cross-thread updates / **WinForms AntdUI**: 使用 `Control.Invoke` 进行跨线程更新
- [ ] **MAUI**: Check `Platform.CurrentActivity` lifecycle on Android / **MAUI**: 检查 `Platform.CurrentActivity` 在 Android 上的生命周期

---

## Anti-Pattern Checklist / 反模式清单

| # | Anti-Pattern | Consequence | Correct Approach |
|---|-------------|-------------|-----------------|
| 1 | **Using a generic ViewModel directly across different frameworks / 通用 ViewModel 直接用在不同框架** | Thread model incompatibility causes cross-thread exceptions / 线程模型不兼容导致跨线程异常 | Adapt Dispatcher/Invoke/MainThread per framework / 按框架分别适配 Dispatcher/Invoke/MainThread |
| 2 | **Performing file IO or network requests inside download statistics event / 在下载统计事件中做文件 IO 或网络请求** | Blocks update flow, UI freezes / 阻塞更新流程，UI 卡顿 | Only update UI-bound properties / 仅更新 UI 绑定的属性 |
| 3 | **Progress bar binding jumps to 100% in one shot / 进度条绑定一次性更新到 100%** | User cannot see intermediate progress, poor UX / 用户看不到中间过程，体验差 | Use `e.ProgressPercentage` for gradual updates / 使用 `e.ProgressPercentage` 逐步更新 |
| 4 | **MultiDownloadError event not handled / 未处理 MultiDownloadError 事件** | User gets no feedback on download failure, stuck waiting / 下载失败时用户无反馈，卡在等待状态 | At minimum show error message + retry button / 至少显示错误信息 + 重试按钮 |
| 5 | **Not distinguishing Client vs Upgrade UI / 未区分 Client 和 Upgrade 的 UI** | Upgrade side shows unnecessary "Download Progress" / Upgrade 端显示不必要的"下载进度" | Upgrade side only shows "Installing, please wait..." / Upgrade 端只显示"正在安装，请稍候" |
| 6 | **Using RealDownloadService.cs directly without adaptation / 直接使用 RealDownloadService.cs 不做适配** | Event bindings do not take effect / 事件绑定不生效 | Must adjust `IDownloadService` implementation per project structure / 必须根据项目结构调整 `IDownloadService` 实现 |
| 7 | **Starting update in ViewModel constructor (Avalonia/WPF) / Avalonia/WPF 在 ViewModel 构造函数中启动更新** | UI not yet initialized, bindings don't work / UI 还未初始化完成，绑定不生效 | Trigger update check in Loaded event or View layer / 在 Loaded 事件或 View 层触发检查更新 |

---

## Related Skills / 相关技能

- `/generalupdate-init` — If Bootstrap is not yet configured / 如果还未配置 Bootstrap
- `/generalupdate-strategy` — If you want Silent mode without UI / 如果想要 Silent 模式不需要 UI
- `/generalupdate-troubleshoot` — If UI displays abnormally / 如果 UI 显示异常
