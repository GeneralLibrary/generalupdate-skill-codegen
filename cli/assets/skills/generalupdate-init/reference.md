# GeneralUpdate 参考手册

> ⚠️ **针对 NuGet v10.4.6 稳定版 API**。该版本使用 `Configinfo` 配置，无可编程 `Option` 系统。

## NuGet 包

所有包的最新版本：[nuget.org/profiles/GeneralLibrary](https://www.nuget.org/profiles/GeneralLibrary)

| 包名 | 用途 | 必需 | .NET 版本 | 典型版本 |
|------|------|------|-----------|---------|
| `GeneralUpdate.Core` | 核心引擎（Bootstrap/下载/事件） | ✅ 是 | net8.0;net10.0 | 10.4.6 |
| `GeneralUpdate.Differential` | BSDIFF/HDiffPatch 差分补丁 | ❌ 可选 | net8.0;net10.0 | 10.4.6 |
| `GeneralUpdate.Bowl` | 进程崩溃监控、MiniDump、自动回滚 | ❌ 可选 | net8.0;net10.0 | 10.4.6 |
| `GeneralUpdate.Extension` | 插件管理系统 | ❌ 可选 | net8.0;net10.0 | ≥ 1.0.0 |
| `GeneralUpdate.Drivelution` | Windows 驱动更新 | ❌ 可选 | net8.0;net10.0 | 10.4.6 |

> ⚠️ **NuGet 类型冲突**：
> - `GeneralUpdate.Differential` 的 `DifferentialCore` 等类型已内嵌在 `GeneralUpdate.Core` 中，**不需额外引用**（直接使用 Core 即可）
> - `GeneralUpdate.Bowl` 和 `GeneralUpdate.Core` **不能同时引用**（两者都发布了 `GeneralUpdate.Common` 导致 CS0433）
> - 使用 Bowl 时**只引用 `GeneralUpdate.Bowl`**（它传递依赖 Core 的所有功能）

## Configinfo 字段完整说明

| 字段 | 类型 | 必需 | 说明 |
|------|------|:----:|------|
| `UpdateUrl` | string | ✅ | 版本验证 API 地址 |
| `AppSecretKey` | string | ✅ | HMAC 认证 + IPC 加密密钥 |
| `AppName` | string | ✅ | 当前应用进程名（含 .exe） |
| `MainAppName` | string | ✅ | 主程序文件名（含 .exe） |
| `ClientVersion` | string | ✅ | 当前版本号（4 段式） |
| `ProductId` | string | ✅ | 产品标识 |
| `InstallPath` | string | ✅ | 应用安装目录 |
| `UpgradeClientVersion` | string | ❌ | Upgrade 进程版本号 |
| `ReportUrl` | string | ❌ | 状态上报 API 地址 |
| `UpdateLogUrl` | string | ❌ | 更新日志 API 地址 |
| `Scheme` | string | ❌ | HTTP 认证方案（Bearer/Basic 等） |
| `Token` | string | ❌ | HTTP 认证令牌 |
| `BlackFiles` | List\<string\> | ❌ | 备份排除文件名模式 |
| `BlackFormats` | List\<string\> | ❌ | 备份排除扩展名 |
| `SkipDirectorys` | List\<string\> | ❌ | 备份排除目录名 |
| `Bowl` | string | ❌ | Bowl 配置参数 |

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
| `MultiDownloadCompletedEventArgs` | `Version` (object), `IsComplated` (bool) | 版本下载完成 |
| `MultiDownloadErrorEventArgs` | `Exception`, `Version` (object) | 下载错误 |
| `MultiAllDownloadCompletedEventArgs` | `IsAllDownloadCompleted` (bool), `FailedVersions` (IList) | 全部完成 |
| `ExceptionEventArgs` | `Exception`, `Message` | 异常 |

## AppType 类

```
AppType.ClientApp = 1      // 标准客户端
AppType.UpgradeApp = 2     // 标准升级程序
```

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
