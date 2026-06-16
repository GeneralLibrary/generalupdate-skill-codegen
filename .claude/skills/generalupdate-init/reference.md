# GeneralUpdate 参考手册

> ⚠️ **针对 NuGet v10.5.0-beta.4 API**。该版本使用 `UpdateRequest` 配置，支持可编程 `Option` 系统。

## NuGet 包

所有包的最新版本：[nuget.org/profiles/GeneralLibrary](https://www.nuget.org/profiles/GeneralLibrary)

| 包名 | 用途 | 必需 | .NET 版本 | 典型版本 |
|------|------|------|-----------|---------|
| `GeneralUpdate.Core` | 核心引擎（Bootstrap/下载/事件） | ✅ 是 | net8.0;net10.0 | 10.5.0-beta.4 |
| `GeneralUpdate.Differential` | BSDIFF/HDiffPatch 差分补丁 | ❌ 可选 | net8.0;net10.0 | 10.5.0-beta.4 |
| `GeneralUpdate.Bowl` | 进程崩溃监控、MiniDump、自动回滚 | ❌ 可选 | net8.0;net10.0 | 10.5.0-beta.4 |
| `GeneralUpdate.Extension` | 插件管理系统 | ❌ 可选 | net8.0;net10.0 | ≥ 10.5.0 |
| `GeneralUpdate.Drivelution` | Windows 驱动更新 | ❌ 可选 | net8.0;net10.0 | 10.5.0-beta.4 |

> ⚠️ **NuGet 类型冲突**：
> - `GeneralUpdate.Differential` 的 `DifferentialCore` 等类型已内嵌在 `GeneralUpdate.Core` 中，**不需额外引用**（直接使用 Core 即可）
> - `GeneralUpdate.Bowl` 和 `GeneralUpdate.Core` **不能同时引用**（两者都发布了 `GeneralUpdate.Common` 导致 CS0433）
> - 使用 Bowl 时**只引用 `GeneralUpdate.Bowl`**（它传递依赖 Core 的所有功能）

## UpdateRequest 字段完整说明

| 字段 | 类型 | 必需 | 说明 |
|------|------|:----:|------|
| `UpdateUrl` | string | ✅ | 版本验证 API 地址 |
| `AppSecretKey` | string | ✅ | HMAC 认证 + IPC 加密密钥 |
| `MainAppName` | string | ✅ | 主程序文件名（含 .exe） |
| `ClientVersion` | string | ✅ | 当前版本号（4 段式） |
| `ProductId` | string | ✅ | 产品标识 |
| `InstallPath` | string | ✅ | 应用安装目录 |
| `UpdateAppName` | string | ❌ | 升级程序文件名（默认 "Update.exe"） |
| `UpdatePath` | string | ❌ | 升级程序子目录（默认 "update" 子目录） |
| `UpgradeClientVersion` | string | ❌ | Upgrade 进程版本号 |
| `ReportUrl` | string | ❌ | 状态上报 API 地址 |
| `UpdateLogUrl` | string | ❌ | 更新日志 API 地址 |
| `AuthScheme` | AuthScheme | ❌ | 认证方案：Hmac/Bearer/ApiKey/Basic |
| `Scheme` | string | ❌ | HTTP 认证方案字符串 |
| `Token` | string | ❌ | HTTP 认证令牌 |
| `BasicUsername` | string | ❌ | Basic 认证用户名 |
| `BasicPassword` | string | ❌ | Basic 认证密码 |
| `Files` | List\<string\> | ❌ | 备份排除文件名模式（原 BlackFiles） |
| `Formats` | List\<string\> | ❌ | 备份排除扩展名（原 BlackFormats） |
| `Directories` | List\<string\> | ❌ | 备份排除目录名（原 SkipDirectorys） |
| `DriverDirectory` | string | ❌ | 驱动目录路径 |

## 后端 API 协议

### 版本验证

```
POST {UpdateUrl}
Content-Type: application/json

{
  "appKey": "client-app-key",
  "appType": 1,
  "clientVersion": "1.0.0.0",
  "productId": "my-product-001",
  "platform": "Windows",
  "tenantId": "default"
}
```

> `appType`: 1=Client, 2=Upgrade

成功响应：
```json
{
  "code": 200,
  "message": "",
  "body": [
    {
      "version": "1.1.0.0",
      "url": "https://storage/packages/v1.1.0.0.zip",
      "hash": "sha256-hex",
      "size": 1048576,
      "name": "update.zip",
      "appType": 1,
      "isForcibly": false
    }
  ]
}
```

### 状态上报

```
POST {ReportUrl}
Content-Type: application/json

{
  "recordId": "升级记录 ID",
  "type": 1,
  "status": 0,
  "message": "更新成功"
}
```

## 事件参数属性

| EventArgs | 关键属性 | 说明 |
|-----------|---------|------|
| `UpdateInfoEventArgs` | `Info` (VersionRespDTO?) → `Info.Body` (List\<VersionInfo\>) | 版本验证结果 |
| `MultiDownloadStatisticsEventArgs` | `ProgressPercentage`, `Speed`, `Remaining`, `TotalBytesToReceive`, `BytesReceived` | 下载进度 |
| `MultiDownloadCompletedEventArgs` | `Version` (object), `IsCompleted` (bool) | 版本下载完成 |
| `MultiDownloadErrorEventArgs` | `Exception`, `Version` (object) | 下载错误 |
| `MultiAllDownloadCompletedEventArgs` | `IsAllDownloadCompleted` (bool), `FailedVersions` (IList) | 全部完成 |
| `ExceptionEventArgs` | `Exception`, `Message` | 异常 |

## AppType 枚举

```
AppType.Client = 1      // 标准客户端
AppType.Upgrade = 2     // 标准升级程序
AppType.OssClient = 3   // OSS 客户端
AppType.OssUpgrade = 4  // OSS 升级程序
```

## 命名空间速查

| 类型 | 命名空间 |
|------|---------|
| `GeneralUpdateBootstrap` | `GeneralUpdate.Core` |
| `UpdateRequest` | `GeneralUpdate.Core.Configuration` |
| `UpdateRequestBuilder` | `GeneralUpdate.Core.Configuration` |
| `AppType`, `Option`, `Format`, `DiffMode` | `GeneralUpdate.Core.Configuration` |
| `UpdateInfoEventArgs` | `GeneralUpdate.Core.Download` |
| `MultiDownloadStatisticsEventArgs` | `GeneralUpdate.Core.Download` |
| `MultiDownloadCompletedEventArgs` | `GeneralUpdate.Core.Download` |
| `MultiDownloadErrorEventArgs` | `GeneralUpdate.Core.Download` |
| `MultiAllDownloadCompletedEventArgs` | `GeneralUpdate.Core.Download` |
| `ExceptionEventArgs` | `GeneralUpdate.Core.Event` |
| `ProgressEventArgs` | `GeneralUpdate.Core.Event` |
| `IUpdateEventListener` | `GeneralUpdate.Core.Event` |
| `IUpdateHooks` | `GeneralUpdate.Core.Hooks` |
| `IStrategy` | `GeneralUpdate.Core.Strategy` |
| `PipelineContext`, `PipelineBuilder` | `GeneralUpdate.Core.Pipeline` |
| `DiffPipelineBuilder` | `GeneralUpdate.Core.Pipeline` |
| `DownloadProgress`, `DownloadAsset` | `GeneralUpdate.Core.Download.Models` |

## 框架兼容性矩阵

| 框架 | 最低 SDK 版本 | SignalR 支持 |
|------|:------------:|:-----------:|
| WPF (Windows) | .NET 8 (`net8.0-windows`) | ✅ |
| WinForms (Windows) | .NET 8 (`net8.0-windows`) | ✅ |
| Avalonia | .NET 8 | ✅ |
| MAUI | .NET 10 | ❌ |
| 控制台 | .NET 8 | ✅ |
| Linux 桌面 | .NET 8 | ❌ |
| macOS 桌面 | .NET 8 | ❌ |

## 常见问题快速链接

| 问题 | 参考文件 |
|------|---------|
| "Method not found" NuGet 版本冲突 | `/generalupdate-troubleshoot` |
| 静默模式不生效 | `/generalupdate-troubleshoot` |
| 升级进程没启动 | `/generalupdate-troubleshoot` |
| Linux 下无权限 | `/generalupdate-troubleshoot` |
