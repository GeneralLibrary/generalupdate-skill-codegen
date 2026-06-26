---
name: generalupdate-skill-codegen
description: |
  Complete Claude Code skill suite for integrating GeneralUpdate (.NET auto-update) into
  any .NET application — desktop (WPF/WinForms/Avalonia/MAUI/console) and mobile
  (Avalonia.Android/MAUI.Android). Generates dual-project scaffolding (Client+Upgrade),
  Bootstrap configuration (4 methods), full-state update UI (6 frameworks), 6 strategy
  implementations (Client-Server/OSS/Silent/Differential/CVP/Push), advanced extension points
  (IPC replacement, Bowl crash daemon, custom Hooks, AOT), mobile auto-update integration
  (3-step Avalonia / 2-step MAUI + FileProvider), and deep troubleshooting (50+ known issues).
  All templates target NuGet v10.5.0-rc.1 API.

  Triggers on: "GeneralUpdate", "auto update", "自动更新", "update framework",
  ".NET update", "Claude Code skill suite", "GeneralUpdate Skill CodeGen",
  "generalupdate-init", "generalupdate-ui", "generalupdate-strategy",
  "generalupdate-advanced", "generalupdate-troubleshoot",
  "generalupdate-migration", "generalupdate-security-audit",
  "generalupdate-mobile",
  "集成GeneralUpdate", "接入自动更新", "更新技能", "升级框架",
  ".NET自动更新", "双进程更新", "Bootstrap配置",
  "WPF update", "Avalonia update", "MAUI update", "WinForms update",
  "差分更新", "静默更新", "跨版本更新", "OSS更新", "推送更新",
  "Bowl崩溃守护", "IPC通讯", "NamedPipe", "更新UI界面",
  "update troubleshooting", "更新失败排查", "升级报错",
  "GeneralUpdate.Avalonia", "GeneralUpdate.Maui", "Avalonia update",
  "MAUI update", "Android update", "移动端更新", "安卓更新",
  "Avalonia自动更新", "MAUI自动更新", "Android自动更新", "mobile auto update",
  "Avalonia Android update", "MAUI Android update",
  "集成移动端更新", "接入安卓自动更新", "APK update".
  Also triggers on common .NET + update combinations.

  Contains 8 sub-skills:
  - generalupdate-init: Dual-project scaffolding + Bootstrap config
  - generalupdate-ui: Full-state update UI for 6 frameworks
  - generalupdate-strategy: 6 strategy decision tree + examples
  - generalupdate-advanced: 10+ extension points + Bowl + IPC + AOT
  - generalupdate-troubleshoot: 50+ known issues diagnosis
  - generalupdate-migration: v9.x → v10 / dev-branch → stable migration
  - generalupdate-security-audit: Security audit for update pipeline
  - generalupdate-mobile: Mobile auto-update for Avalonia.Android & MAUI.Android apps
when_to_use: |
  - First-time integration of GeneralUpdate into any .NET project (desktop or mobile)
  - User needs auto-update capability for WPF/WinForms/Avalonia/MAUI/console app
  - User needs mobile auto-update for Avalonia.Android or .NET MAUI Android
  - User wants production-ready update code with proper error handling
  - User needs to choose the right update strategy for their deployment scenario
  - User reports update failures and needs deep troubleshooting
  - User wants advanced customization: custom IPC, Bowl daemon, AOT compatibility
  - General Claude Code entry point for anything .NET update related
allowed-tools: "Bash, Read, Write, Edit, Glob, Grep, WebSearch"
---

# 🚀 GeneralUpdate Skill CodeGen / GeneralUpdate 技能代码生成器

