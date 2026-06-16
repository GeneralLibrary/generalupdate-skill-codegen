---
name: generalupdate-advanced
description: |
  Reference guide for GeneralUpdate internal architecture — Pipeline, middleware,
  Strategy, Differential engine, Bowl crash monitor, FileTree, blacklist, and AOT.
  Covers what is and isn't available in v10.4.6 stable release vs dev branch.
  Triggers on: "extension points", "custom hooks", "Bowl", "crash dump", "IPC",
  "named pipe", "shared memory", "custom strategy", "download pipeline",
  "SSL policy", "auth provider", "custom download", "extension management",
  "黑名单", "BlackList", "FileTree", "AOT", "NativeAOT", "高级定制",
  "自定义策略", "自定义认证", "Bowl守护", "IPC替换".
when_to_use: |
  - User wants to customize GeneralUpdate beyond basic Bootstrap config
  - User wants crash monitoring and auto-restore (Bowl)
  - User needs custom authentication provider
  - User asks about AOT compatibility or trim warnings
  - User needs file filtering (blacklist) or file tree diffing
  - User wants to integrate Drivelution for driver updates
  - User already completed basic integration and wants more control
allowed-tools: "Read, Write, Edit, Glob"
---

# 🔧 GeneralUpdate 高级定制参考

涵盖扩展点架构、Pipeline 管道、差分引擎、Bowl 崩溃守护、事件系统、文件系统工具等。

> ⚠️ **API 版本说明**：本指南基于 **NuGet v10.4.6 稳定版**。
> 以下功能在稳定版中**不存在**（但在开发分支 v10.5.0-beta.2 中已有）：
> - `IUpdateHooks` 生命周期钩子
> - `IProcessInfoProvider` IPC 替换接口
> - `SilentPollOrchestrator` 静默轮询器
> - `Option` 可编程配置系统（v10.4.6 仅使用 `Configinfo` 属性）
> - `ISslValidationPolicy` SSL 策略接口
>
> 各功能的可用性在文中已标注。

---

## 📋 用户需求提取（高级定制前必须确认）

```
### 定制目标（必需）
- 需要什么定制: ______（Bowl 崩溃守护 / IPC 替换 / Pipeline 定制 / 自定义策略 / AOT / Drivelution / 黑名单 / 认证提供者 / 差分引擎）
- 使用的 GeneralUpdate 版本: ______（v10.4.6 稳定版 / v10.5.0+ 开发分支）
- .NET 版本: ______（.NET 6/8/9/10）

### Bowl（如果选择）
- 被监控进程名: ______
- 工作模式: ______（Normal / Upgrade）
- 是否需要崩溃 Dump: ______（是/否）
- 备份目录路径: ______

### IPC 替换（如果选择）
- 替换方式: ______（NamedPipe / SharedMemory / 自定义）
- 目标平台: ______（Windows / Linux / macOS / 跨平台）
- 安全要求: ______（加密 / 签名 / 无额外安全）

### AOT（如果选择）
- 当前剪裁警告: ______（有/无）
- 是否使用反射: ______（是/否）
- JSON 序列化需求: ______（有/无）
```

---

## 1. Pipeline 管道系统（v10.4.6 可用）

GeneralUpdate 使用 Pipeline 管道模式处理更新包的校验、解压、补丁应用。

### PipelineBuilder API

```csharp
using GeneralUpdate.Common.Internal.Pipeline;
using GeneralUpdate.Common.Internal.Strategy;

// 创建管道上下文
var context = new PipelineContext();
context.Add("ZipFilePath", @"C:\temp\update.zip");
context.Add("Hash", "sha256-hex-value");
context.Add("Format", 0);  // 0=Zip
context.Add("Encoding", System.Text.Encoding.UTF8);
context.Add("SourcePath", @"C:\Program Files\MyApp");
context.Add("PatchEnabled", true);

// 构建并执行管道
await new PipelineBuilder(context)
    .UseMiddleware<HashMiddleware>()      // 哈希校验
    .UseMiddleware<CompressMiddleware>()  // 解压
    .UseMiddleware<PatchMiddleware>()     // 差分补丁（需安装 Differential 包）
    .Build();
```

| 中间件 | 类名 | 命名空间 | 功能 |
|--------|------|---------|------|
| 哈希校验 | `HashMiddleware` | `GeneralUpdate.Core.Pipeline` | SHA256 完整性校验 |
| 解压 | `CompressMiddleware` | `GeneralUpdate.Core.Pipeline` | 解压 ZIP 包 |
| 差分补丁 | `PatchMiddleware` | `GeneralUpdate.Core.Pipeline` | 应用 BSDIFF/HDiffPatch 补丁 |
| 驱动更新 | `DrivelutionMiddleware` | `GeneralUpdate.Core.Pipeline` | Windows 驱动安装 |

---

## 2. 策略系统（v10.4.6 可用）

GeneralUpdate 内置三种平台策略，通过 `AbstractStrategy` 模板方法模式实现：

