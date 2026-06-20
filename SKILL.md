---
name: generalupdate-skill-codegen
description: |
  Complete Claude Code skill suite for integrating GeneralUpdate (.NET auto-update) into
  any .NET application. Generates dual-project scaffolding (Client+Upgrade), Bootstrap
  configuration (4 methods), full-state update UI (6 frameworks), 6 strategy implementations
  (Client-Server/OSS/Silent/Differential/CVP/Push), advanced extension points (IPC replacement,
  Bowl crash daemon, custom Hooks, AOT), and deep troubleshooting (50+ known issues).
  All templates target NuGet v10.5.0-beta.6 API.

  Triggers on: "GeneralUpdate", "auto update", "自动更新", "update framework",
  ".NET update", "Claude Code skill suite", "GeneralUpdate Skill CodeGen",
  "generalupdate-init", "generalupdate-ui", "generalupdate-strategy",
  "generalupdate-advanced", "generalupdate-troubleshoot",
  "generalupdate-migration", "generalupdate-security-audit",
  "集成GeneralUpdate", "接入自动更新", "更新技能", "升级框架",
  ".NET自动更新", "双进程更新", "Bootstrap配置",
  "WPF update", "Avalonia update", "MAUI update", "WinForms update",
  "差分更新", "静默更新", "跨版本更新", "OSS更新", "推送更新",
  "Bowl崩溃守护", "IPC通讯", "NamedPipe", "更新UI界面",
  "update troubleshooting", "更新失败排查", "升级报错".
  Also triggers on common .NET + update combinations.

  Contains 7 sub-skills:
  - generalupdate-init: Dual-project scaffolding + Bootstrap config
  - generalupdate-ui: Full-state update UI for 6 frameworks
  - generalupdate-strategy: 6 strategy decision tree + examples
  - generalupdate-advanced: 10+ extension points + Bowl + IPC + AOT
  - generalupdate-troubleshoot: 50+ known issues diagnosis
  - generalupdate-migration: v9.x → v10 / dev-branch → stable migration
  - generalupdate-security-audit: Security audit for update pipeline
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

> **Current Version: 0.0.2-beta.1** — targets NuGet `GeneralUpdate.Core 10.5.0-beta.6`  
> 兼容性：`v10.5.0-beta.6`（NuGet 最新预览版）  
> 所有模板已通过 `dotnet build` 编译验证（0 errors）。

---

## 🧭 开发者集成路线图

**你是哪种情况？找到你的入口，按步骤推进：**

| 你的场景 | 从哪开始 | 做什么 | 完成后下一步 |
|---------|---------|-------|-------------|
| 🆕 **第一次加更新，从零开始** | `/generalupdate-init` | ① 选集成模式 → ② 生成 Bootstrap → ③ 部署 | `/generalupdate-ui`（加界面） |
| 🎨 **已有集成，需要更新界面** | `/generalupdate-ui` | ① 自动检测框架 → ② 生成窗口 → ③ 桥接事件 | `/generalupdate-strategy`（选策略） |
| ⚙️ **要选更新策略（OSS/静默/差分）** | `/generalupdate-strategy` | ① 决策树选策略 → ② 配置服务端 → ③ 示例代码 | `/generalupdate-init`（配置 Bootstrap） |
| 🔧 **需要高级定制（Bowl/IPC/Hooks）** | `/generalupdate-advanced` | ① 选扩展点 → ② 生成模板代码 → ③ 集成 | 部署验证 |
| 🩺 **更新失败/报错/异常** | `/generalupdate-troubleshoot` | ① 症状收集 → ② 匹配已知问题 → ③ 修复 | 回到对应 skill 改配置 |
| 📦 **已有 v9.x 要迁移到 v10** | `/generalupdate-init` | 参考"从旧版迁移"章节 + 重新生成配置 | `/generalupdate-troubleshoot`（检查迁移问题） |

### 5 个问题快速定位

回答以下问题，系统会自动推荐应该从哪个 skill 开始：

