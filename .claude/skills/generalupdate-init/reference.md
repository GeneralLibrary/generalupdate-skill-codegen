# GeneralUpdate 参考手册

## NuGet 包

所有包的最新版本：[nuget.org/profiles/GeneralLibrary](https://www.nuget.org/profiles/GeneralLibrary)

| 包名 | 用途 | 必需 | .NET 版本 | 典型版本 |
|------|------|------|-----------|---------|
| `GeneralUpdate.Core` | 核心引擎（Bootstrap/策略/下载/IPC/事件） | ✅ 是 | net8.0;net10.0 | ≥ 5.0.0 |
| `GeneralUpdate.Differential` | BSDIFF/HDiffPatch 差分补丁 | ❌ 可选 | net8.0;net10.0 | ≥ 5.0.0 |
| `GeneralUpdate.Bowl` | 进程崩溃监控、MiniDump、自动回滚 | ❌ 可选 | net8.0;net10.0 | ≥ 1.0.0 |
| `GeneralUpdate.Extension` | 插件管理系统 | ❌ 可选 | net8.0;net10.0 | ≥ 1.0.0 |
| `GeneralUpdate.Drivelution` | Windows 驱动更新 | ❌ 可选 | net8.0;net10.0 | ≥ 1.0.0 |
| `GeneralUpdate.Avalonia.Android` | Avalonia Android 更新 | ❌ 可选 | net10.0-android | ≥ 1.0.0 |
| `GeneralUpdate.Maui.Android` | MAUI Android 更新 | ❌ 可选 | net10.0-android | ≥ 1.0.0 |

## UpdateRequest 字段完整说明

| 字段 | 类型 | 必需 | 默认 | 说明 |
|------|------|:----:|------|------|
| `UpdateUrl` | string | ✅ | — | 版本验证 API 地址（必填） |
| `AppSecretKey` | string | ✅ | — | HMAC 认证 + IPC 加密密钥（必填） |
| `InstallPath` | string | ⚠️ | manifest | 应用安装目录 |
| `UpdatePath` | string | ⚠️ | manifest | Upgrade.exe 所在目录 |
| `ClientVersion` | string | ⚠️ | manifest | 当前版本号（4段式） |
| `MainAppName` | string | ⚠️ | manifest | 主程序文件名（含.exe） |
| `UpdateAppName` | string | ⚠️ | manifest | 升级程序文件名（含.exe） |
| `ProductId` | string | ⚠️ | manifest | 产品标识 |
| `ReportUrl` | string | ❌ | — | 状态上报 API 地址 |
| `UpdateLogUrl` | string | ❌ | — | 更新日志 API 地址 |
| `BasicUsername` | string | ❌ | — | Basic Auth 用户名 |
| `BasicPassword` | string | ❌ | — | Basic Auth 密码 |

> ⚠️ = 可通过 `generalupdate.manifest.json` 自动发现

## Option 选项完整列表

| Option | 类型 | 默认值 | 说明 |
|--------|------|--------|------|
| `AppType` | AppType | Client | 应用角色 |
| `MaxConcurrency` | int | 3 | 并行下载数 |
| `PatchEnabled` | bool | false | 启用差分更新（需安装 Differential 包） |
| `BackupEnabled` | bool | false (v5+) | 更新前备份 |
| `Silent` | bool | false | 静默后台轮询 |
| `SilentPollIntervalMinutes` | int | 60 | 静默轮询间隔（分钟） |
| `Format` | CompressionFormat | Zip | 压缩格式 |
| `Encoding` | CompressionEncoding | Default | 压缩编码 |
| `DownloadTimeout` | int | 60 | HTTP 下载超时（秒） |
| `MaxRetryCount` | int | 3 | 下载重试次数 |
| `EnableResume` | bool | true | 断点续传 |
| `VerifyChecksum` | bool | true | 下载后 SHA256 校验 |
| `AutoCleanTemp` | bool | false | 自动清理临时目录 |
| `HooksEnabled` | bool | true | 启用生命周期 Hooks |

## 后端 API 协议

### 版本验证

```
POST {UpdateUrl}
Content-Type: application/json

{
  "appKey": "client-app-key",
  "appType": 0,
  "clientVersion": "1.0.0.0",
  "productId": "my-product-001",
  "platform": "Windows",
  "tenantId": "default"
}
```

成功响应：
```json
{
  "code": 200,
  "message": "",
  "body": [
    {
      "id": "guid",
      "version": "1.1.0.0",
      "url": "https://storage/packages/v1.1.0.0.zip",
      "hash": "sha256-hex",
      "size": 1048576,
      "name": "update.zip",
      "appType": 0,
      "isCrossVersion": false,
      "fromVersion": null,
      "isForcibly": false,
      "publishDate": "2026-01-01T00:00:00Z",
      "description": "修复了若干 Bug"
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

## appsettings.json 模板

```json
{
  "GeneralUpdate": {
    "UpdateUrl": "https://your-server.com/Upgrade/Verification",
    "ReportUrl": "https://your-server.com/Upgrade/Report",
    "AppSecretKey": "your-secret-key-here",
    "InstallPath": "",
    "UpdatePath": "update",
    "ClientVersion": "1.0.0.0",
    "UpgradeClientVersion": "1.0.0.0",
    "MainAppName": "MyApp.exe",
    "UpdateAppName": "UpgradeApp.exe",
    "ProductId": "my-product-001",
    "MaxConcurrency": 3,
    "PatchEnabled": false,
    "BackupEnabled": true,
    "DownloadTimeout": 60
  }
}
```

## 框架兼容性矩阵

| 框架 | 最低 SDK 版本 | AOT 兼容 | SignalR 支持 |
|------|:------------:|:---------:|:-----------:|
| WPF (Windows) | .NET 8 (`net8.0-windows`) | ✅ | ✅ (JSON协议) |
| WinForms (Windows) | .NET 8 (`net8.0-windows`) | ✅ | ✅ |
| Avalonia | .NET 8 | ✅ | ✅ |
| MAUI | .NET 10 | ✅ | ❌ (无 SignalR) |
| 控制台 | .NET 8 | ✅ | ✅ |
| Linux 桌面 | .NET 8 | ✅ | ❌ |
| macOS 桌面 | .NET 8 | ✅ | ❌ |

## 常见问题快速链接

| 问题 | 参考文件 |
|------|---------|
| "Method not found" NuGet 版本冲突 | `/generalupdate-troubleshoot` Q2 |
| 静默模式不生效 | `/generalupdate-troubleshoot` Q7 |
| 版本号更新后错误 | `/generalupdate-troubleshoot` Q13 |
| Linux 下无权限 | `/generalupdate-troubleshoot` Q17 |
| 升级进程没启动 | `/generalupdate-troubleshoot` Q1 |
| IPC 加密文件问题 | `/generalupdate-troubleshoot` Q5 |
