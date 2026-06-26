---
name: generalupdate-migration
description: |
  Guide developers through migrating GeneralUpdate from older versions to the
  latest stable API (v10.4.6). Covers v9.x → v10 and dev-branch (v10.5.0-beta.x)
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

# GeneralUpdate Migration Guide / GeneralUpdate 迁移指南

Helps developers migrate from older versions of GeneralUpdate to the latest stable API (v10.4.6).

帮助开发者从旧版本 GeneralUpdate 迁移到最新稳定版 API（v10.4.6）。

> **Target version: NuGet v10.4.6 stable**
> The dev-branch (v10.5.0-beta.x) API is fundamentally different from the stable release.
> **目标版本：NuGet v10.4.6 稳定版**
> 开发分支（v10.5.0-beta.2）API 与稳定版有根本性差异。

---

## Pre-Migration Information Gathering / 迁移前需求提取

Collect the following information before recommending a migration path. Accurate version information is critical — guessing the version leads to wrong migration advice.

在推荐迁移路径前收集以下信息。准确的版本信息至关重要 — 猜测版本会导致错误的迁移建议。

```
### Current State / 当前状态

- Current GeneralUpdate version / 当前 GeneralUpdate 版本: ______
    （v9.x / v10.0-10.3 / v10.5.0-beta.x / Unknown / 不确定）
- Current .NET version / 当前 .NET 版本: ______
- UI framework / UI 框架: ______
- Using Bowl? / 是否使用了 Bowl: ______（Yes/No / 是/否）
- Using Differential? / 是否使用了 Differential: ______（Yes/No / 是/否）

### Migration Target / 迁移后目标

- Target version / 目标版本: ______（v10.4.6 stable / 稳定版 / keep dev-branch / 继续用开发分支）
- Need new features (Bowl / IPC replacement / AOT)? / 是否需要新的功能（Bowl/IPC 替换/AOT）: ______
```

---

## Migration Paths / 迁移路径

### Path A: v9.x to v10.4.6 Stable / 路径 A：v9.x → v10.4.6 稳定版

This is the largest jump. The architecture between v9.x and v10 is completely different. The migration involves restructuring from a single-process model to a dual-process model, introducing manifest files, AES-encrypted IPC, and a centralized server endpoint.

这是最大的跳跃。v9.x 和 v10 的架构完全不同。迁移涉及从单进程模型重构为双进程模型，引入 manifest 文件、AES 加密 IPC 和集中式服务端。

```
v9.x (Single-process, HttpClient direct connection)
     （单进程, HttpClient 直连）
         ↓
    Breaking Changes:
    ├── Single-process → Dual-process architecture (Client + Upgrade)
    │   单进程 → 双进程架构（Client + Upgrade）
    ├── HttpClient direct → GeneralSpacestation server endpoint
    │   HttpClient 直连 → GeneralSpacestation 服务端
    ├── No IPC → AES-encrypted IPC file
    │   无 IPC → AES 加密 IPC 文件
    ├── No manifest.json → Must carry manifest
    │   无 manifest.json → 必须携带 manifest
    └── All API namespaces renamed
        API 命名空间全部重命名
         ↓
v10.4.6 (Dual-process, Configinfo + Bootstrap)
       （双进程, Configinfo + Bootstrap）
```

**Migration Steps / 迁移步骤：**

