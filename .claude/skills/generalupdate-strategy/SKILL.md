---
name: generalupdate-strategy
description: |
  Configure GeneralUpdate update strategies for any deployment scenario.
  Covers 6 strategies with decision tree, mixed/cascading strategies, 4 Client-Upgrade
  update scenes, CVP+chain fallback, and platform-specific considerations (Windows/Linux/Mac).
  Each strategy includes real issue workarounds from GitHub/Gitee.
  Triggers on: "configure strategy", "OSS update", "silent update", "differential update",
  "cross version update", "push update", "SignalR push", "更新策略", "OSS方案",
  "静默更新", "差分更新", "跨版本", "推送更新", "决策", "选策略",
  "how to update without a server", "background update", "reduce download size",
  "skip versions", "force update", "cascading update", "CVP", "chain packages".
  Also triggers when user mentions specific deployment constraints.
when_to_use: |
  - User asks about different ways to deliver updates (strategy selection)
  - User wants silent/background mode for long-running apps
  - User wants differential/delta updates to save bandwidth
  - User mentions OSS / S3 / MinIO / object storage (no server)
  - User wants cross-version jump (skip intermediate versions)
  - User wants server-push (SignalR) updates
  - User is confused about which strategy fits their use case
  - User asks about chain vs CVP, or combining strategies
  - Best used after generalupdate-init; pair with generalupdate-ui if UI is needed
allowed-tools: "Read, Write, Edit, Glob"
---

# ⚙️ GeneralUpdate 更新策略完全指南

帮助开发者在 6 种更新策略 + 混合方案中做出正确选择，覆盖所有部署场景。

---

## 策略决策树

帮助用户快速选择最适合的策略：

```
你的应用有后端服务吗？
├── 有
│   ├── 需要服务端主动推送更新？
│   │   ├── 是 → SignalR 推送
│   │   └── 否
│   │       ├── 用户长期不关闭应用？
│   │       │   ├── 是 → 静默更新
│   │       │   └── 否
│   │       │       ├── 应用体积大 (>100MB)？
│   │       │       │   ├── 是 → 差分更新
│   │       │       │   └── 否 → 标准客户端-服务端
│   │       └── 可以跳过中间版本？
│   │           └── 是 → 跨版本 (CVP) + 链式兜底
│   └── 否
│       └── 需要全量还是差分？
│           ├── 差分 → 差分更新（服务端构建差分包）
│           └── 全量 → 标准客户端-服务端
│
└── 没有（只有对象存储 S3/MinIO/OSS）
    ├── 用户长期不关闭应用？
    │   ├── 是 → OSS + 静默
    │   └── 否 → OSS 标准
    │
    └── 需要跨版本？
        └── 只能使用全量包（OSS 不支持 CVP 自动构建）
```

---

## 6 种策略详细对比

| 策略 | 服务端 | UI | 带宽效率 | 安装复杂度 | 适用场景 |
|------|:------:|:--:|:--------:|:----------:|---------|
| **① 标准客户端-服务端** | ✅ GeneralSpacestation | 可选 | 中 | ⭐⭐ | 有后端的中大型应用（推荐入门） |
| **② OSS 对象存储** | ❌ 仅 S3/MinIO | 可选 | 中 | ⭐ | 无后端，最低成本 |
| **③ 静默更新** | ✅ 同①或② | ❌ 无UI | 中 | ⭐⭐⭐ | 长驻后台、不打扰用户 |
| **④ 差分更新** | ✅ 需差分构建 | 可选 | **高** (节省60-90%) | ⭐⭐⭐ | 大应用、带宽受限 |
| **⑤ 跨版本 CVP** | ✅ 需 CVP 构建 | 可选 | 高 (一次到位) | ⭐⭐⭐⭐ | 长期未更新的客户端 |
| **⑥ SignalR 推送** | ✅ 需 SignalR Hub | 可选 | 中 | ⭐⭐⭐ | 服务端主动控制更新时机 |

---

## 策略详解 + 完整代码

### ① 标准客户端-服务端（推荐入门）

**工作流程**：
```
Client → POST /Upgrade/Verification → Server 返回包列表
       → 下载所有包 → IPC → Upgrade 进程 → 应用更新 → 重启
```

**服务端接口**：
- `POST /Upgrade/Verification` — 版本验证（必需）
- `POST /Upgrade/Report` — 状态上报（可选）

**完整配置**见 `examples/ClientServerStrategy.cs`

**⚠️ 已知问题**：
- 场景判断可能错位（#475）：确保服务端 `AppType` 字段与客户端匹配
- 版本比较使用 `System.Version`，需保证 4 段式版本号

---

### ② OSS 对象存储（无后端最低成本）

**工作流程**：
```
Client → 下载 versions.json（从 S3/MinIO/阿里云OSS 的固定 URL）
       → 解析版本列表 → 比较版本 → 有更新？下载 ZIP → 应用更新
```

**OSS 上需要的文件**：
```
your-bucket/
├── versions.json           ← 版本清单（由 CI/CD 脚本生成）
├── v1.1.0.0/
│   └── update.zip
└── v1.2.0.0/
    └── update.zip
```

**versions.json 格式**：
```json
{
  "versions": [
    {
      "version": "1.1.0.0",
      "url": "https://your-bucket/v1.1.0.0/update.zip",
      "hash": "sha256-of-zip",
      "size": 1048576
    },
    {
      "version": "1.2.0.0",
      "url": "https://your-bucket/v1.2.0.0/update.zip",
      "hash": "sha256-of-zip",
      "size": 2097152
    }
  ]
}
```

**完整配置**见 `examples/OssStrategy.cs`

