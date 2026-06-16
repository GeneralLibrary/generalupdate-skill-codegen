---
name: generalupdate-skill-codegen
description: |
  Complete Claude Code skill suite for integrating GeneralUpdate (.NET auto-update) into
  any .NET application. Generates dual-project scaffolding (Client+Upgrade), Bootstrap
  configuration (4 methods), full-state update UI (6 frameworks), 6 strategy implementations
  (Client-Server/OSS/Silent/Differential/CVP/Push), advanced extension points (IPC replacement,
  Bowl crash daemon, custom Hooks, AOT), and deep troubleshooting (50+ known issues).
  All templates target NuGet v10.4.6 stable API and pass dotnet build (0 errors).

  Triggers on: "GeneralUpdate", "auto update", "自动更新", "update framework",
  ".NET update", "Claude Code skill suite", "GeneralUpdate Skill CodeGen",
  "generalupdate-init", "generalupdate-ui", "generalupdate-strategy",
  "generalupdate-advanced", "generalupdate-troubleshoot",
  "集成GeneralUpdate", "接入自动更新", "更新技能", "升级框架",
  ".NET自动更新", "双进程更新", "Bootstrap配置",
  "WPF update", "Avalonia update", "MAUI update", "WinForms update",
  "差分更新", "静默更新", "跨版本更新", "OSS更新", "推送更新",
  "Bowl崩溃守护", "IPC通讯", "NamedPipe", "更新UI界面",
  "update troubleshooting", "更新失败排查", "升级报错".
  Also triggers on common .NET + update combinations.

  Contains 5 sub-skills:
  - generalupdate-init: Dual-project scaffolding + Bootstrap config
  - generalupdate-ui: Full-state update UI for 6 frameworks
  - generalupdate-strategy: 6 strategy decision tree + examples
  - generalupdate-advanced: 10+ extension points + Bowl + IPC + AOT
  - generalupdate-troubleshoot: 50+ known issues diagnosis
when_to_use: |
  - First-time integration of GeneralUpdate into any .NET project
  - User needs auto-update capability for WPF/WinForms/Avalonia/MAUI/console app
  - User wants production-ready update code with proper error handling
  - User needs to choose the right update strategy for their deployment scenario
  - User reports update failures and needs deep troubleshooting
  - User wants advanced customization: custom IPC, Bowl daemon, AOT compatibility
  - General Claude Code entry point for anything .NET update related
allowed-tools: "Bash, Read, Write, Edit, Glob, Grep, WebSearch"
---

# 🚀 GeneralUpdate Skill CodeGen

