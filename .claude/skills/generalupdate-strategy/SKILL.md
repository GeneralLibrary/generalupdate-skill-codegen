---
name: generalupdate-strategy
description: |
  Configure GeneralUpdate update strategies for different deployment scenarios.
  Covers 6 strategies: client-server, OSS object storage, silent background polling,
  differential patching, cross-version (CVP) direct jump, and SignalR push.
  Triggers on: "configure update strategy", "OSS update", "silent update",
  "differential update", "cross version update", "push update", "SignalR push",
  "配置更新策略", "OSS方案", "静默更新", "差分更新", "跨版本更新", "推送更新",
  "how to update without a server", "background update", "reduce download size".
  Also triggers when user mentions specific deployment constraints
  (no server / slow network / many versions to skip).
when_to_use: |
  - User asks about different ways to deliver updates
  - User wants silent/background update mode
  - User wants differential (delta) updates to save bandwidth
  - User mentions OSS / S3 / MinIO / object storage
  - User wants cross-version jump (skip intermediate versions)
  - User wants server-push (SignalR) updates
  - User has constraints like no dedicated backend server
  - Best used after generalupdate-init (strategy is the next decision)
allowed-tools: "Read, Write, Edit, Glob"
---

# ⚙️ GeneralUpdate 更新策略配置

帮助开发者在 6 种更新策略中选择最适合其部署场景的方案，并生成对应的配置代码。

## 策略选择矩阵

| 策略 | 是否需要服务端 | 是否需要 UI | 适用场景 | 带宽效率 | 复杂度 |
|------|--------------|-----------|---------|---------|-------|
| **标准客户端-服务端** | ✅ 需要 GeneralSpacestation | 可选 | 有后端服务的中大型应用 | 中 | ⭐⭐ |
| **OSS 对象存储** | ❌ 仅需 S3/MinIO | 可选 | 无后端，使用静态存储 | 中 | ⭐ |
| **静默更新** | ✅ 需要服务端或 OSS | ❌ 无 UI（后台） | 长驻后台应用 | 中 | ⭐⭐⭐ |
| **差分更新** | ✅ 需要服务端+差分构建 | 可选 | 大应用、带宽受限 | 高 | ⭐⭐⭐ |
| **跨版本(CVP)更新** | ✅ 需要服务端支持 | 可选 | 长期未更新的客户端 | 高 | ⭐⭐⭐⭐ |
| **SignalR 推送** | ✅ 需要服务端 +SignalR Hub | 可选 | 需要服务端主动控制的场景 | 中 | ⭐⭐⭐ |

## 策略详解

### 1. 标准客户端-服务端（推荐入门）

通用架构，适合大多数场景。

```
Client → HTTP POST /Upgrade/Verification → Server 返回包列表
       → 下载所有包 → IPC → Upgrade 进程 → 应用更新 → 重启
```

**配置参考** `examples/ClientServerStrategy.cs`

### 2. OSS 对象存储（无需服务器）

适合没有后端服务、使用云存储的场景，成本最低。

```
Client → 下载 versions.json (从 S3/MinIO/阿里云OSS)
       → 比较版本 → 下载更新包 → 应用更新
```

**配置参考** `examples/OssStrategy.cs`

### 3. 静默更新（后台无声升级）

适合用户长时间不关闭的应用。后台轮询下载，在应用退出时自动更新。

```
App启动 → SilentPollOrchestrator 后台定时轮询
       → 发现更新 → 后台下载 + IPC
       → 应用退出 → 触发 Upgrade 进程更新
```

**配置参考** `examples/SilentStrategy.cs`

**⚠️ 注意事项**（来自真实 Issue #484、#471）：
- `ProcessExit` 事件在 `FailFast`、`TerminateProcess`、Ctrl+C 时不会触发
- 建议在应用关闭时显式调用 `bootstrap.SilentOrchestrator?.TryLaunchUpgrade()`
- 静默模式需要 manifest.json 正确配置，否则默认字段会阻塞版本探测

### 4. 差分更新（增量补丁）

只传输两个版本之间的差异部分，大幅减少下载量。

```
服务端: 全量包 → DiffPipeline.CleanAsync() → 生成 .patch 文件
客户端: 下载 .patch → DiffPipeline.DirtyAsync() → 应用补丁还原新文件
```

**配置参考** `examples/DifferentialStrategy.cs`

### 5. 跨版本更新 / CVP（直跳升级）

适用于长期未手动更新的客户端，跳过中间所有版本直接升级到最新。

```
服务端: 全量包v1 + 全量包v3 → CrossVersionPacketBuilder → CVP差分包
客户端: 下载一个 CVP 包 → 一次应用 → 从 v1 到 v3
```

**配置参考** `examples/CrossVersionStrategy.cs`

### 6. SignalR 推送更新

服务端通过 SignalR Hub 主动推送更新通知给客户端。

```
服务端(管理员操作) → SignalR Hub → 客户端接收推送
       → 触发 ClientStrategy 开始更新
```

**配置参考** `examples/PushStrategy.cs`

## 策略组合建议

| 场景 | 推荐组合 |
|------|---------|
| 企业内部应用（有IT基础设施） | 标准 + 差分 + CVP |
| 开源/免费桌面应用（无预算） | OSS + 静默 |
| 游戏客户端 | OSS + 全量包（避免差分损坏） |
| 关键业务系统（需要可控） | 标准 + SignalR推送 + Bowl崩溃守护 |
| 移动端 | OSS + 差分 |

## 输出

根据用户选择的策略，输出：
- ✅ 策略适用的配置代码
- ✅ 必要的后端/存储基础设施说明
- ✅ 已知问题和规避方案（基于真实 Issue）
- ✅ 可选的 UI 集成（引导到 `generalupdate-ui`）
