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

# GeneralUpdate Update Strategy Complete Guide / GeneralUpdate 更新策略完全指南

> **Targeting NuGet v10.5.0-rc.1**. This version uses `UpdateRequest` configuration and supports the programmable `Option` system.
> ⚠️ **针对 NuGet v10.5.0-rc.1**。该版本使用 `UpdateRequest` 配置，支持可编程 `Option` 系统。

---

## User Requirements Checklist (Must Confirm Before Recommending a Strategy) / 用户需求提取（推荐策略前必须确认）

```
### Deployment Environment / 部署环境
- Has backend server: ______ (Yes/No/Planned)
  是否有后端服务: ______（是/否/计划中）
- Server type: ______ (GeneralSpacestation / Custom API / S3/MinIO / None)
  服务端类型: ______（GeneralSpacestation / 自定义 API / S3/MinIO / 无）
- Number of clients: ______ (tens / hundreds / thousands / 10k+)
  客户端数量: ______（几十/几百/几千/万+）
- Client runs 24x7: ______ (Yes/No)
  客户端是否 7×24 运行: ______（是/否）

### Update Requirements / 更新需求
- Need to save bandwidth: ______ (Yes/No → recommend differential)
  是否需要节省带宽: ______（是/否 → 推荐差分）
- Need to skip intermediate versions: ______ (Yes/No → recommend CVP)
  是否需要跳过中间版本: ______（是/否 → 推荐 CVP）
- Need server-initiated push: ______ (Yes/No → recommend SignalR)
  是否需要服务端主动触发: ______（是/否 → 推荐 SignalR）
- Need user-transparent updates: ______ (Yes/No → recommend silent)
  是否需要用户无感知: ______（是/否 → 推荐静默）
- Need to show update progress: ______ (Yes/No → recommend standard + UI)
  是否需要显示更新进度: ______（是/否 → 推荐标准 + UI）

### Constraints / 约束条件
- Target platform: ______ (Windows/Linux/macOS/Multi-platform)
  目标平台: ______（Windows/Linux/macOS/多平台）
- Network environment: ______ (Intranet/Internet/Offline)
  网络环境: ______（内网/公网/离线）
- Need crash recovery: ______ (Yes/No → pair with Bowl)
  是否需要崩溃恢复: ______（是/否 → 配合 Bowl）
```

---

## Strategy Decision Tree (Detailed) / 策略决策树（详细版）

```
Does your app have a backend server? / 你的应用有后端服务吗？
├── Yes / 有
│   ├── Need server to actively push updates? / 需要服务端主动推送更新？
│   │   └── YES → ⑥ SignalR Push (requires extra SignalR Hub deployment)
│   │          ⑥ SignalR 推送（需额外部署 SignalR Hub）
│   └── NO / 不需要
│       ├── Need to save download bandwidth? / 需要节省下载带宽？
│       │   ├── YES → ④ Differential Update (generates patches, reduces size by 60-90%)
│       │   │       ④ 差分更新（生成补丁包，减少 60-90% 体积）
│       │   └── NO / 不需要
│       │       ├── Need to skip intermediate versions and jump to latest? / 需要跳过中间版本直达最新？
│       │       │   ├── YES → ⑤ Cross-Version CVP (requires extra server-side build)
│       │       │   │       ⑤ 跨版本 CVP（需服务端额外构建）
│       │       │   └── NO / 不需要
│       │       │       └── ① Standard Client-Server (recommended for beginners)
│       │       │              ① 标准客户端-服务端（推荐新手入门）
│       └── Need silent background upgrade? / 需要后台无声升级？
│           └── YES → ③ Silent Update (based on standard or OSS + polling)
│                    ③ 静默更新（基于标准或 OSS + 定时轮询）
│
└── No (only object storage S3/MinIO) / 没有（只有对象存储 S3/MinIO）
    ├── Need to save bandwidth? / 需要节省带宽？
    │   ├── YES → ④ Differential Update (OSS + diff patches, limited support in v10.4.6)
    │   │       ④ 差分更新（OSS + 差分补丁，v10.4.6 支持有限）
    │   └── NO / 不需要
    │       └── ② OSS Standard (lowest cost, zero server)
    │              ② OSS 标准（最低成本，零服务端）
    │
    └── Need silent background upgrade? / 需要后台无声升级？
        └── YES → ③ Silent Update (OSS + periodic check)
                    ③ 静默更新（OSS + 定时检查）

### Hybrid Strategy Combinations / 混合策略组合

Common combinations / 常见组合方案：
| Scenario / 场景 | Strategy Combo / 策略组合 | Description / 说明 |
|------|---------|------|
| Standard Web App / 标准 Web 应用 | ① Standard + 🎨 UI | Has backend, shows progress / 有后端，显示进度 |
| No server, save bandwidth / 无服务端节省带宽 | ② OSS + ④ Differential | Zero server + incremental updates / 零服务端 + 增量更新 |
| Long-running background service / 长期运行后台服务 | ③ Silent (based on ① or ②) | User-transparent / 用户无感知 |
| Forced upgrade / 强制升级 | ⑤ CVP + ⑥ SignalR | Skip old versions, push actively / 跳过旧版本，主动推送 |
| Enterprise high-reliability / 企业级高可靠 | ① Standard + Bowl + ③ Silent | Complete chain / 完整链路 |
```

