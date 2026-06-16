---
name: generalupdate-strategy
description: |
  Configure GeneralUpdate update strategies for any deployment scenario.
  Covers 6 strategies with decision tree, 4 Client-Upgrade update scenes,
  and platform-specific considerations (Windows/Linux/Mac).
  Each strategy includes real issue workarounds from GitHub/Gitee.
  Triggers on: "configure strategy", "OSS update", "silent update", "differential update",
  "cross version update", "push update", "SignalR push", "更新策略", "OSS方案",
  "静默更新", "差分更新", "跨版本", "推送更新", "决策", "选策略",
  "how to update without a server", "background update", "reduce download size",
  "skip versions", "force update", "CVP", "chain packages".
  Also triggers when user mentions specific deployment constraints.
when_to_use: |
  - User asks about different ways to deliver updates (strategy selection)
  - User wants silent/background mode for long-running apps
  - User wants differential/delta updates to save bandwidth
  - User mentions OSS / S3 / MinIO / object storage (no server)
  - User wants cross-version jump (skip intermediate versions)
  - User wants server-push (SignalR) updates
  - User is confused about which strategy fits their use case
  - Best used after generalupdate-init
allowed-tools: "Read, Write, Edit, Glob"
---

# ⚙️ GeneralUpdate 更新策略完全指南

> ⚠️ **针对 NuGet v10.4.6 稳定版**。该版本使用 `Configinfo` 配置，无可编程 `Option` 系统。

---

## 策略决策树

```
你的应用有后端服务吗？
├── 有
│   ├── 需要服务端主动推送更新？ → SignalR 推送
│   └── 否 → 标准客户端-服务端
│
└── 没有（只有对象存储 S3/MinIO/OSS）
    └── OSS 标准
```

---

## 6 种策略详细对比

| 策略 | 服务端 | 说明 |
|------|:------:|------|
| **① 标准客户端-服务端** | ✅ GeneralSpacestation | 有后端的中大型应用（推荐入门） |
| **② OSS 对象存储** | ❌ 仅 S3/MinIO | 无后端，最低成本 |
| **③ 静默更新** | ✅ 同①或② | 后台无声升级 |
| **④ 差分更新** | ✅ 需差分构建 | 增量补丁节省带宽 |
| **⑤ 跨版本 CVP** | ✅ 需 CVP 构建 | 跳过中间版本直跳 |
| **⑥ SignalR 推送** | ✅ 需 SignalR Hub | 服务端主动推送 |

---

## 集成代码

所有策略使用相同的 `Configinfo` 配置模式：

```csharp
using GeneralUpdate.Core;
using GeneralUpdate.Common.Shared.Object;

var config = new Configinfo
{
    UpdateUrl = "https://your-server.com/api",
    AppSecretKey = "your-secret-key",
    AppName = "MyApp.exe",
    MainAppName = "MyApp.exe",
    ClientVersion = "1.0.0.0",
    ProductId = "my-product-001",
    InstallPath = ".",
};

await new GeneralUpdateBootstrap()
    .SetConfig(config)
    .AddListener*(...)
    .LaunchAsync();
```

具体示例参见 `examples/` 目录下的策略文件。

---

## 平台特定差异

| 平台 | 特性 |
|------|------|
| **Windows** | 完整功能 |
| **Linux** | 部分功能（无 Bowl） |
| **macOS** | 同 Linux |

---

## 已知问题

| # | 问题 | 规避方案 |
|---|------|---------|
| 1 | OSS 模式不区分 Main/Upgrade 更新 | 接受此行为 |
| 2 | UpgradeApp.exe 必须放在 update/ 子目录 | 按规范部署 |
| 3 | NuGet 版本冲突导致 "Method not found" | Client 和 Upgrade 使用相同版本号 |
| 4 | 无限升级循环 | 确保 manifest.json 版本号正确 |
| 5 | SignalR HubConnection Dispose 后重连崩溃 | Dispose 时将连接置 null |

---

## 相关技能

- `/generalupdate-init` — 如果还未配置 Bootstrap
- `/generalupdate-ui` — 如果需要更新界面
- `/generalupdate-troubleshoot` — 如果遇到问题
- `/generalupdate-advanced` — 高级定制（适用于开发分支）
