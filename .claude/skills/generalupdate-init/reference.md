# GeneralUpdate 参考手册

## NuGet 包版本对照表

下载页面：[nuget.org/profiles/GeneralLibrary](https://www.nuget.org/profiles/GeneralLibrary)

| 包名 | 用途 | 是否必需 | .NET版本要求 |
|------|------|---------|-------------|
| `GeneralUpdate.Core` | 核心引擎（Bootstrap/策略/下载/IPC/事件） | ✅ 是 | .NET 8+ |
| `GeneralUpdate.Differential` | BSDIFF/HDiffPatch差分补丁 | ❌ 可选 | .NET 8+ |
| `GeneralUpdate.Bowl` | 进程崩溃监控、MiniDump、自动回滚 | ❌ 可选 | .NET 8+ |
| `GeneralUpdate.Extension` | 插件管理 | ❌ 可选 | .NET 8+ |
| `GeneralUpdate.Drivelution` | Windows驱动更新 | ❌ 可选 | .NET 8+ |
| `GeneralUpdate.Avalonia.Android` | Avalonia Android更新 | ❌ 可选 | .NET 10+ |
| `GeneralUpdate.Maui.Android` | MAUI Android更新 | ❌ 可选 | .NET 10+ |

## UpdateRequest 字段速查

| 字段 | 类型 | 必需 | 说明 |
|------|------|------|------|
| `UpdateUrl` | string | ✅ | 服务器版本验证 API 地址 |
| `AppSecretKey` | string | ✅ | HMAC 认证密钥 |
| `InstallPath` | string | ⚠️ | 应用安装目录（未设置则从 manifest 读取） |
| `UpdatePath` | string | ⚠️ | Upgrade.exe 所在路径 |
| `ClientVersion` | string | ⚠️ | 当前版本号（4段式） |
| `MainAppName` | string | ⚠️ | 主程序文件名（含.exe） |
| `UpdateAppName` | string | ⚠️ | 升级程序文件名（含.exe） |
| `ProductId` | string | ⚠️ | 产品标识 |
| `ReportUrl` | string | ❌ | 状态上报地址 |
| `BasicUsername` | string | ❌ | Basic 认证用户名 |
| `BasicPassword` | string | ❌ | Basic 认证密码 |

> ⚠️ = 可通过 `generalupdate.manifest.json` 自动发现，非必需手动传入

## Option 选项速查

| Option | 类型 | 默认值 | 说明 |
|--------|------|--------|------|
| `AppType` | AppType | `Client` | 应用角色 |
| `MaxConcurrency` | int | 3 | 并行下载数 |
| `PatchEnabled` | bool | `false` | 启用差分（需Differential包） |
| `BackupEnabled` | bool | `false`（v5+） | 更新前备份 |
| `Silent` | bool | `false` | 静默后台轮询 |
| `SilentPollIntervalMinutes` | int | 60 | 静默轮询间隔 |
| `Format` | CompressionFormat | `Zip` | 压缩格式 |
| `Encoding` | CompressionEncoding | `Default` | 压缩编码 |
| `DownloadTimeout` | int | 60 | 下载超时(秒) |
| `HooksEnabled` | bool | `true` | 启用生命周期Hooks |
| `EnableResume` | bool | `true` | 断点续传 |

## 后端 API 协议

### 版本验证接口

```
POST {UpdateUrl}/Upgrade/Verification
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

### 成功响应

```json
{
  "body": [
    {
      "id": "guid",
      "version": "1.1.0.0",
      "url": "https://storage/packages/v1.1.0.0.zip",
      "hash": "sha256-hex",
      "size": 1048576,
      "name": "update-v1.1.0.0.zip",
      "appType": 0,
      "isCrossVersion": false,
      "fromVersion": null,
      "isForcibly": false,
      "publishDate": "2026-01-01T00:00:00Z",
      "description": "更新说明"
    }
  ],
  "code": 200,
  "message": ""
}
```

### 状态上报接口

```
POST {ReportUrl}
Content-Type: application/json

{
  "recordId": "upgrade-record-guid",
  "type": 1,        // 1=主动拉取, 2=服务端推送
  "status": 0,      // 0=更新中, 1=成功, 2=失败
  "message": ""
}
```

## 支持的 UI 框架

| 框架 | 技术栈 | UI技能支持 |
|------|--------|-----------|
| WPF + LayUI.Wpf | .NET 6+ / .NET 8 | ✅ 生成LayUI风格更新窗口 |
| WPF + WPFDevelopers | .NET 8 | ✅ 生成WPFDevelopers风格 |
| WinForms + AntdUI | .NET 9+ Windows | ✅ 生成AntdUI风格(暗黑模式+本地化) |
| Avalonia + Semi.Ursa | .NET 8 | ✅ 生成SemiUrsa风格(Client+Upgrade双视图) |
| .NET MAUI | .NET 10+ Android | ✅ 生成MAUI更新页面 |
| 原生WPF/WinForms | 任何版本 | ✅ 生成通用更新窗口 |
