# GeneralUpdate 扩展参考

> ⚠️ 基于 **NuGet v10.4.6 稳定版** API。开发分支功能已标注。

## 注入方法调用链（v10.4.6）

```csharp
new GeneralUpdateBootstrap()
    .SetConfig(config)                    // 必需：Configinfo 配置
    .AddListener*(handler)                // 事件监听（可选）
    .LaunchAsync()                        // 执行更新
```

## Pipeline 构建

```csharp
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
| `UpdateInfoEventArgs` | `Common.Download` | `Info` (VersionRespDTO?) |
| `MultiDownloadStatisticsEventArgs` | `Common.Download` | `ProgressPercentage`, `Speed`, `Remaining` |
| `MultiDownloadCompletedEventArgs` | `Common.Download` | `Version` (object), `IsComplated` (bool) |
| `MultiDownloadErrorEventArgs` | `Common.Download` | `Exception`, `Version` (object) |
| `MultiAllDownloadCompletedEventArgs` | `Common.Download` | `IsAllDownloadCompleted`, `FailedVersions` |
| `ExceptionEventArgs` | `Common.Internal` | `Exception`, `Message` |

## Bowl 选项

| 属性 | 类型 | 说明 |
|------|------|------|
| `ProcessNameOrId` | string | 被监控的进程名或 PID |
| `TargetPath` | string | 应用安装根目录 |
| `DumpFileName` | string | Dump 文件名 |
| `FailFileName` | string | 故障报告文件名 |
| `FailDirectory` | string | 崩溃报告输出目录 |
| `BackupDirectory` | string | 备份目录 |
| `WorkModel` | string | 工作模式（"Upgrade"/"Normal"） |
| `ExtendedField` | string | 扩展字段（版本号等） |

## HTTP 认证提供者

通过 `Configinfo.Scheme` + `Configinfo.Token` 在 Configinfo 中配置：

| 方案 | Scheme 值 | Token |
|------|-----------|-------|
| HMAC (默认) | 无需设置（使用 AppSecretKey） | — |
| Bearer Token | `"Bearer"` | JWT Token |
| Basic Auth | `"Basic"` | Base64(username:password) |

## AOT JSON 上下文

```csharp
using GeneralUpdate.Common.Internal.JsonContext;

// 可用的 JsonSerializerContext 子类：
// VersionRespJsonContext      — 版本响应
// PacketJsonContext            — 更新包
// ProcessInfoJsonContext       — 进程信息（IPC）
// GlobalConfigInfoOSSJsonContext — OSS 配置
// VersionOSSJsonContext         — OSS 版本
// HttpParameterJsonContext      — HTTP 参数
// ReportRespJsonContext         — 上报响应
// FileNodesJsonContext          — 文件节点
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
