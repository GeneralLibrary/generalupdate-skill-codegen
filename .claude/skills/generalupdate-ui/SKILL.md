---
name: generalupdate-ui
description: |
  Generate a complete update UI window for the developer's UI framework — no UI coding required.
  Automatically detects WPF (LayUI/WPFDevelopers/native), WinForms (AntdUI/native),
  Avalonia (SemiUrsa), MAUI, or console apps and generates fully wired update UI code
  with real GeneralUpdate.Core event bindings (progress bars, status text, animations).
  Triggers on: "update UI", "progress bar", "update window", "show progress",
  "update界面", "进度显示", "更新窗口", "好看点", "UI样式",
  "how to show update progress", "need a progress UI", "update form".
  ALWAYS load this skill when the user asks for an auto-update UI or integration.
when_to_use: |
  - User wants a visual update progress interface
  - User asks about showing download progress or status
  - User mentions their UI framework (WPF/WinForms/Avalonia/MAUI) in context of updates
  - Used together with generalupdate-init for a complete integration
  - User wants a "beautiful" or "professional looking" update experience
allowed-tools: "Read, Write, Edit, Glob, Grep"
---

# 🎨 GeneralUpdate 更新界面生成

自动检测开发者的 UI 框架类型，生成带真实 GeneralUpdate.Core 事件绑定的完整更新窗口代码。

## 工作流程

1. **框架探测** — 检测用户项目的 UI 框架类型
   - 扫描 `.csproj` 中的 PackageReference（LayUI.Wpf、AntdUI、Semi.Avalonia 等）
   - 如果无法自动识别，询问用户使用的框架
2. **UI 生成** — 按框架类型输出对应的更新窗口代码
3. **桥接代码** — 生成 `RealDownloadService` 将 GeneralUpdate.Core 事件映射到 UI 绑定
4. **集成指导** — 告诉用户如何将生成的代码替换入项目

## 框架检测逻辑

```
扫描项目 .csproj 或 .axaml / .xaml 文件 →
  ├─ 发现 Semi.Avalonia / Ursa → Avalonia + SemiUrsa 风格
  ├─ 发现 LayUI.Wpf → WPF + LayUI 风格
  ├─ 发现 WPFDevelopers → WPF + WPFDevelopers 风格
  ├─ 发现 AntdUI → WinForms + AntdUI 风格
  ├─ 发现 Microsoft.Maui → MAUI 风格
  └─ 未识别 →
      ├─ 项目含 .xaml 但无皮肤库 → 原生 WPF 通用风格
      ├─ 项目含 .Designer.cs → 原生 WinForms 通用风格
      └─ 其他 → 控制台进度条风格
```

## 核心桥接组件：RealDownloadService

所有 UI 模板共享这个桥接类，将 GeneralUpdate.Core 的事件映射到 `IDownloadService` 接口。
UI 的 ViewModel 只需要依赖 `IDownloadService`，不需要直接引用 GeneralUpdate.Core。

**无需开发者自己写** — 以下为 Skill 内部生成逻辑：

```csharp
// Skill 自动生成：将 GeneralUpdate 事件 → IDownloadService 接口
public class RealDownloadService : IDownloadService
{
    private GeneralUpdateBootstrap _bootstrap;
    private DownloadStatistics _stats;

    public event EventHandler<DownloadStatistics> ProgressChanged;
    public event EventHandler<DownloadStatus> StatusChanged;
    public event EventHandler<string> ErrorOccurred;

    public async Task StartAsync()
    {
        _bootstrap = new GeneralUpdateBootstrap()
            .SetConfig(_request)
            .SetOption(Option.AppType, AppType.Client)
            .AddListenerMultiDownloadStatistics((_, e) =>
            {
                _stats = new DownloadStatistics
                {
                    ProgressPercentage = e.ProgressValue,
                    Speed = e.Speed,
                    Remaining = e.Remaining,
                    Version = e.Version?.Version
                };
                ProgressChanged?.Invoke(this, _stats);
            })
            .AddListenerMultiDownloadCompleted((_, e) =>
            {
                StatusChanged?.Invoke(this, DownloadStatus.Completed);
            })
            .AddListenerException((_, e) =>
            {
                ErrorOccurred?.Invoke(this, e.Message);
            });

        StatusChanged?.Invoke(this, DownloadStatus.Downloading);
        var result = await _bootstrap.LaunchAsync();
    }
}
```

## UI 框架模板清单

| 模板文件 | 框架 | 包含 |
|---------|------|------|
| `SemiUrsaClientView.axaml` + `.cs` | Avalonia + SemiUrsa | 主窗口/进度条/状态/版本号/暗黑切换 |
| `SemiUrsaUpgradeView.axaml` + `.cs` | Avalonia + SemiUrsa | 升级中窗口/文件处理进度 |
| `LayUIStyle.cs` + `.xaml` | WPF + LayUI.Wpf | 玻璃效果窗口/进度条/状态标签 |
| `WPFDevelopersStyle.cs` + `.xaml` | WPF + WPFDevelopers | 圆形进度/呼吸灯/通知图标 |
| `AntdUIStyle.cs` | WinForms + AntdUI | 暗黑主题/进度条/状态图标/本地化 |
| `MauiUpdatePage.xaml` + `.cs` | MAUI | 跨平台进度/深色模式/状态 |
| `DownloadViewModels.cs` | 所有框架共用 | MVVM ViewModel / 状态管理 / 命令 |
| `NativeWpfWindow.xaml` + `.cs` | 原生WPF（无皮肤） | 简洁更新窗口 |
| `NativeWinForms.cs` | 原生WinForms | 简单更新表单 |

## 输出

根据用户框架和需求，输出以下组合：
- ✅ 适配目标框架的完整 UI 窗口代码（可直接添加到项目）
- ✅ `RealDownloadService.cs` 桥接代码（替换 Mock）
- ✅ ViewModel / Code-behind（MVVM 或传统模式）
- ✅ 集成步骤说明

## 相关技能

- `/generalupdate-init` — 如果还需要配置 Bootstrap（UI 技能生成后可以接 init 完成集成）
- `/generalupdate-troubleshoot` — 如果 UI 集成后遇到问题