```
Q1: 你的项目已经能正常编译运行吗？
  ├── 能 → Q2
  └── 不能 → 先确保项目能编译，再回来

Q2: 你已经有 GeneralUpdate NuGet 包了吗？
  ├── 有 → Q4
  └── 没有 / 不确定 → 推荐: /generalupdate-init

Q3（接 Q2 没有）: 你需要显示更新进度给用户看吗？
  ├── 要 → 推荐: /generalupdate-ui（会自动引导 init）
  └── 不要 → 推荐: /generalupdate-init（生成控制台版）

Q4（接 Q2 有）: 更新成功了吗？
  ├── 成功了 → Q5
  └── 失败了/报错 → 推荐: /generalupdate-troubleshoot

Q5（接 Q4 成功）: 你需要什么？
  ├── 更省带宽 → 推荐: /generalupdate-strategy → Differential
  ├── 后台自动更新 → 推荐: /generalupdate-strategy → Silent
  ├── 崩溃自动恢复 → 推荐: /generalupdate-advanced → Bowl
  └── 以上都不 → 部署验证，恭喜！🎉
```

---

## 📋 用户需求提取模板

当开发者描述需求时，必须提取以下信息。不确定的字段**必须追问**。

```
### 技术栈（必需）
- .NET 版本: ______（.NET 6/8/9/10）
- UI 框架: ______（WPF/WinForms/Avalonia/MAUI/控制台/无）
- 目标平台: ______（Windows/Linux/macOS/多平台）

### 部署环境（必需）
- 更新策略倾向: ______（标准服务端/OSS/静默/差分/跨版本/推送）
- 是否有后端服务: ______（是/否，如 GeneralSpacestation）
- 如果是 OSS: ______（S3/MinIO/阿里云OSS/其他）

### 集成阶段（必需）
- 当前阶段: ______（① 从零开始 / ② 已有部分集成 / ③ 遇到问题 / ④ 迁移升级）
- 是否已有 Bootstrap 代码: ______（是/否）
- 是否需要更新 UI: ______（是/否，什么框架）

### 高级需求（可选）
- 需要崩溃守护（Bowl）: ______（是/否）
- 需要 IPC 替换（NamedPipe）: ______（是/否）
- 需要 AOT 支持: ______（是/否）
- 其他定制: ______
```

---

## 🧩 Skills Overview

| Skill | Command | 一句话 | 覆盖 |
|-------|---------|--------|------|
| 🚀 `generalupdate-init` | `/generalupdate-init` | 双项目脚手架 + Bootstrap 配置（4 种方式） | 4 大场景 + 4 种配置方式 + 完整 API |
| 🎨 `generalupdate-ui` | `/generalupdate-ui` | 自动识别 UI 框架，生成全状态更新窗口（11 种状态） | 6 UI 框架 + 全状态机 + 桥接代码 |
| ⚙️ `generalupdate-strategy` | `/generalupdate-strategy` | 6 种策略决策树 + 混合组合 + 平台差异 | 6 策略 + 4 组合 + 平台对照 |
| 🔧 `generalupdate-advanced` | `/generalupdate-advanced` | 10+ 扩展点 + 4 种 IPC + Bowl + AOT | 10+ 扩展点 + 完整架构图 |
| 🩺 `generalupdate-troubleshoot` | `/generalupdate-troubleshoot` | 50+ 已知问题诊断 + 6 步通用排查 | 8 致命 + 11 高 + 20 中 + 12 低 |
| 🔄 `generalupdate-migration` | `/generalupdate-migration` | v9.x → v10 / dev-branch → stable 迁移 | 2 条迁移路径 + API 对照表 |
| 🔒 `generalupdate-security-audit` | `/generalupdate-security-audit` | 安全审计 + 修复建议 | 14 项安全矩阵 + 审计报告模板 |

---

## 🎯 输出格式说明

所有代码生成遵循以下结构化输出：