**⚠️ 已知问题**（#485、#487）：
1. OSS 模式不区分 Main/Upgrade 更新（两者 `HasUpdate` 总是相同值）
2. SSL 验证不覆盖文件下载请求 → 需显式设置 `SslValidationPolicy`
3. Upgrade.exe 必须放在 `update/` 子目录中（#485）

---

### ③ 静默更新（后台无声升级）

**工作流程**：
```
App 启动 → SilentPollOrchestrator 后台定时轮询
        → 发现更新 → 后台下载 + 写入 IPC 文件
        → 用户关闭应用 → ProcessExit → 触发 Upgrade 进程
        → 更新文件 → 下次启动是新版本
```

**完整配置**见 `examples/SilentStrategy.cs`

**⚠️ 已知问题（来自 Issue #484, #471, #443, #IJQ0Q5）**：

| # | 问题 | 根因 | 规避 |
|---|------|------|------|
| 1 | **ProcessExit 不触发**（#484） | FailFast/TerminateProcess/Ctrl+C 不会触发 | 应用关闭时显式调用 `TryLaunchUpgrade()` |
| 2 | **PatchMiddleware 抛异常**（#471） | 静默模式未注入 DiffPipeline | 显式设置 `PatchEnabled = false` |
| 3 | **manifest 阻塞版本发现**（#484） | 默认非空字段使字段不被覆盖 | manifest 中字段确保正确 |
| 4 | **更新完自动启动应用**（#443） | 静默模式默认行为 | 配置 `SilentAutoRestart = false` |

---

### ④ 差分更新（增量补丁节省带宽）

**工作流程**：
```
服务端: 全量包 → DiffPipeline.CleanAsync()
       → 新旧版本对比 → 生成 .patch 文件 + 新增文件 + 删除清单
客户端: 下载差分包 → Hash校验 → 解压 → 应用补丁(DirtyAsync)

补丁类型（来自 Issue #II75WI）：
- 修改的文件 → .patch （二进制差分，通常占原始文件 5-20%）
- 新增的文件 → 完整文件
- 删除的文件 → generalupdate.delete.json 清单
```

**完整配置**见 `examples/DifferentialStrategy.cs`

**⚠️ 已知问题**：
1. **同名文件不同目录封包出错**（#II77NS）— DefaultCleanMatcher 缺少相对路径匹配
2. **旧 patch 临时文件残留**（#I8T0QX）— 必须在每次更新前清理 TempPath
3. **文件被上次部分更新修改导致 hash 不匹配**（#II75WI）— 建议对 patch 增加旧文件 hash 判断
4. **多级文件夹结构错乱**（#I59QRI）— 差分包需保持完整相对路径

---

### ⑤ 跨版本 CVP 直跳

**工作流程**：
```
服务端:
  全量包 v1 + 全量包 v3 → CrossVersionPacketBuilder
  → DiffPipeline.CleanAsync(v1, v3) → CVP 差分包
  → 写入 TbPacket(IsCrossVersion=true)
  → 客户端请求时优先返回 CVP 包

客户端:
  Verify → 服务端返回 CVP 包 + 链式兜底包
  → 尝试 CVP 包（一次 patch 操作）
  → 失败？自动退化链式重试（无需二次请求，#499）
```

**完整配置**见 `examples/CrossVersionStrategy.cs`

**⚠️ 注意事项**：
- 需要 GeneralSpacestation 服务端支持（Redis 队列 + BackgroundWorker）
- 构建材料是两个版本的全量包归档（`TbVersionArchive`）
- 如果 CVP 构建未完成，服务端返回链式包列表

---

### ⑥ SignalR 推送更新

**工作流程**：
```
管理员在后台上传包 → 点击"推送更新" → SignalR Hub → 所有在线客户端
                     → 客户端 UpgradeHubService 接收推送
                     → 触发 GeneralUpdateBootstrap.LaunchAsync()
```

**完整配置**见 `examples/PushStrategy.cs`

**⚠️ 已知问题**：
1. HubConnection.Dispose 后不置 null，重连时 `ObjectDisposedException`（#5 代码审计）
2. 使用 `SafeHubConnection` 包装类（示例代码中提供）

---

## 混合策略（Mixing Strategies）

对于复杂场景，可以组合多种策略：

| 组合 | 场景 | 实现方式 |
|------|------|---------|
| OSS + 静默 | 无服务器 + 长驻后台 | `OssClient` + `Silent=true` |
| 标准 + 差分 | 大应用+有服务器 | `PatchEnabled=true` + 服务端差分构建 |
| CVP + 链式兜底 | 需跳过版本+确保可回退 | 服务端同时返回 CVP 和链式包，客户端自动降级（#499） |
| 推送 + 静默 | 服务端主动+后台下载 | SignalR 收到推送后触发 Bootstrap（非静默轮询） |
| 标准 + Bowl | 需崩溃保护 | `StartAppAsync` 可自动启动 Bowl 进程 |

---

## 平台特定策略行为

| 平台 | 策略管道 | Bowl | 差异 |
|------|---------|------|------|
| **Windows** | Hash → 解压 → Patch (可选) + Drivelution (可选) | ✅ 支持 | 完整功能 |
| **Linux** | Hash → 解压 → Patch (可选) | ❌ 不支持 | 无 Bowl，需 UnixPermissionHooks |
| **macOS** | Hash → 解压 → Patch (可选) | ❌ 不支持 | 同 Linux |

---

## 输出

根据用户场景，输出：
- ✅ 对应策略的完整配置代码 + Option 设置
- ✅ 必要的后端/存储基础设施说明
- ✅ 已知问题 + 规避方案（基于真实 Issue）
- ✅ 可选的 UI 集成（引导到 generalupdate-ui）
- ✅ 混合策略组合建议（如果场景复杂）