---

## 6 Strategies Detailed Comparison / 6 种策略详细对比

| Strategy / 策略 | Server / 服务端 | Description / 说明 |
|------|:------:|------|
| **① Standard Client-Server / 标准客户端-服务端** | ✅ GeneralSpacestation | Medium-large apps with backend (recommended starter) / 有后端的中大型应用（推荐入门） |
| **② OSS Object Storage / OSS 对象存储** | ❌ S3/MinIO only / 仅 S3/MinIO | No backend, lowest cost / 无后端，最低成本 |
| **③ Silent Update / 静默更新** | ✅ Same as ① or ② / 同①或② | Silent background upgrade / 后台无声升级 |
| **④ Differential Update / 差分更新** | ✅ Needs diff build / 需差分构建 | Incremental patches save bandwidth / 增量补丁节省带宽 |
| **⑤ Cross-Version CVP / 跨版本 CVP** | ✅ Needs CVP build / 需 CVP 构建 | Skip intermediate versions / 跳过中间版本直跳 |
| **⑥ SignalR Push / SignalR 推送** | ✅ Needs SignalR Hub | Server-initiated push / 服务端主动推送 |

---

## Integration Code / 集成代码

All strategies use the same configuration pattern:
所有策略使用相同的配置模式：

```csharp
using GeneralUpdate.Core;
using GeneralUpdate.Core.Configuration;

var config = new UpdateRequest
{
    UpdateUrl = "https://your-server.com/api",
    AppSecretKey = "your-secret-key",
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

Or use the zero-config `SetSource()` API / 或使用零配置 `SetSource()` API：

```csharp
await new GeneralUpdateBootstrap()
    .SetSource(
        updateUrl: "https://your-server.com/api",
        appSecretKey: "your-secret-key")
    .AddListenerUpdateInfo(...)
    .LaunchAsync();
