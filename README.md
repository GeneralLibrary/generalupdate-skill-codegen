# GeneralUpdate Skill CodeGen

**Claude Code 技能套件** — 帮助 .NET 开发者在 5 分钟内将 [GeneralUpdate](https://github.com/GeneralLibrary/GeneralUpdate) 自动更新系统集成到任意 .NET 应用中。

## 技能总览

| 技能 | 命令 | 一句话功能 |
|------|------|-----------|
| 🚀 `generalupdate-init` | `/generalupdate-init` | 生成双项目脚手架 + Bootstrap 配置，三行代码集成 |
| 🎨 `generalupdate-ui` | `/generalupdate-ui` | 自动识别 UI 框架，生成带真实进度绑定的更新窗口（无需写 UI） |
| ⚙️ `generalupdate-strategy` | `/generalupdate-strategy` | 按场景配置 6 种更新策略（标准/OSS/静默/差分/CVP/推送） |
| 🔧 `generalupdate-advanced` | `/generalupdate-advanced` | 高级定制：Hooks、自定义策略、Bowl守护、IPC、插件 |
| 🩺 `generalupdate-troubleshoot` | `/generalupdate-troubleshoot` | 20+ 真实 Issue 驱动的故障排查清单 |

## 快速开始

````bash
# 在 Claude Code 中，只需描述你的需求：
"给我的 WPF 应用添加自动更新"
# → 自动激活 generalupdate-init + generalupdate-ui

"更新成功了但启动报错"
# → 自动激活 generalupdate-troubleshoot

"配置 OSS 静默更新"
# → 自动激活 generalupdate-strategy

"添加 Bowl 崩溃守护"
# → 自动激活 generalupdate-advanced
````

## 技能触发方式

1. **自动触发** — 描述中包含触发词，Claude 自动加载对应技能
2. **手动触发** — 输入 `/skill-name` 后面跟具体描述

## 前置知识

- **目标用户**：已有一个 .NET 项目，希望添加自动更新功能
- **GeneralUpdate 版本**：推荐使用最新 NuGet 包（≥ v5.0.0）
- **支持框架**：WPF (.NET 6+/Core)、WinForms、Avalonia、MAUI、控制台应用
- **支持平台**：Windows、Linux、macOS

## 技能文件结构

```
.claude/skills/
├── generalupdate-init/
│   ├── SKILL.md              ← 技能定义 + 执行流程
│   ├── templates/
│   │   ├── MinimalIntegration.cs     ← 三行代码最小集成
│   │   ├── FullIntegration.cs        ← 完整配置集成
│   │   └── UpgradeProject.csproj     ← Upgrade 项目模板
│   ├── project-scaffold/
│   │   ├── ClientApp.csproj          ← 客户端项目模板
│   │   ├── UpgradeApp.csproj         ← 升级端项目模板
│   │   ├── ClientProgram.cs          ← 客户端入口
│   │   └── UpgradeProgram.cs         ← 升级端入口
│   └── reference.md          ← NuGet版本、API速查
│
├── generalupdate-ui/
│   ├── SKILL.md              ← UI框架检测 + 代码生成逻辑
│   └── templates/
│       ├── LayUIStyle.cs              ← WPF+LayUI 更新窗口
│       ├── WPFDevelopersStyle.cs      ← WPF+WPFDevelopers
│       ├── AntdUIStyle.cs             ← WinForms+AntdUI
│       ├── SemiUrsaClientView.axaml   ← Avalonia 客户端视图
│       ├── SemiUrsaUpgradeView.axaml  ← Avalonia 升级视图
│       ├── MauiUpdatePage.xaml        ← MAUI 更新页面
│       ├── RealDownloadService.cs     ← 核心桥接: Mock→真实
│       └── DownloadViewModels.cs      ← MVVM ViewModel 模板
│
├── generalupdate-strategy/
│   ├── SKILL.md
│   └── examples/
│       ├── ClientServerStrategy.cs
│       ├── OssStrategy.cs
│       ├── SilentStrategy.cs
│       ├── DifferentialStrategy.cs
│       ├── CrossVersionStrategy.cs
│       └── PushStrategy.cs
│
├── generalupdate-advanced/
│   ├── SKILL.md
│   ├── templates/
│       ├── CustomHooks.cs
│       ├── CustomStrategy.cs
│       ├── BowlIntegration.cs
│       └── NamedPipeIPC.cs
│   └── reference.md
│
└── generalupdate-troubleshoot/
    ├── SKILL.md
    └── reference.md           ← 症状→根因→修复 清单
```

## 设计原则

1. **生成即用代码** — 所有输出可直接粘贴到 .NET 项目中
2. **场景驱动** — 不需要懂 GeneralUpdate 内部架构，描述你的场景就行
3. **内嵌知识库** — 已知 Bug 和规避方案直接写入 skill 内容
4. **模板 + 桥接** — UI 技能自动检测开发者所用框架并桥接到真实更新引擎

## 许可证

Apache 2.0 — 与 GeneralUpdate 主项目一致