**Claude Code 技能套件** — 帮助 .NET 开发者在 5 分钟内将 [GeneralUpdate](https://github.com/GeneralLibrary/GeneralUpdate) 自动更新系统集成到任意 .NET 应用中。

覆盖 50+ 真实 Issue 发现的已知问题，提供即用型代码生成 + 深度故障排查。

> **Current Version: 0.0.1-bate.1** — targets NuGet `GeneralUpdate.Core ≥ 10.4.6` stable release  
> 兼容性：`v10.4.6`（NuGet 最新稳定版）  
> 所有 32 个模板文件已通过 `dotnet build` 编译验证（0 errors）。

## Skills Overview

| Skill | Command | Description | Coverage |
|-------|---------|-------------|----------|
| 🚀 `generalupdate-init` | `/generalupdate-init` | 双项目脚手架 + Bootstrap 配置（4 种方式） | 4 大场景 + 4 种配置方式 + 完整 API |
| 🎨 `generalupdate-ui` | `/generalupdate-ui` | 自动识别 UI 框架，生成全状态更新窗口（11 种状态） | 6 UI 框架 + 全状态机 + 桥接代码 |
| ⚙️ `generalupdate-strategy` | `/generalupdate-strategy` | 6 种策略决策树 + 混合组合 + 平台差异 | 6 策略 + 4 组合 + 平台对照 |
| 🔧 `generalupdate-advanced` | `/generalupdate-advanced` | 10+ 扩展点 + 4 种 IPC + Bowl + AOT | 10+ 扩展点 + 完整架构图 |
| 🩺 `generalupdate-troubleshoot` | `/generalupdate-troubleshoot` | 50+ 已知问题诊断 + 6 步通用排查 | 8 致命 + 11 高 + 20 中 + 12 低 |

## Quick Start

在 Claude Code 中，只需描述你的需求：

```
"给我的 WPF 应用添加自动更新"
→ 自动激活 generalupdate-init + generalupdate-ui

"更新成功了但启动报错"
→ 自动激活 generalupdate-troubleshoot

"配置 OSS 静默更新"
→ 自动激活 generalupdate-strategy

"添加 Bowl 崩溃守护 + 自定义 Hooks"
→ 自动激活 generalupdate-advanced
```

### Prerequisites

1. **Claude Code**: 需要安装并配置 [Claude Code CLI](https://claude.com/claude-code)
2. **.NET SDK**: 目标项目需基于 .NET 8+（推荐 .NET 10）
3. **GeneralUpdate 服务端**: 对于标准策略，需要部署 [GeneralSpacestation](https://github.com/JusterZhu/GeneralSpacestation) 或兼容的后端服务
4. **双进程架构**: 需要理解 Client + Upgrade 双进程的核心理念

## Data Sources

所有技能的内容基于以下真实数据：

- **GitHub Issues**: #308–#517（重构、Bug、功能、测试）
- **Gitee Issues**: 30 个真实用户反馈（中文社区痛点）
- **全面代码审计**: 17 CRITICAL/HIGH + 14 MEDIUM + 10 INFO 发现
- **Samples 源码**: CompleteUpdateSample、SilentUpdateSample、OssSample、DifferentialSample、PushSample、BowlSample、ExtensionSample、CompressSample、ImDiskQuickInstallSample
- **UI Samples**: SemiUrsa、LayUI、AntdUI、WPFDevelopers、MauiUpdate、AndroidUpdate

## Skill File Structure

```
.claude/skills/
├── generalupdate-init/         (7 files)
│   ├── SKILL.md                ← 4 大场景 + 4 种配置 + API 详解
│   ├── reference.md            ← NuGet/API/协议/框架兼容性
│   └── templates/
│       ├── MinimalIntegration.cs
│       ├── FullIntegration.cs
│       ├── generalupdate.manifest.json
│       └── project-scaffold/
│
├── generalupdate-ui/           (10 files)
│   ├── SKILL.md                ← 11 状态 UI 状态机 + 框架检测逻辑
│   └── templates/
│       ├── RealDownloadService.cs
│       ├── DownloadViewModels.cs
│       ├── SemiUrsaClientView.axaml / SemiUrsaUpgradeView.axaml
│       ├── LayUIStyle.xaml / WPFDevelopersStyle.xaml
│       ├── AntdUIStyle.cs
│       └── MauiUpdatePage.xaml/.cs
│
├── generalupdate-strategy/     (7 files)
│   ├── SKILL.md                ← 决策树 + 6 策略详解 + 混合 + 平台对照
│   └── examples/
│       ├── ClientServerStrategy.cs / OssStrategy.cs
│       ├── SilentStrategy.cs / DifferentialStrategy.cs
│       └── CrossVersionStrategy.cs / PushStrategy.cs
│
├── generalupdate-advanced/     (6 files)
│   ├── SKILL.md                ← 10+ 扩展点 + 4 IPC + Bowl + 事件系统
│   ├── reference.md
│   └── templates/
│       ├── CustomHooks.cs / CustomStrategy.cs
│       ├── BowlIntegration.cs / NamedPipeIPC.cs
│
└── generalupdate-troubleshoot/ (2 files)
    ├── SKILL.md                ← 诊断工作流
    └── reference.md            ← 50+ 症状清单（C/H/M/L 四级）
```

## API Compatibility

> ⚠️ **NuGet Reference Rules**:
> - Core only: `dotnet add package GeneralUpdate.Core`
> - With Bowl: reference **only** `GeneralUpdate.Bowl`（传递依赖 Core，两者不能共存）
> - Differential 已嵌入 Core，**无需**额外引用 `GeneralUpdate.Differential`

> ⚠️ **API Surface**: v10.4.6 稳定版 API 与开发分支（v10.5.0-beta.2）有根本性差异。当前稳定版不支持：
> - ❌ 无可编程 `Option` 配置系统（仅 `Configinfo` 属性）
> - ❌ 无 `IUpdateHooks` 生命周期钩子
> - ❌ 无 `IStrategy` 可替换策略接口
> - ❌ 无 `SilentPollOrchestrator`
> - ❌ 无 `ProcessContract` / IPC 替换接口

## Version History

### 0.0.1-bate.1 — 2026-06-16

Initial beta release. All templates rewritten for NuGet v10.4.6 stable API.

## License

Apache 2.0 — 与 GeneralUpdate 主项目一致

## Related Projects

- [GeneralUpdate](https://github.com/GeneralLibrary/GeneralUpdate) — .NET 自动更新核心库
- [GeneralSpacestation](https://github.com/JusterZhu/GeneralSpacestation) — 更新服务端
- [GeneralUpdate-Samples](https://github.com/GeneralLibrary/GeneralUpdate-Samples) — 示例项目合集