| 策略 | 类名 | 平台 |
|------|------|------|
| Windows | `WindowsStrategy` | Windows |
| Linux | `LinuxStrategy` | Linux |
| OSS | `OSSStrategy` | 跨平台（对象存储） |

> ⚠️ 稳定版**不支持**通过 `bootstrap.Strategy<T>()` 注入自定义策略。
> 自定义策略需要继承 `AbstractStrategy` 并直接调用。

---

## 3. Bowl 崩溃守护（v10.4.6 存在但功能有限）

Bowl 是一个崩溃监控组件，通过 `MonitorParameter` 配置。

> ⚠️ **注意**：v10.4.6 的 Bowl 仅提供基础类型定义，`Bowl` 类没有公开的 `LaunchAsync` 方法。
> 完整功能在开发分支（v10.5.0-beta.2）中。

### MonitorParameter 配置

```csharp
using GeneralUpdate.Bowl;
using GeneralUpdate.Bowl.Strategys;

var param = new MonitorParameter
{
    ProcessNameOrId = "MyApp.exe",
    DumpFileName = "v1.0.0.0_fail.dmp",
    FailFileName = "v1.0.0.0_fail.json",
    TargetPath = @"C:\Program Files\MyApp",
    FailDirectory = @"C:\Program Files\MyApp\fail",
    BackupDirectory = @"C:\Program Files\MyApp\backup",
    WorkModel = "Upgrade",
};

// Bowl 实例（v10.4.6 无公开 LaunchAsync，此为占位）
var bowl = new Bowl();
```

完整 Bowl 崩溃守护功能请关注 GeneralUpdate 后续版本。

---

## 4. EventManager 事件系统（v10.4.6 可用）

EventManager 是一个全局单例，提供事件的发布和订阅：

```csharp
using GeneralUpdate.Common.Internal.Event;

// 添加监听
EventManager.Instance.AddListener((object? sender, UpdateInfoEventArgs e) =>
{
    // 处理版本发现事件
});

// 手动分发事件
EventManager.Instance.Dispatch(this, new ExceptionEventArgs(ex, "自定义错误"));

// 清空所有监听
EventManager.Instance.Clear();

// 释放
EventManager.Instance.Dispose();
```

> ⚠️ EventManager 是全局单例，`Dispose()` 后 `Instance` 仍然可访问（代码审计发现）。

---

## 5. 文件系统工具（v10.4.6 可用）

### BlackList（黑名单）

`Configinfo` 支持通过以下属性排除文件：

```csharp
var config = new Configinfo
{
    // ...
    BlackFiles = new List<string> { "*.log", "*.tmp" },
    BlackFormats = new List<string> { ".pdb", ".vshost.exe" },
    SkipDirectorys = new List<string> { "logs", "cache", "temp" },
};
```

### FileTree（文件树对比）

```csharp
using GeneralUpdate.Common.FileBasic;

var tree = new FileTree();
var snapshot = tree.CreateSnapshot(@"C:\Program Files\MyApp");
// 或从 StorageManager 获取比较结果
```

---

## 6. 差分引擎（v10.4.6 可用，需安装 Differential 包）

安装 `GeneralUpdate.Differential` 包后可用：

```csharp
// DifferentialCore 提供核心差分能力
using GeneralUpdate.Differential;

// 清理模式（服务端）：对比新旧版本生成补丁
await DifferentialCore.CleanAsync(srcDir, tgtDir, patchDir);

// 脏模式（客户端）：应用补丁
await DifferentialCore.DirtyAsync(installDir, patchDir);
```

自定义匹配器（v10.4.6 可用）：

```csharp
using GeneralUpdate.Differential.Matchers;

// 自定义清理匹配器
var cleanMatcher = new DefaultCleanMatcher();  // 或实现 ICleanMatcher
var dirtyMatcher = new DefaultDirtyMatcher();  // 或实现 IDirtyMatcher
```

---

## 7. AOT / NativeAOT 兼容性

GeneralUpdate.Core v10.4.6 支持 .NET Native AOT：

```xml
<PropertyGroup>
  <IsAotCompatible>true</IsAotCompatible>
  <EnableTrimAnalyzer>true</EnableTrimAnalyzer>
</PropertyGroup>
```

JSON 序列化上下文（减少 AOT 大小）：

```csharp
using GeneralUpdate.Common.Internal.JsonContext;

// 使用内置的 JsonSerializerContext
// VersionRespJsonContext, PacketJsonContext, ProcessInfoJsonContext 等
```

---

## 8. Drivelution（Windows 驱动更新）

`GeneralUpdate.Drivelution` 包提供 Windows 驱动管理：

```csharp
using GeneralUpdate.Drivelution;

// 扫描驱动目录
var allDrivers = GeneralDrivelution.ScanDirectory(driverDir);

// 验证驱动
var isValid = GeneralDrivelution.ValidateDriver(driverPath);

// 安装驱动（DIFx → SetupAPI → PnPUtil 级联）
var result = GeneralDrivelution.InstallDriver(driverPath);
```

