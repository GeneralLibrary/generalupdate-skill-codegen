---
name: generalupdate-migration
description: |
  Guide developers through migrating GeneralUpdate from older versions to the
  latest stable API (v10.4.6). Covers v9.x → v10 and dev-branch (v10.5.0-beta.2)
  → stable (v10.4.6) migration paths. Detects breaking API changes, deprecated
  types, and provides automated migration scripts.
  Triggers on: "migrate", "migration", "upgrade from v9", "upgrade from v10.5",
  "迁移", "旧版本升级", "API 变更", "breaking changes", "不再兼容",
  "v10.4.6", "v10.5.0", "开发分支", "稳定版迁移",
  "IUpdateHooks not found", "SetSource not found", "OssClient missing",
  "ProcessContract missing", "Option system".
when_to_use: |
  - User has an existing GeneralUpdate integration and wants to upgrade to latest
  - User reports compilation errors after updating NuGet package
  - User's code uses v10.5.0+ dev-branch APIs (IUpdateHooks, SetSource, etc.)
  - User is on v9.x and needs to migrate to the dual-process architecture
  - User sees "missing method" or "type not found" after package update
  - User asks about API compatibility between versions
  - Run AFTER generalupdate-init if migration is the primary goal
allowed-tools: "Read, Write, Edit, Glob, Grep, Bash"
---

# 🔄 GeneralUpdate 迁移指南

帮助开发者从旧版本 GeneralUpdate 迁移到最新稳定版 API（v10.4.6）。

> ⚠️ **目标版本：NuGet v10.4.6 稳定版**
> 开发分支（v10.5.0-beta.2）API 与稳定版有根本性差异。

---

## 📋 迁移前需求提取

```
### 当前状态
- 当前 GeneralUpdate 版本: ______（v9.x / v10.0-10.3 / v10.5.0-beta.x / 不确定）
- 当前 .NET 版本: ______
- UI 框架: ______
- 是否使用了 Bowl: ______（是/否）
- 是否使用了 Differential: ______（是/否）

### 迁移后目标
- 目标版本: ______（v10.4.6 稳定版 / 继续用开发分支）
- 是否需要新的功能（Bowl/IPC 替换/AOT）: ______
```

---

## 迁移路径

### 路径 A：v9.x → v10.4.6 稳定版

这是最大的跳跃。v9.x 和 v10 的架构完全不同。

```
v9.x (单进程, HttpClient 直连)
         ↓
    Breaking Changes:
    ├── 单进程 → 双进程架构（Client + Upgrade）
    ├── HttpClient 直连 → GeneralSpacestation 服务端
    ├── 无 IPC → AES 加密 IPC 文件
    ├── 无 manifest.json → 必须携带 manifest
    └── API 命名空间全部重命名
         ↓
v10.4.6 (双进程, Configinfo + Bootstrap)
```

**迁移步骤：**

```csharp
// ❌ v9.x 写法（不复存在）
// var updater = new GeneralUpdater("https://api/method");
// updater.Start();

// ✅ v10.4.6 写法
await new GeneralUpdateBootstrap()
    .SetConfig(new Configinfo
    {
        UpdateUrl = "https://your-server.com/Upgrade/Verification",
        AppSecretKey = "your-secret-key",
        AppName = "MyApp.exe",
        MainAppName = "MyApp.exe",
        ClientVersion = "1.0.0.0",
        ProductId = "my-product-001",
        InstallPath = "."
    })
    .LaunchAsync();
```

| v9.x API | v10.4.6 对应 | 说明 |
|----------|-------------|------|
| `GeneralUpdater` | `GeneralUpdateBootstrap` | 完全重命名 |
| `SetApiUrl()` / `SetMethod()` | `Configinfo.UpdateUrl` | 统一到 Configinfo |
| `CheckUpdateAsync()` | `.LaunchAsync()` | 异步改为返回 Bootstrap 实例 |
| 单进程直接更新 | Client + Upgrade 双进程 | 必须创建独立 Upgrade 项目 |
| N/A | `generalupdate.manifest.json` | 必须随首发版本发布 |

### 路径 B：v10.5.0-beta.x (开发分支) → v10.4.6 稳定版

如果你已经在用开发分支的 API（如 `IUpdateHooks`、`Option` 系统），回退到稳定版需要重写：

| 开发分支 API (v10.5.0-beta.x) | 稳定版替代 (v10.4.6) | 处理方式 |
|-------------------------------|---------------------|---------|
| `new Option()` / `SetOption()` | 不存在 | 改用 `Configinfo` 属性直接设置 |
| `.Hooks<T>()` / `IUpdateHooks` | 不存在 | 去除 Hooks 引用；在事件监听中做等价逻辑 |
| `.Strategy<T>()` / `IStrategy` | 不存在 | 直接用内置策略；或手动调用 `AbstractStrategy` |
| `SilentPollOrchestrator` | 不存在 | 手动实现定时器 + 调用 Bootstrap |
| `ISslValidationPolicy` | 不存在 | 在 `HttpClientHandler` 层级配置 |
| `IProcessInfoProvider` / `ProcessContract` | 不存在 | 接受默认加密文件 IPC；无法替换 |
| `OssClient (AppType=3,4)` | 不存在 | 只使用 AppType=1(Client) 和 2(Upgrade) |
| 硬编码版本号 | `Configinfo.ClientVersion` | 建议使用 `Assembly.GetEntryAssembly()?.GetName()?.Version` |

---

## 迁移验证清单

### 编译验证
- [ ] `dotnet build` 无错误
- [ ] 无 `MissingMethodException` 的风险（检查所有方法名是否存在于 v10.4.6）
- [ ] 无 `CS0433` 类型冲突（v10.5.0-beta.4 中 Core + Bowl 无冲突，可同时引用）

### 架构验证
- [ ] 项目已拆分为 Client + Upgrade 两个独立项目
- [ ] Upgrade 项目 `AppType = 2`
- [ ] Client 项目 `AppType = 1`
- [ ] `generalupdate.manifest.json` 存在且配置正确

### 运行验证
- [ ] 版本检查 API 可正常返回
- [ ] 下载后 Upgrade 进程可正常启动
- [ ] 更新完成后主程序可正常重启
- [ ] IPC 文件编码设为 `Encoding.UTF8`

---

## ⚠️ 迁移反模式

| # | 反模式 | 后果 | 正确做法 |
|---|--------|------|---------|
| 1 | **直接在项目中替换 NuGet 版本不修改代码** | 大量编译错误 | 先清理旧 API 引用再升级 NuGet |
| 2 | **认为 v9.x 的配置对象就是 Configinfo** | Configinfo 属性名完全不同 | 对照文档重新写 Configinfo |
| 3 | **试图在 v10.4.6 中使用 dev-branch 的 API** | MissingMethodException | 检查 API 可用性表 |
| 4 | **迁移后不测试 Upgrade 进程** | 主程序能更新但 Upgrade 崩溃 | 两端都要测试 |
| 5 | **保留旧的 v9.x 引用不删除** | 类型冲突 | 清空 csproj 重新添加引用 |
