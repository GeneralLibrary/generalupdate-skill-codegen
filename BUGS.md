# GeneralUpdate Skill CodeGen — 审计报告

> 审计日期：2026-06-16
> **当前状态：全部模板已针对 NuGet v10.5.0-rc.1 更新，已通过 dotnet build 编译验证（0 errors）**

---

## 版本升级：v10.4.6 稳定版 → v10.5.0-rc.1

套件从 NuGet v10.4.6 稳定版升级到 v10.5.0-rc.1。主要变更：

| 功能 | v10.4.6 (旧) | v10.5.0-rc.1 (新) |
|------|-------------|---------------------|
| 配置对象 | `Configinfo` (已移除) | `UpdateRequest` (新) |
| 配置命名空间 | `GeneralUpdate.Common.Shared.Object` | `GeneralUpdate.Core.Configuration` |
| 事件参数命名空间 | `GeneralUpdate.Common.Download` | `GeneralUpdate.Core.Download` |
| 异常事件命名空间 | `GeneralUpdate.Common.Internal` | `GeneralUpdate.Core.Event` |
| 枚举/类 | `AppType` class (`ClientApp=1`, `UpgradeApp=2`) | `AppType` enum (Client/Upgrade/OssClient/OssUpgrade) |
| LaunchAsync 返回值 | `Task<GeneralUpdateBootstrap>` | `Task<GeneralUpdateBootstrap>` |
| 扩展点 | 无 | `IUpdateHooks`, `IStrategy`, `IUpdateReporter` 等 |
| 可编程配置 | 仅 Configinfo 属性 | `SetOption<T>(Option<T>, T)` |
| 静默模式 | 不支持 | `Option.Silent` + `SilentPollOrchestrator` |
| `IsComplated` (typo) | `IsComplated` | `IsCompleted`（已修复） |
| 零配置入口 | 不存在 | `SetSource(url, key)` |
| 差分管道配置 | 不支持 | `UseDiffPipeline(Action<DiffPipelineBuilder>)` |

---

## 修复状态

> ✅ **全部更新完成**。所有模板文件已更新为 v10.5.0-rc.1 API。
> ✅ **已知问题全部确认解决**。Bowl API 已验证，NuGet 类型冲突已修复，版本号已统一，CLI 已编译。

| 类别 | 更新内容 | 状态 |
|------|---------|:----:|
| 🔴 Configinfo → UpdateRequest | 全部 11 个 .cs 模板文件 | ✅ |
| 🔴 命名空间迁移 | Common.Shared.Object → Core.Configuration | ✅ |
| 🔴 事件参数命名空间 | Common.Download → Core.Download | ✅ |
| 🟠 IsComplated → IsCompleted | 所有事件处理代码 | ✅ |
| 🟠 BuildProperties.csproj 版本 | 5.* → 10.5.0-rc.1 | ✅ |
| 🟡 文档更新 | SKILL.md / reference.md / RULES.md | ✅ |
| 🔵 高级模板 | CustomHooks/CustomStrategy 激活 v10.5 功能 | ✅ |

---

## 已知剩余问题

### 1. NuGet 类型冲突（v10.5.0-rc.1 ✅ 已解决）

`GeneralUpdate.Common` 独立命名空间在 v10.4.6 中存在于 `GeneralUpdate.Core` 中，与 `GeneralUpdate.Bowl` 冲突。
**v10.5.0-rc.1 已解决**：Bowl 项目不再引用 Core，各自使用独立的 `GeneralUpdate.Bowl` / `GeneralUpdate.Core` 命名空间。

| 场景 | 引用方式 | 状态 |
|------|---------|:----:|
| 有 Bowl | 只引用 `GeneralUpdate.Bowl`（不单独引用 Core） | ✅ 已验证 |
| 无 Bowl | 只引用 `GeneralUpdate.Core` | ✅ 正常 |
| 两者都用 | 同时引用 Core + Bowl | ✅ 无冲突 |

### 2. Bowl LaunchAsync（v10.5.0-rc.1 ✅ 已验证）

`Bowl` 类在 v10.5.0-rc.1 中有公开的 `LaunchAsync` 方法：

```csharp
public async Task<BowlResult> LaunchAsync(BowlContext context, CancellationToken ct = default)
```

- `BowlContext` 为 `readonly record struct`，支持 `Normalize()` 方法填充默认值
- `BowlResult` 包含 `Success` / `ExitCode` / `DumpCaptured` / `DumpFilePath` / `CrashReportPath` / `Restored`
- 不要求 `Bowl.LaunchAsync` 存在其它签名，模板已按实际 API 更新
