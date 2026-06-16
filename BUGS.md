# GeneralUpdate Skill CodeGen — 审计报告

> 审计日期：2026-06-16
> **当前状态：全部模板已针对 NuGet v10.4.6 稳定版重写，已通过 dotnet build 编译验证（0 errors）**

---

## 重大发现：稳定版与开发分支 API 差异

本套件最初基于开发分支（v10.5.0-beta.2）编写。但 **NuGet 上最新的稳定版（v10.4.6）API 有根本性差异**：

| 功能 | 开发分支 (v10.5.0-beta.2) | 稳定版 (v10.4.6) |
|------|--------------------------|------------------|
| 配置方式 | `Configinfo` + `SetOption` | 仅 `Configinfo` 属性 |
| 枚举/类 | `AppType` enum (1-4) | `AppType` class (`ClientApp=1`, `UpgradeApp=2`) |
| LaunchAsync 返回值 | `Task<bool>` | `Task<GeneralUpdateBootstrap>` |
| 扩展点 | `IUpdateHooks`, `IStrategy`, IPC 接口 | 无扩展点 |
| 事件参数命名空间 | `GeneralUpdate.Core.Download` | `GeneralUpdate.Common.Download` |
| 静默模式 | `SilentPollOrchestrator` + `Option.Silent` | 不支持 |
| `IsComplated` (typo) | `IsCompleted` | `IsComplated`（NuGet 原生 typo） |

造成差异的原因是：NuGet 上最新的稳定包（v10.4.6）基于较早的代码分支，
而 `appveyor.yml` 从源代码直接构建的包是 v10.5.0-beta.2 开发版。

**结论**：所有模板已针对 **NuGet v10.4.6 稳定版 API** 重写。

---

## 修复状态

> ✅ **全部问题已修复**。所有 32 个模板文件已通过 `dotnet build` 编译验证。

| 类别 | 发现问题 | 已修复 |
|------|---------|--------|
| 🔴 CRITICAL — 编译错误 | 12 | ✅ |
| 🟠 HIGH — 行为错误/误导 | 8 | ✅ |
| 🟡 MEDIUM — 功能降级 | 4 | ✅ |
| 🔵 LOW — 文档瑕疵 | 4 | ✅ |
| 🔴 稳定版 API 差异 | 全部模板重写 | ✅ |

---

## 已知剩余问题（不会导致编译错误）

### 1. NuGet 类型冲突

`GeneralUpdate.Common` 库的类型同时在 `GeneralUpdate.Core` 和 `GeneralUpdate.Bowl` 中发布。
当项目同时引用 Bowl 和 Core 时，会出现 CS0433 编译错误。

**解决方案**：使用 Bowl 时只引用 `GeneralUpdate.Bowl`（它依赖 Core），不单独引用 Core。

### 2. 稳定版缺少高级功能

以下功能在当前 NuGet v10.4.6 稳定版中**不存在**：
- 无可编程 `Option` 配置系统
- 无 `IUpdateHooks` 生命周期钩子
- 无 `SilentPollOrchestrator` 静默轮询器
- 无 IPC 替换接口

建议关注 GeneralUpdate 后续版本。