```
### 📦 生成内容总览
- Bootstrap 配置: Minimal/Standard/Full + appsettings.json
- UI 框架: WPF(AntdUI/LayUI/WPFDevelopers)/Avalonia(SemiUrsa)/MAUI
- 更新策略: Client-Server/OSS/Silent/Differential/CVP/Push

### 🔧 关键决策
- 选择理由: ______
- 为什么是这个策略: ______
- 为什么不是其他策略: ______

### ⚠️ 已知问题预警
- 该配置组合下已知问题: ______
- 避坑指南: ______

### ✅ 部署检查清单
- [ ] 必填项已填
- [ ] NuGet 版本一致
- [ ] UpgradeApp 已发布
- [ ] 安全配置已验证
```

---

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

"把 v9.x 的项目迁移到 v10"
→ 自动激活 generalupdate-init（参考迁移章节）
```

### Prerequisites

1. **Claude Code**: 需要安装并配置 [Claude Code CLI](https://claude.com/claude-code)
2. **.NET SDK**: 目标项目需基于 .NET 8+（推荐 .NET 10）
3. **GeneralUpdate 服务端**: 对于标准策略，需要部署 [GeneralSpacestation](https://github.com/JusterZhu/GeneralSpacestation) 或兼容的后端服务
4. **双进程架构**: 需要理解 Client + Upgrade 双进程的核心理念

---

## 通用集成验证清单

无论使用哪个 skill，完成集成后请逐项检查：

### Bootstrap 配置
- [ ] `Configinfo` 的 6 个必填字段都已设置（UpdateUrl, AppSecretKey, AppName, MainAppName, ClientVersion, ProductId, InstallPath）
- [ ] `UpdateUrl` 指向的服务端 API 可正常返回版本信息
- [ ] `AppSecretKey` 长度 ≥ 16 字符，与服务端一致
- [ ] `InstallPath` 指向正确的安装目录（生产环境用 `AppDomain.CurrentDomain.BaseDirectory`）
- [ ] `AppType` 设置正确（Client = 1, Upgrade = 2）

### NuGet & 编译
- [ ] Client 和 Upgrade 项目使用**完全相同**的 GeneralUpdate NuGet 版本
- [ ] 如果用 Bowl：项目中只能有 `GeneralUpdate.Bowl`，不能同时有 `GeneralUpdate.Core`
- [ ] 项目能正常 `dotnet build`（0 errors）

### 部署结构
- [ ] UpgradeApp.exe 存在于发布目录（首个版本就必须有）
- [ ] `generalupdate.manifest.json` 的 `UpdateAppName` 包含 `.exe`
- [ ] IPC 文件（`UpdateInfo.msg`）路径在 Client/Upgrade 间一致
- [ ] `Encoding` 设置为 `Encoding.UTF8`（防止 Linux/macOS 中文乱码）

### 安全（可选但推荐）
- [ ] `AppSecretKey` 使用强密码（大小写 + 数字 + 符号，≥ 32 字符）
- [ ] 生产环境使用 HTTPS 的 UpdateUrl
- [ ] OSS 场景下 Bucket 权限设置为私有

---

## ⚠️ 通用反模式清单

以下错误在所有集成场景中反复出现，务必避免：

| # | 反模式 | 后果 | 正确做法 |
|---|--------|------|---------|
| 1 | **Core 和 Bowl 引用到同一个项目** | CS0433 类型冲突，编译失败 | 用 Bowl 时只引 Bowl |
| 2 | **Client/Upgrade NuGet 版本号不一致** | 运行时 MethodNotFoundException | 锁定完全相同版本 |
| 3 | **事件监听中做耗时操作（网络 IO / 磁盘 IO）** | Update 进程 UI 卡死，超时被 Kill | 仅更新 UI 状态，耗时操作异步 |
| 4 | **IPC 文件编码未设置 UTF-8** | Linux/macOS 中文乱码 | `Encoding.UTF8` |
| 5 | **UpgradeApp.exe 不随首个版本发布** | 第一次更新时 FileNotFoundException | 首个版本就包含 UpgradeApp |
| 6 | **版本号不是 4 段式（如 1.0.0.0）** | 版本比较逻辑异常 | 始终用 `x.y.z.w` 格式 |
| 7 | **manifest.json 的 mainAppName 不匹配真实进程名** | 更新后主程序找不到 | 和实际 exe 名称一致 |
| 8 | **为旧版 GeneralUpdate 编写的代码直接用在 v10** | API 不兼容，编译失败 | 对照 v10.4.6 稳定版 API 重写 |

---

## Data Sources

所有技能的内容基于以下真实数据：

- **GitHub Issues**: #308–#517（重构、Bug、功能、测试）
- **Gitee Issues**: 30 个真实用户反馈（中文社区痛点）
- **全面代码审计**: 17 CRITICAL/HIGH + 14 MEDIUM + 10 INFO 发现
- **Samples 源码**: CompleteUpdateSample、SilentUpdateSample、OssSample、DifferentialSample、PushSample、BowlSample、ExtensionSample、CompressSample、ImDiskQuickInstallSample
- **UI Samples**: SemiUrsa、LayUI、AntdUI、WPFDevelopers、MauiUpdate、AndroidUpdate

---

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
├── generalupdate-troubleshoot/ (5+ files)
│   ├── SKILL.md                ← 诊断工作流
│   ├── reference.md            ← 50+ 症状清单（C/H/M/L 四级）
│   ├── scripts/search.py       ← BM25 搜索引擎
│   ├── scripts/core.py         ← BM25 算法核心
│   └── data/issues.csv         ← 51 条已知问题数据库
│
├── generalupdate-migration/    (1 file)
│   └── SKILL.md                ← v9.x→v10 / dev-branch→stable 迁移
│
└── generalupdate-security-audit/ (1 file)
    └── SKILL.md                ← 14 项安全审计矩阵
```