---

## 内容索引

| 主题 | 可用性 | 参考 |
|------|--------|------|
| Pipeline 管道 | ✅ v10.4.6 | `GeneralUpdate.Common.Internal.Pipeline` |
| 策略系统 | ✅ v10.4.6 | `GeneralUpdate.Common.Internal.Strategy` |
| FileTree | ✅ v10.4.6 | `GeneralUpdate.Common.FileBasic` |
| BlackList | ✅ v10.4.6 | `Configinfo.BlackFiles` 等属性 |
| 差分引擎 | ✅ 需 `GeneralUpdate.Differential` | `DifferentialCore` |
| AOT | ✅ v10.4.6 | `JsonSerializerContext` 子类 |
| EventManager | ✅ v10.4.6 | `GeneralUpdate.Common.Internal.Event` |
| Bowl 崩溃守护 | ⚠️ 基础类型 | `GeneralUpdate.Bowl.Bowl` |
| IUpdateHooks | ❌ v10.4.6 不支持 | 开发分支 v10.5.0-beta.2 中 |
| 自定义 Strategy 注入 | ❌ v10.4.6 不支持 | 开发分支 v10.5.0-beta.2 中 |
| IPC 替换接口 | ❌ v10.4.6 不支持 | 开发分支 v10.5.0-beta.2 中 |
| SilentPollOrchestrator | ❌ v10.4.6 不支持 | 开发分支 v10.5.0-beta.2 中 |
| Option 系统 | ❌ v10.4.6 不支持 | 仅 Configinfo 属性 |

---

## ✅ 高级定制验证清单

### Bowl 崩溃守护
- [ ] 只引用了 `GeneralUpdate.Bowl`（不单独引用 Core）
- [ ] `MonitorParameter` 的 `ProcessNameOrId` 与实际进程名匹配
- [ ] `TargetPath` 设置为应用安装根目录，非子目录
- [ ] `WorkModel` 根据场景选择 Correct（Normal/Upgrade）
- [ ] `FailDirectory` 有写入权限
- [ ] Linux/macOS 无此功能（Bowl 仅 Windows）

### Pipeline 定制
- [ ] `PipelineContext` 中的 Key 名称使用字符串常量拼写正确（"ZipFilePath", "Hash", "Format", "Encoding", "SourcePath", "PatchEnabled"）
- [ ] 中间件注册顺序正确：Hash → Compress → Patch → Drivelution
- [ ] `Encoding` 设置为 `Encoding.UTF8`

### AOT/NativeAOT
- [ ] 启用了 `<IsAotCompatible>true</IsAotCompatible>`
- [ ] 对反射路径添加了 `[DynamicDependency]` 或 `[RequiresUnreferencedCode]`
- [ ] 使用了内置的 `JsonSerializerContext` 子类（减少裁剪）
- [ ] 通过 `dotnet build` 无 AOT 裁剪警告

### IPC 替换
- [ ] 替换方案在目标平台上可用（Linux 无 NamedPipe 服务端，但有客户端）
- [ ] 加密方案与 Client/Upgrade 两端一致
- [ ] IPC 数据长度有上限保护（防止内存溢出）

---

## ⚠️ 反模式清单（高级定制特有）

| # | 反模式 | 后果 | 正确做法 |
|---|--------|------|---------|
| 1 | **在 v10.4.6 稳定版上使用开发分支 API（IUpdateHooks 等）** | 编译失败 / 运行时 MissingMethodException | 检查 API 可用性表 |
| 2 | **PipeLineContext Key 拼写错误（如 ZipFilePath 写成 ZipFilePatch）** | Pipeline 运行异常，值未传递 | 使用类库公开的常量或文档中的 Key 名 |
| 3 | **Bowl 的 WorkModel 设为 Upgrade 但进程是主程序** | 监控逻辑错误 | Normal=主线进程，Upgrade=升级进程 |
| 4 | **Windows 上 IPC 使用默认加密密钥** | 加密可被破解（代码审计 #1） | 使用强密钥（≥ 32 字符） |
| 5 | **差分包生成时使用不同版本的源文件结构** | 补丁应用失败，文件找不到 | 源和目标版本的文件结构必须一致 |
| 6 | **AOT 项目中使用了大量反射且未标记 DynamicDependency** | 运行时 TypeLoadException / 被剪裁 | 使用源代码生成器或显式标记保留 |
| 7 | **Pipeline 中 PatchMiddleware 排在 CompressMiddleware 前面** | 未解压就试图打补丁 | 顺序必须是 Compress→Patch |
| 8 | **自定义 Strategy 直接操作 private 方法** | 下游版本更新后 API 兼容性破裂 | 通过受保护的抽象方法扩展 |

---

## 相关技能

- `/generalupdate-init` — Bootstrap 配置
- `/generalupdate-strategy` — 更新策略选择
- `/generalupdate-troubleshoot` — 问题诊断
