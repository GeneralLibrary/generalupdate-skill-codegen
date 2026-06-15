# GeneralUpdate 扩展点 API 速查

## 注入方法调用链

```csharp
new GeneralUpdateBootstrap()
    .SetSource(url, key)                    // 必需：服务端 API + 密钥
    .SetConfig(request)                     // 备选：完整配置（替代 SetSource）
    .SetOption(option, value)               // 运行时选项
    .AddListener*(handler)                  // 事件监听
    .Hooks<T>()                             // 生命周期回调
    .Strategy<T>()                          // 平台策略替换
    .UpdateReporter<T>()                    // 状态上报
    .DownloadSource<T>()                    // 版本源
    .DownloadOrchestrator<T>()             // 下载编排
    .DownloadPolicy<T>()                    // 重试策略
    .DownloadExecutor<T>()                  // 下载执行器
    .DownloadPipeline<T>()                  // 下载后处理
    .SslValidationPolicy<T>()               // SSL 验证
    .HttpAuthProvider<T>()                  // HTTP 认证
    .LaunchAsync()                          // 执行更新
```

## IUpdateHooks 生命周期

```
Client 进程:
  OnBeforeUpdateAsync() → 返回 false 中止更新
  OnDownloadCompletedAsync() → 所有包下载完成
  → 启动 Upgrade 进程

Upgrade 进程:
  OnBeforeUpdateAsync() → 返回 false 中止更新
  → 执行管道（Hash→解压→Patch）
  OnAfterUpdateAsync() → 更新完成
  OnBeforeStartAppAsync() → 返回 false 阻止启动
  → 启动主应用

任意进程出错时:
  OnUpdateErrorAsync() → 错误处理
```

## SSL 验证策略

```csharp
// 默认：系统默认证书验证
public class DefaultSslValidationPolicy : ISslValidationPolicy
{
    public bool ValidateServerCertificate(
        object sender, X509Certificate? certificate,
        X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        => sslPolicyErrors == SslPolicyErrors.None;
}
```

## HTTP 认证提供者

| 提供者 | 实现 | 适用场景 |
|--------|------|---------|
| `HmacAuthProvider` | HMAC-SHA256 Header | GeneralSpacestation 默认 |
| `BasicAuthProvider` | Basic Auth Header | 企业内网 |
| `BearerAuthProvider` | Bearer Token | OAuth / JWT |
| 自定义 | 实现 `IHttpAuthProvider` | 任意认证方案 |

## Bowl 选项

| 选项 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `TargetAppPath` | string | — | 被监控的主应用路径 |
| `InstallPath` | string | — | 安装目录 |
| `AutoRestore` | bool | false | 崩溃时自动回滚备份 |
| `ReportOutputPath` | string | `CrashReports/` | 崩溃报告输出目录 |

## Bowl 崩溃生命周期

```
Upgrade 进程完成更新
  → 调用 StartAppAsync
    → 启动主应用
    → (可选) 启动 Bowl 守护进程
      → Bowl Phase 1: 准备 procdump 监控
      → Bowl Phase 2: 运行监控子进程
      → Bowl Phase 3: 检查 dump 文件
      → Bowl Phase 4 (崩溃):
          → 生成 MiniDump
          → 写入 CrashReport.json
          → 自动回滚（如配置）
          → 导出系统诊断信息
          → 触发 OnCrash 回调
```
