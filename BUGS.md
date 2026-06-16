# GeneralUpdate Skill CodeGen — 审计报告

> 审计日期：2026-06-16
> **当前状态：全部模板已针对 NuGet v10.5.0-beta.4 更新，已通过 dotnet build 编译验证（0 errors）**

---

## 版本升级：v10.4.6 稳定版 → v10.5.0-beta.4

套件从 NuGet v10.4.6 稳定版升级到 v10.5.0-beta.4。主要变更：

| 功能 | v10.4.6 (旧) | v10.5.0-beta.4 (新) |
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

> ✅ **全部更新完成**。所有模板文件已更新为 v10.5.0-beta.4 API。

| 类别 | 更新内容 | 状态 |
|------|---------|:----:|
| 🔴 Configinfo → UpdateRequest | 全部 11 个 .cs 模板文件 | ✅ |
| 🔴 命名空间迁移 | Common.Shared.Object → Core.Configuration | ✅ |
| 🔴 事件参数命名空间 | Common.Download → Core.Download | ✅ |
| 🟠 IsComplated → IsCompleted | 所有事件处理代码 | ✅ |
| 🟠 BuildProperties.csproj 版本 | 5.* → 10.5.0-beta.4 | ✅ |
| 🟡 文档更新 | SKILL.md / reference.md / RULES.md | ✅ |
| 🔵 高级模板 | CustomHooks/CustomStrategy 激活 v10.5 功能 | ✅ |

---

## 已知剩余问题

### 1. NuGet 类型冲突（未变）

`GeneralUpdate.Common` 库的类型同时在 `GeneralUpdate.Core` 和 `GeneralUpdate.Bowl` 中发布。
当项目同时引用 Bowl 和 Core 时，会出现 CS0433 编译错误。

**解决方案**：使用 Bowl 时只引用 `GeneralUpdate.Bowl`（它依赖 Core），不单独引用 Core。

### 2. Bowl LaunchAsync 可用性待确认

v10.5.0-beta.4 中的 `Bowl` 类是否有公开的 `LaunchAsync` 方法需要在实际使用前确认。
如不可用，请保持事件回调替代方案。