**Claude Code skill suite** — integrate [GeneralUpdate](https://github.com/GeneralLibrary/GeneralUpdate) auto-update into any .NET application in 5 minutes. Covers desktop (WPF/WinForms/Avalonia/MAUI/console) and mobile (Avalonia.Android/MAUI.Android).

**Claude Code 技能套件** — 帮助 .NET 开发者在 5 分钟内将 [GeneralUpdate](https://github.com/GeneralLibrary/GeneralUpdate) 自动更新系统集成到任意 .NET 应用中。覆盖桌面端（WPF/WinForms/Avalonia/MAUI/控制台）和移动端（Avalonia.Android/MAUI.Android）。

Covers 50+ known issues from real GitHub/Gitee issues, with ready-to-use code generation + deep troubleshooting.

覆盖 50+ 真实 Issue 发现的已知问题，提供即用型代码生成 + 深度故障排查。

> **Current Version: 0.0.3-beta.1** — targets NuGet `GeneralUpdate.Core 10.5.0-rc.1`  
> Compatibility / 兼容性：`v10.5.0-rc.1`（NuGet latest preview / 最新预览版）  
> All templates verified with `dotnet build` (0 errors) / 所有模板已通过 `dotnet build` 编译验证（0 errors）。

---

## 🧭 Developer Integration Roadmap / 开发者集成路线图

**What's your situation? Find your entry point and follow the steps:**

**你是哪种情况？找到你的入口，按步骤推进：**

| Your Scenario / 你的场景 | Start Here / 从哪开始 | What to Do / 做什么 | Next Step / 完成后下一步 |
|---------|---------|-------|-------------|
| 🆕 **First time, starting from scratch / 第一次加更新，从零开始** | `/generalupdate-init` | ① Choose integration mode → ② Generate Bootstrap → ③ Deploy / ① 选集成模式 → ② 生成 Bootstrap → ③ 部署 | `/generalupdate-ui` (add UI / 加界面) |
| 🎨 **Already integrated, need update UI / 已有集成，需要更新界面** | `/generalupdate-ui` | ① Auto-detect framework → ② Generate window → ③ Wire events / ① 自动检测框架 → ② 生成窗口 → ③ 桥接事件 | `/generalupdate-strategy` (choose strategy / 选策略) |
| ⚙️ **Choosing update strategy (OSS/Silent/Differential) / 要选更新策略** | `/generalupdate-strategy` | ① Decision tree → ② Configure server → ③ Example code / ① 决策树选策略 → ② 配置服务端 → ③ 示例代码 | `/generalupdate-init` (configure Bootstrap / 配置 Bootstrap) |
| 🔧 **Advanced customization (Bowl/IPC/Hooks) / 需要高级定制** | `/generalupdate-advanced` | ① Choose extension point → ② Generate template → ③ Integrate / ① 选扩展点 → ② 生成模板代码 → ③ 集成 | Deploy & verify / 部署验证 |
| 📱 **Mobile: Avalonia.Android / MAUI.Android / 移动端更新** | `/generalupdate-mobile` | ① Auto-detect framework → ② Configure FileProvider → ③ Generate integration code / ① 自动检测框架 → ② 配置 FileProvider → ③ 生成集成代码 | Deploy & verify / 部署验证 |
| 🩺 **Update failed / error / exception / 更新失败/报错/异常** | `/generalupdate-troubleshoot` | ① Collect symptoms → ② Match known issues → ③ Fix / ① 症状收集 → ② 匹配已知问题 → ③ 修复 | Return to relevant skill / 回到对应 skill 改配置 |
| 📦 **Migrating from v9.x to v10 / 从 v9.x 迁移到 v10** | `/generalupdate-migration` | ① Identify current version → ② Follow migration path → ③ Verify / ① 识别当前版本 → ② 跟随迁移路径 → ③ 验证 | `/generalupdate-troubleshoot` (check migration issues / 检查迁移问题) |
| 🔒 **Security review / compliance audit / 安全审查/合规审计** | `/generalupdate-security-audit` | ① Collect deployment info → ② Run audit matrix → ③ Generate report / ① 收集部署信息 → ② 运行审计矩阵 → ③ 生成报告 | Return to relevant skill for fixes / 回到对应 skill 修复 |

### 5-Question Quick Locator / 5 个问题快速定位

Answer the following questions to find your starting skill:

回答以下问题，系统会自动推荐应该从哪个 skill 开始：

```
Q1: Can your project compile and run normally? / 你的项目已经能正常编译运行吗？
  ├── Yes / 能 → Q2
  └── No / 不能 → Fix compilation first / 先确保项目能编译，再回来

Q2: Is your target platform mobile (Android)? / 你的目标平台是移动端（Android）吗？
  ├── Yes / 是 → Q2a
  └── No / 不是 → Q3

Q2a: Which mobile framework? / 什么移动端框架？
  ├── Avalonia.Android → Recommend / 推荐: /generalupdate-mobile
  └── .NET MAUI Android → Recommend / 推荐: /generalupdate-mobile

Q3: Do you already have GeneralUpdate NuGet packages? / 你已经有 GeneralUpdate NuGet 包了吗？
  ├── Yes / 有 → Q5
  └── No / 没有 / 不确定 → Recommend / 推荐: /generalupdate-init

Q4 (from Q3 No / 接 Q3 没有): Do you need to show update progress to users? / 需要显示更新进度吗？
  ├── Yes / 要 → Recommend / 推荐: /generalupdate-ui (will auto-guide init / 会自动引导 init)
  └── No / 不要 → Recommend / 推荐: /generalupdate-init (generate console version / 生成控制台版)

Q5 (from Q3 Yes / 接 Q3 有): Is the update working? / 更新成功了吗？
  ├── Yes / 成功了 → Q6
  └── No / error / 失败了/报错 → Recommend / 推荐: /generalupdate-troubleshoot

Q6 (from Q5 Yes / 接 Q5 成功): What do you need? / 你需要什么？
  ├── Save bandwidth / 更省带宽 → Recommend / 推荐: /generalupdate-strategy → Differential
  ├── Background auto-update / 后台自动更新 → Recommend / 推荐: /generalupdate-strategy → Silent
  ├── Crash recovery / 崩溃自动恢复 → Recommend / 推荐: /generalupdate-advanced → Bowl
  └── None of the above / 以上都不 → Deploy & verify, congratulations! / 部署验证，恭喜！🎉
```

---

## 📋 Universal Requirements Extraction Template / 用户需求提取模板

When a developer describes their needs, you must extract the following. **If uncertain, ask:**

当开发者描述需求时，必须提取以下信息。不确定的字段**必须追问：**

```
### Tech Stack (required) / 技术栈（必需）
- .NET version / .NET 版本: ______ (.NET 6/8/9/10)
- UI framework / UI 框架: ______ (WPF/WinForms/Avalonia/MAUI/Console/None 控制台/无)
- Target platform / 目标平台: ______ (Windows/Linux/macOS/Cross-platform 多平台 / Android)

### Mobile (only if target is Android) / 移动端（仅当目标是 Android）
- Mobile framework / 移动端框架: ______ (Avalonia.Android / .NET MAUI Android)
- .csproj TargetFramework: ______ (net10.0-android / net9.0-android / other 其他)

### Deployment Environment (required) / 部署环境（必需）
- Strategy preference / 更新策略倾向: ______ (Standard server 标准服务端 / OSS / Silent 静默 / Differential 差分 / CVP 跨版本 / Push 推送)
- Has backend service? / 是否有后端服务: ______ (Yes/No 是/否, e.g. GeneralSpacestation)
- If OSS / 如果是 OSS: ______ (S3/MinIO/Aliyun OSS 阿里云OSS/Other 其他)

### Integration Stage (required) / 集成阶段（必需）
- Current stage / 当前阶段: ______ (① Starting from scratch 从零开始 / ② Partially integrated 已有部分集成 / ③ Hit an issue 遇到问题 / ④ Migrating 迁移升级)
- Already have Bootstrap code? / 是否已有 Bootstrap 代码: ______ (Yes/No 是/否)
- Need update UI? / 是否需要更新 UI: ______ (Yes/No 是/否, which framework 什么框架)

### Advanced Requirements (optional) / 高级需求（可选）
- Need crash daemon (Bowl)? / 需要崩溃守护: ______ (Yes/No 是/否)
- Need IPC replacement (NamedPipe)? / 需要 IPC 替换: ______ (Yes/No 是/否)
- Need AOT support? / 需要 AOT 支持: ______ (Yes/No 是/否)
- Other customizations / 其他定制: ______
```

---

## 🧩 Skills Overview / 技能总览

| Skill | Command | One-Liner / 一句话 | Coverage / 覆盖 |
|-------|---------|--------|------|
| 🚀 `generalupdate-init` | `/generalupdate-init` | Dual-project scaffolding + Bootstrap config (4 methods) / 双项目脚手架 + Bootstrap 配置（4 种方式） | 4 scenes + 4 config methods + full API / 4 大场景 + 4 种配置方式 + 完整 API |
| 🎨 `generalupdate-ui` | `/generalupdate-ui` | Auto-detect framework, generate full-state update window (11 states) / 自动识别框架，生成全状态更新窗口（11 种状态） | 6 UI frameworks + full state machine + bridge code / 6 UI 框架 + 全状态机 + 桥接代码 |
| ⚙️ `generalupdate-strategy` | `/generalupdate-strategy` | 6 strategy decision tree + hybrid combos + platform diffs / 6 种策略决策树 + 混合组合 + 平台差异 | 6 strategies + 4 combos + platform matrix / 6 策略 + 4 组合 + 平台对照 |
| 🔧 `generalupdate-advanced` | `/generalupdate-advanced` | 10+ extension points + 4 IPC + Bowl + AOT / 10+ 扩展点 + 4 种 IPC + Bowl + AOT | 10+ extension points + full architecture diagram / 10+ 扩展点 + 完整架构图 |
| 🩺 `generalupdate-troubleshoot` | `/generalupdate-troubleshoot` | 50+ known issues diagnosis + 6-step universal workflow / 50+ 已知问题诊断 + 6 步通用排查 | 8 Critical + 11 High + 20 Medium + 12 Low / 8 致命 + 11 高 + 20 中 + 12 低 |
| 🔄 `generalupdate-migration` | `/generalupdate-migration` | v9.x → v10 / dev-branch → stable migration / v9.x → v10 / 开发分支 → 稳定版迁移 | 2 migration paths + API mapping table / 2 条迁移路径 + API 对照表 |
| 🔒 `generalupdate-security-audit` | `/generalupdate-security-audit` | Security audit + remediation / 安全审计 + 修复建议 | 14-item security matrix + audit report template / 14 项安全矩阵 + 审计报告模板 |
| 📱 `generalupdate-mobile` | `/generalupdate-mobile` | Mobile auto-update integration (Avalonia.Android & MAUI.Android) / 移动端自动更新集成 | 2 frameworks + 3/2-step API + FileProvider + event system / 2 框架 + 3/2 步 API + FileProvider + 事件系统 |

---

## 🎯 Output Format / 输出格式说明

All code generation follows this structured output. Scope varies by skill — desktop-only, mobile-only, or both:

所有代码生成遵循以下结构化输出。覆盖范围因技能而异——桌面端、移动端或两者兼有：

```
### 📦 Generated Content Overview / 生成内容总览
- Bootstrap config / Bootstrap 配置: Minimal/Standard/Full + appsettings.json
- UI framework / UI 框架: WPF(AntdUI/LayUI/WPFDevelopers)/Avalonia(SemiUrsa)/MAUI
- Update strategy / 更新策略: Client-Server/OSS/Silent/Differential/CVP/Push
- Mobile / 移动端: NuGet install + AndroidManifest FileProvider + Bootstrap + events

### 🔧 Key Decisions / 关键决策
- Rationale / 选择理由: ______
- Why this strategy / 为什么是这个策略: ______
- Why not the others / 为什么不是其他策略: ______

### ⚠️ Known Issue Warnings / 已知问题预警
- Known issues for this configuration / 该配置组合下已知问题: ______
- Avoidance guide / 避坑指南: ______

### ✅ Deployment Checklist / 部署检查清单
- [ ] Required fields filled / 必填项已填
- [ ] NuGet versions consistent / NuGet 版本一致
- [ ] UpgradeApp.exe deployed (desktop) OR FileProvider configured (mobile) / UpgradeApp 已发布（桌面）或 FileProvider 已配置（移动端）
- [ ] Security config verified / 安全配置已验证
```

---

## Quick Start / 快速上手

Just describe your needs in Claude Code:

在 Claude Code 中，只需描述你的需求：

```
"Add auto-update to my WPF app"
/ "给我的 WPF 应用添加自动更新"
→ Auto-activates: generalupdate-init + generalupdate-ui

"Update succeeded but app crashes on startup"
/ "更新成功了但启动报错"
→ Auto-activates: generalupdate-troubleshoot

"Configure OSS silent update"
/ "配置 OSS 静默更新"
→ Auto-activates: generalupdate-strategy

"Add Bowl crash daemon + custom Hooks"
/ "添加 Bowl 崩溃守护 + 自定义 Hooks"
→ Auto-activates: generalupdate-advanced

"Migrate my v9.x project to v10"
/ "把 v9.x 的项目迁移到 v10"
→ Auto-activates: generalupdate-init (reference migration chapter / 参考迁移章节)

"Add auto-update to my Avalonia Android app"
/ "给我的 Avalonia Android 应用添加自动更新"
→ Auto-activates: generalupdate-mobile

"Integrate Android auto-update into my MAUI app"
/ "MAUI App 接入安卓自动更新"
→ Auto-activates: generalupdate-mobile
```

### Prerequisites / 前置条件

1. **Claude Code**: Install and configure [Claude Code CLI](https://claude.com/claude-code) / 需要安装并配置 Claude Code CLI
2. **.NET SDK**: Target project requires .NET 8+ (.NET 10 recommended / 推荐 .NET 10)；Mobile requires .NET 10.0+ / 移动端需要 .NET 10.0+
3. **Android workload** (mobile only / 仅移动端): `dotnet workload install android`
4. **GeneralUpdate Server**: For standard strategy, deploy [GeneralSpacestation](https://github.com/JusterZhu/GeneralSpacestation) or compatible backend / 对于标准策略，需要部署 GeneralSpacestation 或兼容的后端服务
5. **Dual-process architecture** (desktop only / 仅桌面端): Understand the Client + Upgrade dual-process concept / 需要理解 Client + Upgrade 双进程的核心理念

---

## Universal Integration Checklist / 通用集成验证清单

Regardless of which skill you use, verify these items after integration:

无论使用哪个 skill，完成集成后请逐项检查：

### Desktop: Bootstrap Configuration / 桌面端 — Bootstrap 配置
- [ ] `UpdateRequest` required fields set (UpdateUrl, AppSecretKey, MainAppName, ClientVersion, ProductId, InstallPath) / `UpdateRequest` 的必填字段都已设置
- [ ] `UpdateUrl` endpoint returns valid version info / `UpdateUrl` 指向的服务端 API 可正常返回版本信息
- [ ] `AppSecretKey` length ≥ 16 chars, matches server / `AppSecretKey` 长度 ≥ 16 字符，与服务端一致
- [ ] `InstallPath` points to correct install dir (production: `AppDomain.CurrentDomain.BaseDirectory`) / `InstallPath` 指向正确的安装目录
- [ ] `AppType` set correctly (Client = 1, Upgrade = 2) / `AppType` 设置正确

### Mobile: Android Manifest Configuration / 移动端 — Android Manifest 配置
- [ ] NuGet package installed (`GeneralUpdate.Avalonia.Android` or `GeneralUpdate.Maui.Android`) / NuGet 包安装成功
- [ ] `<provider>` element added to `<application>` in AndroidManifest.xml / `<provider>` 节点已添加到 AndroidManifest.xml 中
- [ ] `android:authorities` matches code's `FileProviderAuthority` exactly / `android:authorities` 与代码中 `FileProviderAuthority` 完全一致
- [ ] `generalupdate_file_paths.xml` created at correct path / `generalupdate_file_paths.xml` 已创建且路径正确
- [ ] `REQUEST_INSTALL_PACKAGES` permission declared (Android 8.0+) / 声明 `REQUEST_INSTALL_PACKAGES` 权限

### NuGet & Compilation / NuGet & 编译
- [ ] Client and Upgrade projects use **exactly the same** GeneralUpdate NuGet version (desktop) / Client 和 Upgrade 项目使用**完全相同**的 NuGet 版本
- [ ] Mobile: NuGet package matches project framework (Avalonia vs MAUI) / 移动端：NuGet 包与项目框架匹配
- [ ] If using Bowl: reference **both** `GeneralUpdate.Core` and `GeneralUpdate.Bowl` (v10.5.0-rc.1 no conflict) / 如果用 Bowl：同时引用 Core 和 Bowl
- [ ] `dotnet build` succeeds with 0 errors / 项目能正常 `dotnet build`

### Deployment / 部署结构
- [ ] Desktop: `UpgradeApp.exe` exists in publish directory (must ship with first version) / UpgradeApp.exe 存在于发布目录（首个版本就必须有）
- [ ] Desktop: `generalupdate.manifest.json` `UpdateAppName` includes `.exe` / manifest.json 的 UpdateAppName 包含 .exe
- [ ] Desktop: IPC file (`UpdateInfo.msg`) path consistent between Client/Upgrade / IPC 文件路径在 Client/Upgrade 间一致
- [ ] Mobile: Server returns non-empty `sha256` and `downloadUrl` in version-info API / 移动端：服务端返回非空的 sha256 和 downloadUrl
- [ ] `Encoding` set to `Encoding.UTF8` (prevents garbled text on Linux/macOS) / Encoding 设置为 UTF-8

### Security (optional but recommended) / 安全（可选但推荐）
- [ ] `AppSecretKey` uses strong password (upper+lower+digits+symbols, ≥ 32 chars) / AppSecretKey 使用强密码（大小写 + 数字 + 符号，≥ 32 字符）
- [ ] Production uses HTTPS for UpdateUrl / 生产环境使用 HTTPS 的 UpdateUrl
- [ ] OSS: Bucket permissions set to private / OSS 场景下 Bucket 权限设置为私有

---

## ⚠️ Universal Anti-Patterns / 通用反模式清单

The following mistakes recur across all integration scenarios. Avoid them:

以下错误在所有集成场景中反复出现，务必避免：

| # | Anti-Pattern / 反模式 | Consequence / 后果 | Correct Approach / 正确做法 |
|---|--------|------|---------|
| 1 | **Not referencing both Core and Bowl (when using Bowl)** / 未同时引用 Core 和 Bowl | Compilation failure, missing Core types / 编译失败，缺少 Core 类型 | Reference both Core and Bowl when using Bowl / 用 Bowl 时同时引 Core 和 Bowl |
| 2 | **Client/Upgrade NuGet version mismatch** / 版本号不一致 | Runtime MethodNotFoundException | Lock to the exact same version / 锁定完全相同版本 |
| 3 | **Blocking I/O in event listeners (network/disk)** / 事件监听中做耗时操作 | Update process UI freeze, killed by timeout / Update 进程 UI 卡死 | Only update UI state; async for heavy work / 仅更新 UI 状态，耗时操作异步 |
| 4 | **IPC file encoding not set to UTF-8** / IPC 文件编码未设置 UTF-8 | Chinese garbled text on Linux/macOS / 中文乱码 | `Encoding.UTF8` |
| 5 | **UpgradeApp.exe not shipped with first release** / 首个版本不包含 UpgradeApp | FileNotFoundException on first update / 第一次更新时报错 | Ship UpgradeApp.exe from the very first version / 首个版本就包含 |
| 6 | **Version not in 4-segment format (e.g. 1.0.0.0)** / 版本号不是 4 段式 | Version comparison logic breaks / 版本比较逻辑异常 | Always use `x.y.z.w` format / 始终用 x.y.z.w 格式 |
| 7 | **manifest.json mainAppName doesn't match actual process name** / mainAppName 不匹配进程名 | Main app not found after update / 更新后主程序找不到 | Match the actual exe name / 和实际 exe 名称一致 |
| 8 | **Legacy GeneralUpdate code used directly on v10** / 旧版代码直接用在 v10 | API incompatibility, compilation failure / API 不兼容 | Rewrite against v10.4.6 stable API / 对照 v10 API 重写 |
| 9 | **Mobile: FileProviderAuthority mismatch between manifest and code** / 移动端：FileProviderAuthority 不一致 | Installer launch fails with `InstallLaunchFailed` / 安装器调起失败 | Use a constant; keep manifest and code identical / 使用常量保持一致 |
| 10 | **Mobile: mixing Avalonia API pattern in MAUI code (or vice versa)** / 移动端：API 模式混用 | Compilation failure / 编译失败 | Avalonia: 3-step; MAUI: 2-step — keep separate / Avalonia: 3 步；MAUI: 2 步，代码不能混用 |
| 11 | **Mobile: using external storage for download directory** / 移动端：下载目录使用外部存储 | Scoped storage restrictions on Android 11+ / 作用域存储限制 | Use `Context.CacheDir` or `Context.FilesDir` / 使用 Context.CacheDir |
| 12 | **Mobile: not disposing Bootstrap** / 移动端：未 dispose Bootstrap | Semaphore leak, file handle leak / 信号量泄漏 | `using var` or explicit `Dispose()` / using var 或手动 Dispose |

---

## Data Sources / 数据来源

All skill content is based on real data:

所有技能的内容基于以下真实数据：

- **GitHub Issues**: #308–#517 (refactoring, bugs, features, testing / 重构、Bug、功能、测试)
- **Gitee Issues**: 30 real user reports (Chinese community pain points / 中文社区痛点)
- **Full Code Audit**: 17 CRITICAL/HIGH + 14 MEDIUM + 10 INFO findings / 全面代码审计发现
- **Desktop Samples**: CompleteUpdateSample, SilentUpdateSample, OssSample, DifferentialSample, PushSample, BowlSample, ExtensionSample, CompressSample, ImDiskQuickInstallSample
- **UI Samples**: SemiUrsa, LayUI, AntdUI, WPFDevelopers, MauiUpdate, AndroidUpdate
- **Mobile Library Source**: GeneralUpdate.Avalonia.Android, GeneralUpdate.Maui.Android / 移动端库源码

---

## Skill File Structure / 技能文件结构

```
.claude/skills/
├── generalupdate-init/         (7 files)
│   ├── SKILL.md                ← 4 scenes + 4 config methods + API reference / 4 大场景 + 4 种配置 + API 详解
│   ├── reference.md            ← NuGet/API/Protocol/Framework compatibility / NuGet/API/协议/框架兼容性
│   └── templates/
│       ├── MinimalIntegration.cs
│       ├── FullIntegration.cs
│       ├── generalupdate.manifest.json
│       └── project-scaffold/
│
├── generalupdate-ui/           (10 files)
│   ├── SKILL.md                ← 11-state UI state machine + framework detection / 11 状态 UI 状态机 + 框架检测
│   └── templates/
│       ├── RealDownloadService.cs
│       ├── DownloadViewModels.cs
│       ├── SemiUrsaClientView.axaml / SemiUrsaUpgradeView.axaml
│       ├── LayUIStyle.xaml / WPFDevelopersStyle.xaml
│       ├── AntdUIStyle.cs
│       └── MauiUpdatePage.xaml/.cs
│
├── generalupdate-strategy/     (7 files)
│   ├── SKILL.md                ← Decision tree + 6 strategy details + hybrid + platform matrix / 决策树 + 6 策略详解 + 混合 + 平台对照
│   └── examples/
│       ├── ClientServerStrategy.cs / OssStrategy.cs
│       ├── SilentStrategy.cs / DifferentialStrategy.cs
│       └── CrossVersionStrategy.cs / PushStrategy.cs
│
├── generalupdate-advanced/     (6 files)
│   ├── SKILL.md                ← 10+ extension points + 4 IPC + Bowl + event system / 10+ 扩展点 + 4 IPC + Bowl + 事件系统
│   ├── reference.md
│   └── templates/
│       ├── CustomHooks.cs / CustomStrategy.cs
│       ├── BowlIntegration.cs / NamedPipeIPC.cs
│
├── generalupdate-troubleshoot/ (5+ files)
│   ├── SKILL.md                ← Diagnostic workflow / 诊断工作流
│   ├── reference.md            ← 50+ symptom catalog (C/H/M/L 4 tiers) / 50+ 症状清单
│   ├── scripts/search.py       ← BM25 search engine / BM25 搜索引擎
│   ├── scripts/core.py         ← BM25 algorithm core / BM25 算法核心
│   └── data/issues.csv         ← 51 known issues database / 51 条已知问题数据库
│
├── generalupdate-migration/    (1 file)
│   └── SKILL.md                ← v9.x→v10 / dev-branch→stable migration / 迁移指南
│
├── generalupdate-security-audit/ (1 file)
│   └── SKILL.md                ← 14-item security audit matrix / 14 项安全审计矩阵
│
└── generalupdate-mobile/        (1 file)
    └── SKILL.md                ← Mobile auto-update integration (Avalonia.Android & MAUI.Android) + FileProvider + events / 移动端自动更新集成 + FileProvider + 事件
```

---

## API Compatibility / API 兼容性

> ⚠️ **NuGet Reference Rules / NuGet 引用规则**:
> - Core only / 仅 Core: `dotnet add package GeneralUpdate.Core --version 10.5.0-rc.1`
> - With Bowl / 配合 Bowl: reference **both** `GeneralUpdate.Core` and `GeneralUpdate.Bowl` (v10.5.0-rc.1: Bowl is standalone, no type conflict; needs separate Core reference / Bowl 为独立包，无类型冲突，需独立引用 Core)
> - Differential embedded in Core — **no additional** `GeneralUpdate.Differential` package needed / 差分已嵌入 Core，无需额外引用
> - Mobile / 移动端: `dotnet add package GeneralUpdate.Avalonia.Android` or `GeneralUpdate.Maui.Android`

> ⚠️ **API Surface / API 层面**: v10.5.0-rc.1 introduces a new configuration system / 采用了全新的配置系统：
> - ✅ `UpdateRequest` / `UpdateRequestBuilder` — replaces old Configinfo / 替代旧的 Configinfo
> - ✅ `SetSource(updateUrl, appSecretKey)` — zero-config entry point / 零配置入口
> - ✅ `SetOption<T>(Option<T>, T)` — programmable option system / 可编程配置系统
> - ✅ `IUpdateHooks` — lifecycle hooks / 生命周期钩子 (`Hooks<T>()`)
> - ✅ `IStrategy` — replaceable strategy interface / 可替换策略接口 (`Strategy<T>()`)
> - ✅ `UseDiffPipeline(Action<DiffPipelineBuilder>)` — diff pipeline config / 差分管道配置
> - ✅ `SilentPollOrchestrator` — silent polling / 静默轮询
> - ✅ `AddListenerProgress` — 7th event listener / 第 7 个事件监听器
> - ✅ `AddEventListener<TListener>()` — batch registration / 批量注册
> - ❌ `Configinfo` class removed / Configinfo 类已被移除

---

## Version History / 版本历史

### 0.0.3-beta.1 — 2026-06-26

- Added `generalupdate-mobile` skill: full mobile auto-update integration for Avalonia.Android & MAUI.Android / 新增移动端自动更新技能
- All 8 sub-skills now fully bilingual (English + Chinese) / 全部 8 个子技能双语化
- Root SKILL.md bilingualized with mobile entry points in roadmap and decision tree / 根 SKILL.md 双语化，路线图和决策树中加入移动端入口
- Universal checklist and anti-patterns now cover both desktop and mobile scenarios / 通用清单和反模式覆盖桌面+移动双场景
- Fixed HTTP auth section in mobile skill: split Avalonia vs MAUI with correct class names and property names / 修正移动端 HTTP 认证段：Avalonia/MAUI 独立区块，类名/属性名全对

### 0.0.2-beta.1 — 2026-06-16

Updated for GeneralUpdate v10.5.0-rc.1 API:
- Configinfo → UpdateRequest (namespace: `GeneralUpdate.Core.Configuration`)
- Event args moved to `GeneralUpdate.Core.Download` and `GeneralUpdate.Core.Event`
- Added SetSource(), SetOption(), Hooks<T>(), Strategy<T>() API coverage
- Updated all strategy examples to use the new API
- Updated CustomHooks.cs and CustomStrategy.cs to show v10.5 capabilities
- Fixed IsComplated → IsCompleted (typo was in NuGet stable, fixed in beta)
- NuGet version bumped to `10.5.0-rc.1`

### 0.0.1-beta.1 — 2026-06-16

Initial beta release. All templates rewritten for NuGet v10.5.0-rc.1 API.

---

## License / 许可证

Apache 2.0 — consistent with GeneralUpdate main project / 与 GeneralUpdate 主项目一致

## Related Projects / 相关项目

- [GeneralUpdate](https://github.com/GeneralLibrary/GeneralUpdate) — .NET auto-update core library / .NET 自动更新核心库
- [GeneralUpdate.Avalonia](https://github.com/GeneralLibrary/GeneralUpdate.Avalonia) — Avalonia.Android auto-update library / Avalonia.Android 自动更新库
- [GeneralUpdate.Maui](https://github.com/GeneralLibrary/GeneralUpdate.Maui) — MAUI.Android auto-update library / MAUI.Android 自动更新库
- [GeneralSpacestation](https://github.com/JusterZhu/GeneralSpacestation) — Update server / 更新服务端
- [GeneralUpdate-Samples](https://github.com/GeneralLibrary/GeneralUpdate-Samples) — Sample projects / 示例项目合集