```csharp
// v9.x style (no longer exists) / v9.x 写法（不复存在）
// var updater = new GeneralUpdater("https://api/method");
// updater.Start();

// v10.4.6 style / v10.4.6 写法
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

| v9.x API | v10.4.6 Equivalent / v10.4.6 对应 | Notes / 说明 |
|----------|-------------|------|
| `GeneralUpdater` | `GeneralUpdateBootstrap` | Complete rename / 完全重命名 |
| `SetApiUrl()` / `SetMethod()` | `Configinfo.UpdateUrl` | Unified into Configinfo / 统一到 Configinfo |
| `CheckUpdateAsync()` | `.LaunchAsync()` | Async changed to return Bootstrap instance / 异步改为返回 Bootstrap 实例 |
| Single-process direct update / 单进程直接更新 | Client + Upgrade dual-process / Client + Upgrade 双进程 | Must create a separate Upgrade project / 必须创建独立 Upgrade 项目 |
| N/A | `generalupdate.manifest.json` | Must be included in initial release / 必须随首发版本发布 |

### Path B: v10.5.0-beta.x (Dev-Branch) to v10.4.6 Stable / 路径 B：v10.5.0-beta.x (开发分支) → v10.4.6 稳定版

If you are already using dev-branch APIs (such as `IUpdateHooks`, the `Option` system), rolling back to the stable release requires rewriting. Dev-branch APIs are experimental and have no direct stable equivalents — the table below maps each to a workable alternative.

如果你已经在用开发分支的 API（如 `IUpdateHooks`、`Option` 系统），回退到稳定版需要重写。开发分支 API 是实验性的，在稳定版中没有直接对应 — 下表将每个 API 映射到可行的替代方案。

| Dev-Branch API (v10.5.0-beta.x) | Stable Alternative (v10.4.6) | Handling / 处理方式 |
|-------------------------------|---------------------|---------|
| `new Option()` / `SetOption()` | Does not exist / 不存在 | Use `Configinfo` properties directly instead / 改用 `Configinfo` 属性直接设置 |
| `.Hooks<T>()` / `IUpdateHooks` | Does not exist / 不存在 | Remove Hooks references; implement equivalent logic in event listeners / 去除 Hooks 引用；在事件监听中做等价逻辑 |
| `.Strategy<T>()` / `IStrategy` | Does not exist / 不存在 | Use built-in strategies directly; or call `AbstractStrategy` manually / 直接用内置策略；或手动调用 `AbstractStrategy` |
| `SilentPollOrchestrator` | Does not exist / 不存在 | Manually implement timer + call Bootstrap / 手动实现定时器 + 调用 Bootstrap |
| `ISslValidationPolicy` | Does not exist / 不存在 | Configure at `HttpClientHandler` level / 在 `HttpClientHandler` 层级配置 |
| `IProcessInfoProvider` / `ProcessContract` | Does not exist / 不存在 | Accept default encrypted-file IPC; not replaceable / 接受默认加密文件 IPC；无法替换 |
| `OssClient (AppType=3,4)` | Does not exist / 不存在 | Only use AppType=1 (Client) and 2 (Upgrade) / 只使用 AppType=1(Client) 和 2(Upgrade) |
| Hardcoded version / 硬编码版本号 | `Configinfo.ClientVersion` | Prefer `Assembly.GetEntryAssembly()?.GetName()?.Version` / 建议使用 `Assembly.GetEntryAssembly()?.GetName()?.Version` |

---

## Migration Verification Checklist / 迁移验证清单

### Compilation Verification / 编译验证

- [ ] `dotnet build` succeeds with no errors / `dotnet build` 无错误
- [ ] No risk of `MissingMethodException` (verify all method names exist in v10.4.6)
      无 `MissingMethodException` 的风险（检查所有方法名是否存在于 v10.4.6）
- [ ] No `CS0433` type conflicts (in v10.5.0-rc.1, Core + Bowl can coexist without conflict)
      无 `CS0433` 类型冲突（v10.5.0-rc.1 中 Core + Bowl 无冲突，可同时引用）

### Architecture Verification / 架构验证

- [ ] Project is split into two independent projects: Client + Upgrade
      项目已拆分为 Client + Upgrade 两个独立项目
- [ ] Upgrade project has `AppType = 2`
      Upgrade 项目 `AppType = 2`
- [ ] Client project has `AppType = 1`
      Client 项目 `AppType = 1`
- [ ] `generalupdate.manifest.json` exists and is correctly configured
      `generalupdate.manifest.json` 存在且配置正确

### Runtime Verification / 运行验证

- [ ] Version check API returns valid responses / 版本检查 API 可正常返回
- [ ] Upgrade process starts correctly after download / 下载后 Upgrade 进程可正常启动
- [ ] Main application restarts correctly after update / 更新完成后主程序可正常重启
- [ ] IPC file encoding is set to `Encoding.UTF8` / IPC 文件编码设为 `Encoding.UTF8`

---

## Migration Anti-Patterns / 迁移反模式

These are the most common mistakes made during migration. Each wastes at least an hour of debugging time — avoid them by following the correct approach column.

以下是迁移过程中最常见的错误。每个都会浪费至少一个小时的调试时间 — 请按照正确做法列来避免。

| # | Anti-Pattern / 反模式 | Consequence / 后果 | Correct Approach / 正确做法 |
|---|--------|------|---------|
| 1 | **Replacing NuGet version in csproj without modifying code**
      **直接在项目中替换 NuGet 版本不修改代码** | Massive compilation errors / 大量编译错误 | Clean up old API references first, then upgrade NuGet
      先清理旧 API 引用再升级 NuGet |
| 2 | **Assuming v9.x config objects are Configinfo**
      **认为 v9.x 的配置对象就是 Configinfo** | Configinfo property names are completely different
      Configinfo 属性名完全不同 | Rewrite Configinfo against documentation
      对照文档重新写 Configinfo |
| 3 | **Attempting to use dev-branch APIs in v10.4.6**
      **试图在 v10.4.6 中使用 dev-branch 的 API** | MissingMethodException / MissingMethodException | Check the API availability table above
      检查 API 可用性表 |
| 4 | **Not testing the Upgrade process after migration**
      **迁移后不测试 Upgrade 进程** | Main app updates but Upgrade crashes / 主程序能更新但 Upgrade 崩溃 | Test both sides / 两端都要测试 |
| 5 | **Keeping old v9.x references without removing them**
      **保留旧的 v9.x 引用不删除** | Type conflicts / 类型冲突 | Clear csproj and re-add references
      清空 csproj 重新添加引用 |

---

## Related Skills / 相关技能

- `/generalupdate-init` — Bootstrap configuration and fresh integration / Bootstrap 配置和全新集成
- `/generalupdate-troubleshoot` — Diagnose migration-related issues / 诊断迁移相关问题
- `/generalupdate-security-audit` — Security audit after migration / 迁移后安全审计
