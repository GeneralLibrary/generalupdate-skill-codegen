# GeneralUpdate 扩展参考

> ⚠️ 基于 **NuGet v10.5.0-beta.4** API。

## 注入方法调用链（v10.5.0-beta.4）

```csharp
new GeneralUpdateBootstrap()
    .SetConfig(config)                    // 必需：UpdateRequest 配置
    .SetSource(url, key)                  // 或：零配置入口（自动发现 manifest）
    .SetOption(Option.Silent, true)       // 可选：可编程选项
    .Hooks<MyCustomHooks>()               // 可选：生命周期钩子
    .Strategy<MyCustomStrategy>()         // 可选：自定义策略
    .UseDiffPipeline(p => p...)           // 可选：差分管道
    .AddListener*(handler)                // 可选：事件监听
    .LaunchAsync()                        // 执行更新
```

## Pipeline 构建

```csharp
using GeneralUpdate.Core.Pipeline;

var context = new PipelineContext();
context.Add("ZipFilePath", path);
context.Add("Hash", hash);
context.Add("Format", 0);
context.Add("Encoding", Encoding.UTF8);
context.Add("SourcePath", sourcePath);
context.Add("PatchEnabled", true);

await new PipelineBuilder(context)
    .UseMiddleware<HashMiddleware>()
    .UseMiddleware<CompressMiddleware>()
    .UseMiddlewareIf(condition)           // 条件性添加中间件
    .Build();
```

## 事件参数属性

| EventArgs | 命名空间 | 关键属性 |
|-----------|---------|---------|
| `UpdateInfoEventArgs` | `GeneralUpdate.Core.Download` | `Info` (VersionRespDTO?) |
| `MultiDownloadStatisticsEventArgs` | `GeneralUpdate.Core.Download` | `ProgressPercentage`, `Speed`, `Remaining` |
| `MultiDownloadCompletedEventArgs` | `GeneralUpdate.Core.Download` | `Version` (object), `IsCompleted` (bool) |
| `MultiDownloadErrorEventArgs` | `GeneralUpdate.Core.Download` | `Exception`, `Version` (object) |
| `MultiAllDownloadCompletedEventArgs` | `GeneralUpdate.Core.Download` | `IsAllDownloadCompleted`, `FailedVersions` |
| `ExceptionEventArgs` | `GeneralUpdate.Core.Event` | `Exception`, `Message` |
| `ProgressEventArgs` | `GeneralUpdate.Core.Event` | `Progress` / `DiffProgress` |

## Bowl BowlContext 选项

| 属性 | 类型 | 说明 | 默认值 |
|------|------|------|--------|
| `ProcessNameOrId` | string | 被监控的进程名或 PID | 必填 |
| `TargetPath` | string | 应用安装根目录 | 必填 |
| `DumpFileName` | string | Dump 文件名 | 必填 |
| `FailFileName` | string | 故障报告文件名 | 必填 |
| `FailDirectory` | string | 崩溃报告输出目录 | 必填 |
| `BackupDirectory` | string | 备份目录 | 必填 |
| `WorkModel` | string | 工作模式（"Upgrade"/"Normal"） | "Upgrade" |
| `TimeoutMs` | int | 超时时长(毫秒) | 30000 |
| `AutoRestore` | bool | 崩溃后自动回滚 | false |
| `DumpType` | DumpType | Mini / Full | Full |
| `OnCrash` | delegate | 崩溃回调 | null |

## HTTP 认证提供者

通过 `UpdateRequest.AuthScheme` + `UpdateRequest.Token` 配置，或通过 `HttpAuth<T>()` 注入自定义实现：

| 方案 | AuthScheme 值 | Token | 注入方式 |
|------|--------------|-------|---------|
| HMAC (默认) | `AuthScheme.Hmac` | 使用 AppSecretKey | `bootstrap.HttpAuth<HmacAuthProvider>()` |
| Bearer Token | `AuthScheme.Bearer` | JWT Token | `bootstrap.HttpAuth<BearerTokenAuthProvider>()` |
| Api Key | `AuthScheme.ApiKey` | API Key | `bootstrap.HttpAuth<ApiKeyAuthProvider>()` |
| Basic Auth | `AuthScheme.Basic` | Base64 | `bootstrap.HttpAuth<BasicAuthProvider>()` |

```csharp
new GeneralUpdateBootstrap()
    .SetConfig(new UpdateRequest
    {
        AuthScheme = AuthScheme.Bearer,
        Token = "eyJhbG..."
    })
    .LaunchAsync();

// 或使用自定义认证提供者
new GeneralUpdateBootstrap()
    .HttpAuth<MyCustomAuthProvider>()
    .SetConfig(config)
    .LaunchAsync();
```

## AOT JSON 上下文

```csharp
using GeneralUpdate.Core.JsonContext;

// 可用的 JsonSerializerContext 子类：
// VersionRespJsonContext            — 版本响应
// PacketJsonContext                  — 更新包
// ProcessInfoJsonContext             — 进程信息（IPC）
// GlobalConfigInfoOSSJsonContext     — OSS 配置
// VersionOSSJsonContext              — OSS 版本
// HttpParameterJsonContext           — HTTP 参数
// ReportRespJsonContext              — 上报响应
// FileNodesJsonContext               — 文件节点
```

## 差分引擎

```csharp
using GeneralUpdate.Differential;
using GeneralUpdate.Differential.Matchers;

// 清理模式（服务端生成补丁）
await DifferentialCore.CleanAsync(srcDir, tgtDir, patchDir);

// 脏模式（客户端应用补丁）
await DifferentialCore.DirtyAsync(installPath, patchDir);

// 自定义匹配器
class MyMatcher : ICleanMatcher
{
    public ComparisonResult Match(string srcDir, string tgtDir)
    {
        // 自定义文件匹配逻辑
    }
}
```
