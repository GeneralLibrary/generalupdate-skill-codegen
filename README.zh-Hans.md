# GeneralUpdate Skill CodeGen

**Claude Code 技能套件** — 帮助 .NET 开发者在 5 分钟内将 [GeneralUpdate](https://github.com/GeneralLibrary/GeneralUpdate) 自动更新系统集成到任意 .NET 应用中。

覆盖 50+ 真实 Issue 发现的已知问题，提供即用型代码生成 + 深度故障排查。

> 当前版本：0.0.3-beta.1 — 针对 NuGet `GeneralUpdate.Core 10.5.0-rc.1`
> 兼容性：`v10.5.0-rc.1`（NuGet 最新预览版）
> **所有模板已通过 `dotnet build` 编译验证（0 errors）。**

---

## 技能总览

| 技能 | 命令 | 一句话功能 | 覆盖量 |
|------|------|-----------|--------|
| 🚀 `generalupdate-init` | `/generalupdate-init` | 双项目脚手架 + Bootstrap 配置（4 种方式） | 4 大场景 + 4 种配置方式 + 完整 API |
| 🎨 `generalupdate-ui` | `/generalupdate-ui` | 自动识别 UI 框架，生成全状态更新窗口（11 种状态） | 6 UI 框架 + 全状态机 + 桥接代码 |
| ⚙️ `generalupdate-strategy` | `/generalupdate-strategy` | 6 种策略决策树 + 混合组合 + 平台差异 | 6 策略 + 4 组合 + 平台对照 |
| 🔧 `generalupdate-advanced` | `/generalupdate-advanced` | 10+ 扩展点 + 4 种 IPC + Bowl + AOT | 10+ 扩展点 + 完整架构图 |
| 🩺 `generalupdate-troubleshoot` | `/generalupdate-troubleshoot` | 50+ 已知问题诊断 + 6 步通用排查 | 8 致命 + 11 高 + 20 中 + 12 低 |

---

## 快速开始

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

### 使用前提

1. **Claude Code**：需要安装并配置 [Claude Code CLI](https://claude.com/claude-code)
2. **.NET SDK**：目标项目需基于 .NET 8+（推荐 .NET 10）
3. **GeneralUpdate 服务端**：对于标准策略，需要部署 [GeneralSpacestation](https://github.com/JusterZhu/GeneralSpacestation) 或兼容的后端服务
4. **双进程架构**：需要理解 Client + Upgrade 双进程的核心理念

---

## 数据来源

所有技能的内容基于以下真实数据：

- **GitHub Issues**: #308–#517（重构、Bug、功能、测试）
- **Gitee Issues**: 30 个真实用户反馈（中文社区痛点）
- **全面代码审计**: 17 CRITICAL/HIGH + 14 MEDIUM + 10 INFO 发现
- **Samples 源码**: CompleteUpdateSample、SilentUpdateSample、OssSample、DifferentialSample、PushSample、BowlSample、ExtensionSample、CompressSample、ImDiskQuickInstallSample
- **UI Samples**: SemiUrsa、LayUI、AntdUI、WPFDevelopers、MauiUpdate、AndroidUpdate

---

## 技能文件结构

```
.claude/skills/
├── generalupdate-init/         (7 files)
│   ├── SKILL.md                     ← 4大场景 + 3种配置 + API详解
│   ├── reference.md                 ← NuGet/API/协议/框架兼容性
│   └── templates/
│       ├── MinimalIntegration.cs    ← 3行代码 + 注释说明
│       ├── FullIntegration.cs       ← 完整配置 + Upgrade进程 + appsettings
│       ├── generalupdate.manifest.json
│       └── project-scaffold/
│           ├── ClientApp.csproj / ClientProgram.cs
│           └── UpgradeApp.csproj / UpgradeProgram.cs
│
├── generalupdate-ui/           (10 files)
│   ├── SKILL.md                     ← 11状态UI状态机 + 框架检测逻辑
│   └── templates/
│       ├── RealDownloadService.cs   ← ★ 核心桥接 Mock→GeneralUpdate
│       ├── DownloadViewModels.cs    ← 全状态MVVM ViewModel
│       ├── SemiUrsaClientView.axaml ← Avalonia全状态窗口
│       ├── SemiUrsaUpgradeView.axaml
│       ├── LayUIStyle.xaml          ← WPF+LayUI
│       ├── WPFDevelopersStyle.xaml  ← WPF+WPFDevelopers
│       ├── AntdUIStyle.cs           ← WinForms+AntdUI
│       └── MauiUpdatePage.xaml/.cs  ← MAUI
│
├── generalupdate-strategy/     (7 files)
│   ├── SKILL.md                     ← 决策树 + 6策略详解 + 混合 + 平台对照
│   └── examples/
│       ├── ClientServerStrategy.cs  ← 标准服务端模式
│       ├── OssStrategy.cs           ← 对象存储模式
│       ├── SilentStrategy.cs        ← 静默轮询模式
│       ├── DifferentialStrategy.cs  ← 差分更新模式
│       ├── CrossVersionStrategy.cs  ← 跨版本CVP模式
│       └── PushStrategy.cs          ← SignalR推送模式
│
├── generalupdate-advanced/     (6 files)
│   ├── SKILL.md                     ← 10+扩展点 + 4 IPC + Bowl + 事件系统
│   ├── reference.md                 ← 扩展点速查 + Bowl选项
│   └── templates/
│       ├── CustomHooks.cs           ← 完整IUpdateHooks + Unix权限
│       ├── CustomStrategy.cs        ← 自定义平台策略
│       ├── BowlIntegration.cs       ← 崩溃守护配置
│       └── NamedPipeIPC.cs          ← 命名管道IPC替换
│
└── generalupdate-troubleshoot/ (2 files)
    ├── SKILL.md                     ← 诊断工作流
    └── reference.md                 ← ★ 50+症状清单（C/H/M/L四级）
```

---

## 已知限制与注意事项

> ⚠️ **NuGet 包引用规则**：
> - 使用 Core：`dotnet add package GeneralUpdate.Core`
> - 使用 Bowl：**只引用** `GeneralUpdate.Bowl`（它传递依赖 Core，两者不能同时引用）
> - 差分功能已内嵌在 Core 中，**无需额外引用** `GeneralUpdate.Differential`

> ⚠️ **API 说明**：v10.5.0-rc.1 采用 `UpdateRequest` 配置系统，支持可编程 `Option`、`IUpdateHooks` 生命周期钩子、`SetSource()` 零配置入口等新特性。详情请查看 BUGS.md。

详见 [BUGS.md](BUGS.md)。

---

## 如何贡献

1. 提交 Issue 报告 Bug 或功能请求
2. Fork 本仓库，在 `.claude/skills/` 下添加或修改技能
3. 确保模板代码与 GeneralUpdate 最新版 API 一致
4. 提交 PR

### 开发指南

```bash
# 本地测试技能
# 在 Claude Code 中加载技能
/claude-code --load-skills .claude/skills/

# 验证模板代码可编译
dotnet build your-test-project/
```

---

## 许可证

Apache 2.0 — 与 GeneralUpdate 主项目一致

## 相关项目

- [GeneralUpdate](https://github.com/GeneralLibrary/GeneralUpdate) — .NET 自动更新核心库
- [GeneralSpacestation](https://github.com/JusterZhu/GeneralSpacestation) — 更新服务端
- [GeneralUpdate-Samples](https://github.com/GeneralLibrary/GeneralUpdate-Samples) — 示例项目合集