---

## API Compatibility

> ⚠️ **NuGet Reference Rules**:
> - Core only: `dotnet add package GeneralUpdate.Core --version 10.5.0-beta.6`
> - With Bowl: reference **only** `GeneralUpdate.Bowl`（传递依赖 Core，两者不能共存）
> - Differential 已嵌入 Core，**无需**额外引用 `GeneralUpdate.Differential`

> ⚠️ **API Surface**: v10.5.0-beta.6 采用了全新的配置系统：
> - ✅ `UpdateRequest` / `UpdateRequestBuilder` — 替代旧的 Configinfo
> - ✅ `SetSource(updateUrl, appSecretKey)` — 零配置入口
> - ✅ `SetOption<T>(Option<T>, T)` — 可编程配置系统
> - ✅ `IUpdateHooks` — 生命周期钩子（Hooks<T>()）
> - ✅ `IStrategy` — 可替换策略接口（Strategy<T>()）
> - ✅ `UseDiffPipeline(Action<DiffPipelineBuilder>)` — 差分管道配置
> - ✅ `SilentPollOrchestrator` — 静默轮询
> - ✅ `AddListenerProgress` — 第 7 个事件监听器
> - ✅ `AddEventListener<TListener>()` — 批量注册
> - ❌ `Configinfo` 类已被移除

---

## Version History

### 0.0.2-bate.1 — 2026-06-16

Updated for GeneralUpdate v10.5.0-beta.6 API:
- Configinfo → UpdateRequest (namespace: `GeneralUpdate.Core.Configuration`)
- Event args moved to `GeneralUpdate.Core.Download` and `GeneralUpdate.Core.Event`
- Added SetSource(), SetOption(), Hooks<T>(), Strategy<T>() API coverage
- Updated all strategy examples to use the new API
- Updated CustomHooks.cs and CustomStrategy.cs to show v10.5 capabilities
- Fixed IsComplated → IsCompleted (typo was in NuGet stable, fixed in beta)
- NuGet version bumped to `10.5.0-beta.6`

### 0.0.1-bate.1 — 2026-06-16

Initial beta release. All templates rewritten for NuGet v10.5.0-beta.6 API.

---

## License

Apache 2.0 — 与 GeneralUpdate 主项目一致

## Related Projects

- [GeneralUpdate](https://github.com/GeneralLibrary/GeneralUpdate) — .NET 自动更新核心库
- [GeneralSpacestation](https://github.com/JusterZhu/GeneralSpacestation) — 更新服务端
- [GeneralUpdate-Samples](https://github.com/GeneralLibrary/GeneralUpdate-Samples) — 示例项目合集