```

See the strategy files under `examples/` for specific examples.
具体示例参见 `examples/` 目录下的策略文件。

---

## Platform-Specific Differences / 平台特定差异

| Platform / 平台 | Features / 特性 |
|------|------|
| **Windows** | Full functionality / 完整功能 |
| **Linux** | Partial functionality (no Bowl) / 部分功能（无 Bowl） |
| **macOS** | Same as Linux / 同 Linux |

---

## Known Issues / 已知问题

| # | Issue / 问题 | Workaround / 规避方案 |
|---|------|---------|
| 1 | OSS mode does not distinguish Main/Upgrade updates / OSS 模式不区分 Main/Upgrade 更新 | Accept this behavior / 接受此行为 |
| 2 | UpgradeApp.exe must be placed in update/ subdirectory / UpgradeApp.exe 必须放在 update/ 子目录 | Deploy per spec / 按规范部署 |
| 3 | NuGet version conflict causes "Method not found" / NuGet 版本冲突导致 "Method not found" | Use same version number for Client and Upgrade / Client 和 Upgrade 使用相同版本号 |
| 4 | Infinite upgrade loop / 无限升级循环 | Ensure manifest.json version number is correct / 确保 manifest.json 版本号正确 |
| 5 | Crash on reconnect after SignalR HubConnection Dispose / SignalR HubConnection Dispose 后重连崩溃 | Set connection to null on Dispose / Dispose 时将连接置 null |

---

## Strategy Selection Verification Checklist / 策略选择验证清单

### Strategy Fit / 策略匹配度
- [ ] Selected strategy matches deployment environment (has backend→Standard / no backend→OSS)
      选定的策略与部署环境匹配（有后端→标准/无后端→OSS）
- [ ] Bandwidth needs match strategy (large files→Differential, many versions→CVP)
      带宽需求与策略匹配（大文件→差分，版本多→CVP）
- [ ] UX goals match strategy (interactive→Standard+UI, background→Silent)
      用户体验目标与策略匹配（需要交互→标准+UI，后台→静默）
- [ ] Platform compatibility confirmed (Linux/macOS do not support Bowl)
      平台兼容性确认（Linux/macOS 不支持 Bowl）

### OSS Strategy / OSS 策略
- [ ] Bucket permission set to private / Bucket 权限设置为私有
- [ ] Update package URL publicly accessible or use pre-signed URL / 更新包的 URL 可公开访问或使用预签名 URL
- [ ] Upgrade.exe placed in `update/` subdirectory (OSS-specific requirement) / Upgrade.exe 放在 `update/` 子目录（OSS 特有要求）
- [ ] No separate Main/Upgrade update packages (OSS limitation, accept) / 没有区分 Main/Upgrade 独立更新包（OSS 限制，接受）

### Silent Strategy / 静默策略
- [ ] Polling interval is reasonable (30-60 min recommended; too short drains battery/bandwidth)
      轮询间隔合理（建议 30-60 分钟，太短耗电/流量）
- [ ] System notification or tray icon hint for "new version available"
      有"新版本可用"的系统通知或托盘图标提示
- [ ] Notify user to restart after download completes, not before
      下载完成后再通知用户重启，而非下载前
- [ ] Background download has data/battery optimization (download large packages over WiFi only)
      后台下载有流量/电量优化（WiFi 下才下载大包）

### SignalR Push / SignalR 推送
- [ ] HubConnection lifecycle management is complete / HubConnection 的生命周期管理完善
- [ ] Reconnect logic (auto-retry 3 times with increasing intervals) / 重连逻辑（自动重试 3 次，间隔递增）
- [ ] Set HubConnection to null on Dispose (otherwise reconnect crashes) / Dispose 时将 HubConnection 置 null（否则重连崩溃）
- [ ] Push messages have timeout protection and fallback strategy (push fails → fallback to polling)
      推送消息有超时保护和降级策略（推送失败→回退到轮询）

### Differential Strategy / 差分策略
- [ ] Server has diff package generation mechanism (`DifferentialCore.CleanAsync`) / 服务端有差分包生成机制（`DifferentialCore.CleanAsync`）
- [ ] Client Pipeline has PatchMiddleware configured / 客户端 Pipeline 配置了 PatchMiddleware
- [ ] Watch out for integer overflow on large file diffs (fixed in v10.4.6 #514) / 注意大文件差分可能触发的整数溢出（v10.4.6 已修复 #514）
- [ ] BSDIFF patch compatibility verified on Linux/macOS / Linux/macOS 上 BSDIFF 补丁兼容性已验证

---

## Anti-Pattern Checklist / 反模式清单

| # | Anti-Pattern / 反模式 | Consequence / 后果 | Correct Approach / 正确做法 |
|---|--------|------|---------|
| 1 | **Choosing OSS when you have a backend / 有后端却选 OSS** | Wastes backend service capability, loses version management / 浪费后端服务能力，失去版本管理 | Has backend → Standard strategy / 有后端 → 标准策略 |
| 2 | **Low-frequency polling (once per day) / 低频轮询（每天 1 次）** | Users wait a long time for updates / 用户等很久才收到更新 | Silent mode 30-60 min polling / 静默模式 30-60 分钟轮询 |
| 3 | **High-frequency polling (once per minute) / 高频轮询（每分钟 1 次）** | Wastes bandwidth and battery / 浪费带宽和电池 | Silent mode recommends >= 30 min / 静默模式建议 ≥ 30 分钟 |
| 4 | **SignalR connection never released / SignalR 连接永不释放** | Memory leak / 内存泄漏 | Dispose HubConnection on page/app close / 页面/应用关闭时 Dispose HubConnection |
| 5 | **Diff patch too large (> 2GB) / 差分包太大（> 2GB）** | Integer overflow crashes process (BSD-514) / 整数溢出导致进程崩溃（BSD-514） | Release in multiple versions or use full package / 分多个版本发布，或用全量包 |
| 6 | **CVP skip versions without testing intermediate API changes / CVP 跳版本不测试中间版本 API 变更** | Client data migration fails / 客户端数据迁移失败 | Test version compatibility on the server side / 在服务端做好版本兼容测试 |
| 7 | **OSS package name does not include version number / OSS 包名不包含版本号** | Client version comparison logic breaks / 客户端版本比较逻辑异常 | Name in `MyApp_1.0.0.0.zip` format / `MyApp_1.0.0.0.zip` 格式命名 |
| 8 | **Not notifying user to restart after silent update / 静默更新后不通知用户重启** | User does not know new version is downloaded / 用户不知道新版本已下载 | Notify after download completes + delayed restart option / 下载完成后通知 + 延迟重启选项 |

---

## Related Skills / 相关技能

- `/generalupdate-init` — If Bootstrap is not yet configured / 如果还未配置 Bootstrap
- `/generalupdate-ui` — If you need an update UI / 如果需要更新界面
- `/generalupdate-troubleshoot` — If you encounter issues / 如果遇到问题
- `/generalupdate-advanced` — Advanced customization / 高级定制
