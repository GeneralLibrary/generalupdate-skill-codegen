---
name: generalupdate-advanced
description: |
  Advanced GeneralUpdate customization: lifecycle hooks, custom strategies, Bowl crash
  monitoring, IPC providers, custom download/authentication, and extension management.
  Triggers on: "custom strategy", "custom hooks", "Bowl crash monitoring", "crash dump",
  "IPC named pipe", "named pipe IPC", "custom download", "custom auth provider",
  "extension management", "自定义策略", "Hooks", "Bowl守护", "崩溃监控",
  "IPC", "自定义下载", "自定义认证", "扩展管理", "插件管理".
  Also triggers when user mentions advanced configuration or replacement of defaults.
when_to_use: |
  - User needs custom update lifecycle hooks (before/after update, download complete)
  - User wants crash monitoring and auto-restore (Bowl)
  - User wants to replace IPC mechanism (NamedPipe instead of encrypted file)
  - User needs custom authentication provider
  - User wants custom download executor or orchestrator
  - User needs custom OS strategy (replacing Windows/Linux/Mac strategy)
  - User asks about extension/plugin management
  - User already has basic integration working and wants more control
allowed-tools: "Read, Write, Edit, Glob"
---

# 🔧 GeneralUpdate 高级定制

提供 GeneralUpdate 全部扩展点的注入指南和代码模板。

## 扩展点一览

GeneralUpdate 提供 10+ 扩展点，可通过泛型方法注入自定义实现：

| 扩展方法 | 接口 | 默认实现 | 用途 |
|---------|------|---------|------|
| `.Hooks<T>()` | `IUpdateHooks` | — | 生命周期回调（更新前/后/下载完成/启动应用前） |
| `.Strategy<T>()` | `IStrategy` | `WindowsStrategy` / `LinuxStrategy` / `MacStrategy` | 替换平台级更新策略 |
| `.UpdateReporter<T>()` | `IUpdateReporter` | `HttpUpdateReporter` | 自定义状态上报 |
| `.DownloadSource<T>()` | `IDownloadSource` | `HttpDownloadSource` | 自定义版本源 |
| `.DownloadOrchestrator<T>()` | `IDownloadOrchestrator` | `DefaultDownloadOrchestrator` | 自定义下载编排 |
| `.DownloadPolicy<T>()` | `IDownloadPolicy` | `DefaultRetryPolicy` | 自定义重试策略 |
| `.DownloadExecutor<T>()` | `IDownloadExecutor` | `HttpDownloadExecutor` | 自定义下载执行器（如 FTP） |
| `.DownloadPipeline<T>()` | `IDownloadPipeline` | — | 下载后处理（解密/扫描/哈希校验） |
| `.SslValidationPolicy<T>()` | `ISslValidationPolicy` | `DefaultSslValidationPolicy` | 自定义 SSL 证书验证 |
| `.HttpAuthProvider<T>()` | `IHttpAuthProvider` | `HmacAuthProvider` | 自定义 HTTP 认证（HMAC/Basic/Bearer/自定义） |

## 扩展点注入示例

```csharp
var bootstrap = new GeneralUpdateBootstrap()
    .SetSource(url, key)
    .Hooks<MyCustomHooks>()                     // 自定义生命周期
    .UpdateReporter<MyCustomReporter>()          // 自定义上报
    .HttpAuthProvider<MyCustomAuthProvider>()    // 自定义认证
    .DownloadPolicy<MyCustomRetryPolicy>()       // 自定义重试
    .SslValidationPolicy<MySslPolicy>()          // 自定义 TLS 验证
    .SetOption(Option.AppType, AppType.Client)
    .LaunchAsync();
```

## 高级功能模板

| 模板文件 | 功能 | 难度 |
|---------|------|------|
| `CustomHooks.cs` | 生命周期回调：更新前/后/下载完成/启动前 | ⭐ |
| `CustomStrategy.cs` | 完全替换平台更新策略 | ⭐⭐⭐ |
| `BowlIntegration.cs` | 崩溃监控 + MiniDump + 自动回滚 | ⭐⭐ |
| `NamedPipeIPC.cs` | 命名管道 IPC（替代加密文件） | ⭐⭐⭐ |

## 输出

根据用户的高级定制需求，输出：
- ✅ 对应扩展点的接口实现代码
- ✅ 注入配置代码
- ✅ 注意事项和最佳实践
