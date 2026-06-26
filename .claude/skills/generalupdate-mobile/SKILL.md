---
name: generalupdate-mobile
description: |
  Integrate GeneralUpdate mobile auto-update into Avalonia.Android and .NET MAUI Android apps.
  Covers both GeneralUpdate.Avalonia.Android (3-step API) and GeneralUpdate.Maui.Android
  (2-step API + DI). Generates complete integration code including AndroidManifest FileProvider
  configuration, Bootstrap setup, version check, download+verify, APK install, and event wiring
  for progress/error handling. Auto-detects project type from .csproj.
  Triggers on: "GeneralUpdate.Avalonia", "GeneralUpdate.Maui", "Avalonia update",
  "MAUI update", "Android update", "移动端更新", "安卓更新", "APK update",
  "Avalonia自动更新", "MAUI自动更新", "Android自动更新", "mobile auto update",
  "Avalonia Android update", "MAUI Android update", "dotnet add package GeneralUpdate.Avalonia",
  "dotnet add package GeneralUpdate.Maui", "集成移动端更新", "接入安卓自动更新",
  "Avalonia更新", "MAUI更新".
  Also triggers when user mentions their Avalonia or MAUI Android project + auto-update.
when_to_use: |
  - User has an Avalonia.Android or .NET MAUI Android app and wants auto-update
  - User asks how to add auto-update to their mobile .NET app
  - User asks about APK download, verification, and install in Android
  - User needs FileProvider configuration for APK installation
  - User wants progress/error event wiring for mobile update UI
  - User needs to choose between Avalonia.Android vs MAUI.Android NuGet packages
  - User asks about DI integration for update services in MAUI
allowed-tools: "Bash, Read, Write, Edit, Glob, Grep"
---

# 📱 GeneralUpdate Mobile — Avalonia & MAUI Android Auto-Update Integration / Avalonia 与 MAUI Android 自动更新集成

Integrate auto-update into Avalonia.Android or .NET MAUI Android apps. Covers the full pipeline: NuGet installation, AndroidManifest FileProvider setup, version check, APK download with SHA256 verification, installer launch, and progress/error event wiring.

将自动更新集成到 Avalonia.Android 或 .NET MAUI Android 应用中。覆盖完整流水线：NuGet 安装、AndroidManifest FileProvider 配置、版本检查、带 SHA256 校验的 APK 下载、安装器启动以及进度/错误事件接入。

> ⚠️ Both mobile libraries are **Android-only** (`net10.0-android`), **UI-free** — the host app controls all progress display.  
> NuGet packages: `GeneralUpdate.Avalonia.Android` / `GeneralUpdate.Maui.Android`
>
> ⚠️ 两个移动端库均为 **仅限 Android**（`net10.0-android`），**无 UI** —— 宿主应用自行控制所有进度展示。  
> NuGet 包：`GeneralUpdate.Avalonia.Android` / `GeneralUpdate.Maui.Android`

---

## 📋 Requirements Extraction / 用户需求提取

Before generating code, extract the following. **Ask the user when anything is unclear:**

在生成代码之前，提取以下信息。**如有不明确之处，请向用户确认：**

```
### Project Status (required) / 项目状态（必需）
- Mobile framework / 移动端框架: ______ (Avalonia.Android / .NET MAUI Android / unsure 不确定)
- .csproj TargetFramework: ______ (net10.0-android / net9.0-android / other 其他)
- Version retrieval method / 当前版本号获取方式: ______ (Assembly / config / hardcoded 硬编码)

### Update Server (required) / 更新服务端（必需）
- Version info API URL / 版本信息接口: ______ (endpoint returning UpdatePackageInfo-compatible JSON / 返回 UpdatePackageInfo 兼容 JSON 的接口)
- Auth method / 认证方式: ______ (None 无 / HMAC / Bearer Token / API Key / Basic Auth)
- APK download URL source / APK 下载地址来源: ______ (server returns direct URL 服务器直接返回 / CDN / OSS)

### Update Strategy (required) / 更新策略（必需）
- Check timing / 检查时机: ______ (App startup 应用启动 / user-triggered 用户触发 / periodic polling 定时轮询)
- Force update? / 是否强制更新: ______ (Yes 是 / No 否, based on server's IsForced / ForceUpdate field 根据服务器返回的 IsForced / ForceUpdate 字段)
- Silent download? / 是否静默下载: ______ (Yes 是 / No 否)

### UI Requirements (optional) / UI 需求（可选）
- Show progress? / 是否需要进度展示: ______ (Yes 是 / No 否 — dialog 弹窗 / status bar 状态栏 / custom 自定义)
- Error handling / 错误处理策略: ______ (Toast / dialog with retry 带重试的弹窗 / silent ignore 静默忽略)
```

---

## Workflow / 工作流

```
1. Framework Detection / 框架检测
   ├── Scan .csproj → PackageReference identifies Avalonia vs Microsoft.Maui
   │   扫描 .csproj → 通过 PackageReference 识别 Avalonia 还是 Microsoft.Maui
   ├── Check TargetFramework → confirm it's netX.X-android
   │   检查 TargetFramework → 确认是 netX.X-android
   └── If undetected → ask the user
        如果无法检测 → 询问用户

2. Requirements extraction / 需求提取 (see template above 参见上方模板)

3. Generate integration code / 生成集成代码
   ├── NuGet install command / NuGet 安装命令 (Avalonia or MAUI / Avalonia 或 MAUI)
   ├── AndroidManifest.xml FileProvider configuration / FileProvider 配置
   ├── Bootstrap creation code / Bootstrap 创建代码 (Minimal 最简 / Standard 标准)
   ├── Version check + download + install orchestration / 版本检查 + 下载 + 安装编排
   ├── Event wiring / 事件接入 (progress + completion + failure 进度 + 完成 + 失败)
   └── Server API contract reference / 服务端 API 契约参考

4. Deployment verification checklist / 部署验证清单
   └── Build → AndroidManifest → FileProvider → permissions → test
        构建 → AndroidManifest → FileProvider → 权限 → 测试
```

---

## API Comparison: Avalonia vs MAUI / API 对比：Avalonia 与 MAUI

| Feature | GeneralUpdate.Avalonia.Android | GeneralUpdate.Maui.Android |
|---------|------|------|
| NuGet Package | `GeneralUpdate.Avalonia.Android` | `GeneralUpdate.Maui.Android` |
| Namespaces | `GeneralUpdate.Avalonia.Android` | `GeneralUpdate.Maui.Android.Services` / `.Models` |
| Entry Point | `GeneralUpdateBootstrap` (static 静态) | `GeneralUpdateBootstrap` (static 静态) |
| Return Interface | `IAndroidBootstrap : IDisposable` | `IAndroidBootstrap : IDisposable` |
| Config Object | `AndroidUpdateOptions` (record) | `UpdateOptions` (class) |
| Package Info | `UpdatePackageInfo` (record) | `UpdatePackageInfo` (class) |
| API Steps | 3 steps: `ValidateAsync` → `DownloadAndVerifyAsync` → `LaunchInstallerAsync` | 2 steps: `ValidateAsync` → `ExecuteUpdateAsync` |
| Version Passing | `ValidateAsync(packageInfo, "1.0.0", ct)` — separate param 独立参数 | `UpdateOptions.CurrentVersion` — embedded in options 嵌入到 options 中 |
| DI Support | ❌ | ✅ `AddGeneralUpdateMauiAndroid(IServiceCollection)` |
| Download API | `IUpdateDownloader.DownloadAsync(packageInfo, callback, ct)` | `IUpdateDownloader.DownloadAsync(packageInfo, target, temp, interval, progress, ct)` |
| Events | 4 events (Validate / DownloadProgressChanged / UpdateCompleted / UpdateFailed) | 4 events (same pattern 同样模式) |
| Logger | `IUpdateLogger` (custom 自定义) | `IUpdateLogger` (default 默认: `NullUpdateLogger`) |

---

## Core Concept: Update State Machine / 核心概念：更新状态机

Both libraries share the same update lifecycle (naming differs slightly):

两个库共享相同的更新生命周期（命名略有不同）：

```
None → Checking → UpdateAvailable → Downloading → Verifying
  → ReadyToInstall → Installing → Completed
  Any node → Failed / Canceled
  任意节点 → Failed 失败 / Canceled 取消
```

### Avalonia.Android Event Mapping / 事件映射

```csharp
bootstrap.AddListenerValidate         // → UpdateAvailable — version check passed 版本检查通过
bootstrap.AddListenerDownloadProgressChanged  // → Downloading — progress updates 进度更新
bootstrap.AddListenerUpdateCompleted  // → ReadyToInstall / Installing — download done or install triggered 下载完成或安装触发
bootstrap.AddListenerUpdateFailed     // → Failed/Canceled — any failure 任何失败
bootstrap.GetSnapshot()               // → Query current state (state + reason + message) 查询当前状态（状态 + 原因 + 消息）
```

### MAUI.Android Event Mapping / 事件映射

```csharp
bootstrap.AddListenerValidate         // → UpdateAvailable
bootstrap.AddListenerDownloadProgressChanged  // → Downloading — progress updates 进度更新
bootstrap.AddListenerUpdateCompleted  // → DownloadCompleted / VerificationCompleted / InstallationTriggered / WorkflowCompleted
bootstrap.AddListenerUpdateFailed     // → Failed/Canceled
bootstrap.CurrentState                // → Read-only property for current state 只读属性获取当前状态
```

---

## Integration Code Generation / 集成代码生成

### Step 1: Install NuGet / 安装 NuGet

```bash
# Avalonia project / Avalonia 项目
dotnet add package GeneralUpdate.Avalonia.Android

# MAUI project / MAUI 项目
dotnet add package GeneralUpdate.Maui.Android
```

### Step 2: AndroidManifest.xml FileProvider Configuration / FileProvider 配置

**Required for both libraries** (必须为两个库都配置), otherwise the APK installer cannot launch (否则 APK 安装器无法启动):

```xml
<!-- Platforms/Android/AndroidManifest.xml or Properties/AndroidManifest.xml -->
<manifest xmlns:android="http://schemas.android.com/apk/res/android">
  <application>
    <!-- FileProvider: authority MUST match code's FileProviderAuthority -->
    <!-- FileProvider：authority 必须与代码中的 FileProviderAuthority 完全一致 -->
    <provider
      android:name="androidx.core.content.FileProvider"
      android:authorities="{applicationId}.generalupdate.fileprovider"
      android:exported="false"
      android:grantUriPermissions="true">
      <meta-data
        android:name="android.support.FILE_PROVIDER_PATHS"
        android:resource="@xml/generalupdate_file_paths" />
    </provider>
  </application>
</manifest>
```

```xml
<!-- Resources/xml/generalupdate_file_paths.xml (Avalonia) -->
<!-- or/或 Platforms/Android/Resources/xml/generalupdate_file_paths.xml (MAUI) -->
<paths xmlns:android="http://schemas.android.com/apk/res/android">
  <cache-path name="generalupdate_apks" path="update/" />
  <external-files-path name="generalupdate_external" path="update/" />
</paths>
```

> ⚠️ The `android:authorities` value must **exactly match** the `FileProviderAuthority` configured in code.
>
> ⚠️ `android:authorities` 的值必须与代码中配置的 `FileProviderAuthority` **完全一致**。

### Step 3: Bootstrap Setup + Update Orchestration / Bootstrap 设置 + 更新编排

#### Avalonia.Android — Minimal / 最简模式 (3-step manual / 3 步手动)

```csharp
using GeneralUpdate.Avalonia.Android;
using GeneralUpdate.Avalonia.Android.Models;

public async Task CheckAndUpdateAsync(CancellationToken ct = default)
{
    // 1. Create Bootstrap / 创建 Bootstrap
    var cacheDir = Android.App.Application.Context.CacheDir?.AbsolutePath
        ?? Path.GetTempPath();

    var options = new AndroidUpdateOptions
    {
        DownloadDirectoryPath = Path.Combine(cacheDir, "update"),
        FileProviderAuthority = "com.example.app.generalupdate.fileprovider"
    };

    using var bootstrap = GeneralUpdateBootstrap.CreateDefault(options);

    // 2. Build package info (fetch from your server API)
    //    构建包信息（从你的服务端 API 获取）
    var packageInfo = new UpdatePackageInfo
    {
        Version = "2.3.0",                     // target version from server 从服务端获取的目标版本
        DownloadUrl = "https://cdn.example.com/app-release.apk",
        Sha256 = "abc123...",                  // SHA256 from server 从服务端获取的 SHA256
        FileSize = 50_000_000,                 // APK file size in bytes APK 文件大小（字节）
        IsForced = false                       // force-update flag 强制更新标志
    };

    // 3. Version check / 版本检查
    var check = await bootstrap.ValidateAsync(packageInfo, "1.0.0", ct);
    if (!check.UpdateFound)
    {
        // No update available, proceed to main app flow
        // 无可用更新，继续主应用流程
        return;
    }

    // 4. Download + SHA256 verification / 下载 + SHA256 校验
    var prepared = await bootstrap.DownloadAndVerifyAsync(packageInfo, ct);
    if (!prepared.Success || prepared.FilePath is null)
    {
        // Download or verification failed — check prepared.FailureReason
        // 下载或校验失败 — 检查 prepared.FailureReason
        return;
    }

    // 5. Launch system package installer / 启动系统安装器
    await bootstrap.LaunchInstallerAsync(packageInfo, prepared.FilePath, ct);
}
```

#### Avalonia.Android — Standard / 标准模式 (with event listeners / 带事件监听)

```csharp
using GeneralUpdate.Avalonia.Android;
using GeneralUpdate.Avalonia.Android.Models;

public async Task CheckAndUpdateWithEventsAsync(CancellationToken ct = default)
{
    var cacheDir = Android.App.Application.Context.CacheDir?.AbsolutePath
        ?? Path.GetTempPath();

    var options = new AndroidUpdateOptions
    {
        DownloadDirectoryPath = Path.Combine(cacheDir, "update"),
        FileProviderAuthority = "com.example.app.generalupdate.fileprovider"
    };

    using var bootstrap = GeneralUpdateBootstrap.CreateDefault(options);

    // === Register events BEFORE ValidateAsync / 在 ValidateAsync 之前注册事件 ===

    // Version check passed, new version found / 版本检查通过，发现新版本
    bootstrap.AddListenerValidate += (_, e) =>
    {
        Console.WriteLine($"New version found / 发现新版本: {e.PackageInfo.Version}");
        // Prompt user to update (dialog / snackbar) / 提示用户更新（弹窗 / snackbar）
    };

    // Download progress (rate controlled by downloader) / 下载进度（由下载器控制速率）
    bootstrap.AddListenerDownloadProgressChanged += (_, e) =>
    {
        var p = e.Progress;
        Console.WriteLine($"Downloading / 正在下载: {p.ProgressPercentage:F1}% " +
            $"{p.DownloadedBytes}/{p.TotalBytes} " +
            $"{p.DownloadSpeedBytesPerSecond / 1024.0:F0} KB/s");
        // Update UI progress bar / 更新 UI 进度条
    };

    // Download completed / verification completed / install triggered
    // 下载完成 / 校验完成 / 安装触发
    bootstrap.AddListenerUpdateCompleted += (_, e) =>
    {
        Console.WriteLine($"Completed / 已完成: {e.Result.State} — {e.Result.Message}");
    };

    // Download failed / verification failed / install failed
    // 下载失败 / 校验失败 / 安装失败
    bootstrap.AddListenerUpdateFailed += (_, e) =>
    {
        Console.WriteLine($"Failed / 失败: [{e.Result.FailureReason}] {e.Result.Message}");
        // Show dialog, allow retry / 弹出对话框，允许重试
    };

    // === Execute update (same as Minimal) / 执行更新（与最简模式相同） ===
    var packageInfo = new UpdatePackageInfo
    {
        Version = "2.3.0",
        DownloadUrl = "https://cdn.example.com/app-release.apk",
        Sha256 = "abc123...",
        FileSize = 50_000_000
    };

    var check = await bootstrap.ValidateAsync(packageInfo, "1.0.0", ct);
    if (!check.UpdateFound) return;

    var prepared = await bootstrap.DownloadAndVerifyAsync(packageInfo, ct);
    if (!prepared.Success || prepared.FilePath is null) return;

    await bootstrap.LaunchInstallerAsync(packageInfo, prepared.FilePath, ct);
}
```

#### MAUI.Android — Minimal / 最简模式 (2-step / 2 步)

```csharp
using GeneralUpdate.Maui.Android.Models;
using GeneralUpdate.Maui.Android.Services;

public async Task CheckAndUpdateAsync(CancellationToken ct = default)
{
    // 1. Create Bootstrap (parameterless factory) / 创建 Bootstrap（无参工厂）
    var bootstrap = GeneralUpdateBootstrap.CreateDefault();

    // 2. Build package info & options / 构建包信息和选项
    var package = new UpdatePackageInfo
    {
        Version = "2.3.0",
        VersionName = "v2.3.0",
        ReleaseNotes = "Bug fixes and stability improvements. / 错误修复和稳定性提升。",
        DownloadUrl = "https://cdn.example.com/app-release.apk",
        Sha256 = "abc123...",
        PackageSize = 50_000_000
    };

    var options = new UpdateOptions
    {
        CurrentVersion = "1.0.0",
        InstallOptions = new AndroidInstallOptions
        {
            FileProviderAuthority = "com.example.app.generalupdate.fileprovider"
        }
    };

    // 3. Version check / 版本检查
    var check = await bootstrap.ValidateAsync(package, options, ct);
    if (!check.IsUpdateAvailable) return;

    // 4. Download + verify + install (single call) / 下载 + 校验 + 安装（单次调用）
    var result = await bootstrap.ExecuteUpdateAsync(package, options, ct);
    if (!result.IsSuccess)
    {
        Console.WriteLine($"Update failed / 更新失败: [{result.FailureReason}] {result.Message}");
    }
}
```

#### MAUI.Android — Standard / 标准模式 (with events / 带事件)

```csharp
using GeneralUpdate.Maui.Android.Models;
using GeneralUpdate.Maui.Android.Services;

public async Task CheckAndUpdateWithEventsAsync(CancellationToken ct = default)
{
    var bootstrap = GeneralUpdateBootstrap.CreateDefault();

    // === Register events / 注册事件 ===
    bootstrap.AddListenerValidate += (_, e) =>
    {
        Console.WriteLine($"New version found / 发现新版本: {e.PackageInfo.Version}");
    };

    bootstrap.AddListenerDownloadProgressChanged += (_, e) =>
    {
        var s = e.Statistics;
        Console.WriteLine($"Downloading / 正在下载: {s.ProgressPercentage:F1}% " +
            $"{s.BytesPerSecond / 1024.0:F0} KB/s");
    };

    bootstrap.AddListenerUpdateCompleted += (_, e) =>
    {
        Console.WriteLine($"Stage complete / 阶段完成: {e.Stage} — {e.Message}");
    };

    bootstrap.AddListenerUpdateFailed += (_, e) =>
    {
        Console.WriteLine($"Failed / 失败: [{e.Reason}] {e.Message}");
    };

    // === Execute / 执行 ===
    var package = new UpdatePackageInfo
    {
        Version = "2.3.0",
        DownloadUrl = "https://cdn.example.com/app-release.apk",
        Sha256 = "abc123...",
        PackageSize = 50_000_000
    };

    var options = new UpdateOptions
    {
        CurrentVersion = "1.0.0",
        InstallOptions = new AndroidInstallOptions
        {
            FileProviderAuthority = "com.example.app.generalupdate.fileprovider"
        }
    };

    var check = await bootstrap.ValidateAsync(package, options, ct);
    if (!check.IsUpdateAvailable) return;

    await bootstrap.ExecuteUpdateAsync(package, options, ct);
}
```

#### MAUI.Android — DI approach / DI 方式 (recommended for larger projects / 推荐用于较大项目)

```csharp
// === MauiProgram.cs ===
using GeneralUpdate.Maui.Android.Abstractions;
using GeneralUpdate.Maui.Android.Services;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Register GeneralUpdate mobile update services
        // 注册 GeneralUpdate 移动端更新服务
        builder.Services.AddGeneralUpdateMauiAndroid();

        return builder.Build();
    }
}

// === ViewModel / Page ===
public class UpdateViewModel
{
    private readonly IAndroidBootstrap _bootstrap;

    public UpdateViewModel(IAndroidBootstrap bootstrap)
    {
        _bootstrap = bootstrap;
    }

    public async Task CheckAsync(CancellationToken ct)
    {
        var options = new UpdateOptions
        {
            CurrentVersion = "1.0.0",
            InstallOptions = new AndroidInstallOptions
            {
                FileProviderAuthority = "com.example.app.generalupdate.fileprovider"
            }
        };

        var package = new UpdatePackageInfo { /* fetch from server / 从服务端获取 */ };
        var check = await _bootstrap.ValidateAsync(package, options, ct);
        if (check.IsUpdateAvailable)
        {
            await _bootstrap.ExecuteUpdateAsync(package, options, ct);
        }
    }
}
```

---

## Server API Contract / 服务端 API 契约

Both libraries expect a server endpoint that returns version info matching `UpdatePackageInfo`:

两个库都期望服务端接口返回与 `UpdatePackageInfo` 匹配的版本信息：

```json
// GET /api/update/{productId}/{currentVersion}
// or/或 POST /api/update/check

{
  "version": "2.3.0",
  "versionName": "v2.3.0",
  "description": "Bug fixes and stability improvements. / 错误修复和稳定性提升。",
  "downloadUrl": "https://cdn.example.com/app-release.apk",
  "size": 52428800,
  "sha256": "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
  "publishTime": "2026-06-15T10:00:00Z",
  "isForced": false,
  "fileName": "app-release.apk"
}
```

| JSON Field | Mapped Property | Notes |
|------------|----------------|-------|
| `version` | `UpdatePackageInfo.Version` | **Required 必需** — 4-segment version `x.y.z.w` / 4 段版本号 |
| `versionName` | `UpdatePackageInfo.VersionName` | Optional 可选 — display name 显示名称 |
| `description` | `UpdatePackageInfo.Description` / `ReleaseNotes` | Optional 可选 — release notes 更新说明 |
| `downloadUrl` | `UpdatePackageInfo.DownloadUrl` | **Required 必需** — direct APK download URL / APK 直接下载地址 |
| `size` | `UpdatePackageInfo.FileSize` / `PackageSize` | **Required 必需** — used for size validation / 用于大小校验 |
| `sha256` | `UpdatePackageInfo.Sha256` | **Required 必需** — SHA256 hash (lowercase hex) / SHA256 哈希（小写十六进制） |
| `isForced` | `UpdatePackageInfo.IsForced` / `ForceUpdate` | Optional 可选 — force-update flag / 强制更新标志 |

---

## Advanced: HTTP Auth & SSL / 高级：HTTP 认证与 SSL

### Avalonia.Android

```csharp
using GeneralUpdate.Avalonia.Android.Models;
using GeneralUpdate.Avalonia.Android.Services;

// === Bearer Token ===
var httpOptions = new HttpDownloadOptions
{
    AuthProvider = new BearerTokenAuthProvider("your-jwt-token"),
    RequestTimeout = TimeSpan.FromSeconds(30),     // per-request timeout / 单次请求超时
    DownloadTimeout = TimeSpan.FromMinutes(10),    // overall download timeout / 整体下载超时
    MaxRetryAttempts = 3,                          // max retry count / 最大重试次数
    RetryBaseDelay = TimeSpan.FromSeconds(1)       // backoff base interval / 退避基准间隔
};

using var bootstrap = GeneralUpdateBootstrap.CreateDefault(
    androidUpdateOptions,
    httpOptions: httpOptions);

// === API Key ===
var apiKeyOptions = new HttpDownloadOptions
{
    AuthProvider = new ApiKeyAuthProvider("your-api-key")
};

// === HMAC Signature / HMAC 签名 ===
var hmacOptions = new HttpDownloadOptions
{
    AuthProvider = new HmacAuthProvider("your-secret-key")
};

// === HTTP Basic ===
var basicOptions = new HttpDownloadOptions
{
    AuthProvider = BasicAuthProvider.FromCredentials("username", "password")
};

// === Or use the factory (auto-create from AuthScheme enum) / 或使用工厂（从 AuthScheme 枚举自动创建） ===
var httpOptions = new HttpDownloadOptions
{
    AuthProvider = HttpAuthProviderFactory.Create(
        AuthScheme.Hmac,
        secretKey: "your-hmac-key")
};

// === Self-signed cert (development only!) / 自签名证书（仅限开发环境！） ===
var devOptions = new HttpDownloadOptions
{
    SslValidationPolicy = new AllowAllSslValidationPolicy()
};

// === Proxy / 代理 ===
var proxyOptions = new HttpDownloadOptions
{
    Proxy = new System.Net.WebProxy("http://proxy.example.com:8080"),
    UseProxy = true
};
```

### MAUI.Android

```csharp
using GeneralUpdate.Maui.Android.Models;
using GeneralUpdate.Maui.Android.Services;

// === Bearer Token ===
var httpOptions = new HttpDownloadOptions
{
    AuthProvider = new BearerTokenAuthProvider("your-jwt-token"),
    DownloadTimeout = TimeSpan.FromMinutes(10)     // overall download timeout / 整体下载超时
};

var bootstrap = GeneralUpdateBootstrap.CreateDefault(
    httpOptions: httpOptions);

// === API Key ===
var apiKeyOptions = new HttpDownloadOptions
{
    AuthProvider = new ApiKeyAuthProvider("your-api-key")
};

// === HMAC Signature / HMAC 签名 ===
var hmacOptions = new HttpDownloadOptions
{
    AuthProvider = new HmacAuthProvider("your-secret-key")
};

// === HTTP Basic ===
var basicOptions = new HttpDownloadOptions
{
    AuthProvider = BasicAuthProvider.FromCredentials("username", "password")
};

// === Or use the factory (auto-create from AuthScheme enum) / 或使用工厂（从 AuthScheme 枚举自动创建） ===
var httpOptions = new HttpDownloadOptions
{
    AuthProvider = HttpAuthProviderFactory.Create(
        AuthScheme.Hmac,
        secretKey: "your-hmac-key")
};

// === Self-signed cert (development only!) / 自签名证书（仅限开发环境！） ===
var devOptions = new HttpDownloadOptions
{
    SslValidationPolicy = new AllowAllSslValidationPolicy()
};
```

### Per-Package Auth / 按包认证 (both libraries / 两个库均支持)

Per-package credentials take priority over the global `HttpDownloadOptions.AuthProvider` when `UpdatePackageInfo.AuthScheme` is explicitly set:

当 `UpdatePackageInfo.AuthScheme` 被显式设置时，按包凭证优先于全局的 `HttpDownloadOptions.AuthProvider`：

```csharp
var packageInfo = new UpdatePackageInfo
{
    Version = "2.3.0",
    DownloadUrl = "https://private-cdn.example.com/app.apk",
    Sha256 = "abc123...",
    // Per-package Basic auth (overrides global HttpDownloadOptions)
    // 按包的 Basic 认证（覆盖全局 HttpDownloadOptions）
    AuthScheme = AuthScheme.Basic,
    BasicUsername = "cdn-user",
    BasicPassword = "cdn-pass"
};
```

---

## Retrieving the Current App Version / 获取当前应用版本

Both libraries need the current app version. Recommended approaches:

两个库都需要当前应用版本。推荐以下方式：

```csharp
// Option A: Assembly attribute (most common) / 程序集属性（最常用）
var currentVersion = System.Reflection.Assembly
    .GetExecutingAssembly()
    .GetCustomAttribute<System.Reflection.AssemblyInformationalVersionAttribute>()
    ?.InformationalVersion
    ?? "1.0.0.0";

// Option B: AppInfo (MAUI built-in / MAUI 内置)
var currentVersion = AppInfo.Current.VersionString;

// Option C: Preferences / config / 首选项 / 配置
var currentVersion = Preferences.Get("app_version", "1.0.0.0");
```

---

## ⚠️ Anti-Patterns / 反模式

| # | Anti-Pattern / 反模式 | Consequence / 后果 | Correct Approach / 正确做法 |
|---|-------------|------------|-----------------|
| 1 | **Mismatched FileProviderAuthority / FileProviderAuthority 不匹配** | Installer launch fails with `InstallLaunchFailed` / 安装器启动失败，抛出 `InstallLaunchFailed` | Use a constant shared between AndroidManifest and code / 使用 AndroidManifest 与代码之间共享的常量 |
| 2 | **Missing xml/file_paths config / 缺少 xml/file_paths 配置** | FileProvider can't find shared paths / FileProvider 找不到共享路径 | Create `generalupdate_file_paths.xml` and reference it in manifest / 创建 `generalupdate_file_paths.xml` 并在 manifest 中引用 |
| 3 | **DownloadUrl inaccessible without auth headers / 没有认证头的 DownloadUrl 无法访问** | HTTP 403/401 on download / 下载时返回 HTTP 403/401 | Use `HttpDownloadOptions.AuthProvider` to inject auth headers / 使用 `HttpDownloadOptions.AuthProvider` 注入认证头 |
| 4 | **Empty SHA256 string / SHA256 为空** | Verification always fails / 校验始终失败 | Server MUST return SHA256; client MUST populate it / 服务端必须返回 SHA256；客户端必须填充它 |
| 5 | **Download directory on external storage / 下载目录在外部存储** | Android 11+ scoped storage restrictions / Android 11+ 分区存储限制 | Use `Context.CacheDir` or `Context.FilesDir` / 使用 `Context.CacheDir` 或 `Context.FilesDir` |
| 6 | **Bootstrap not disposed / Bootstrap 未释放** | Semaphore leak, file handle leak / 信号量泄漏，文件句柄泄漏 | `using var` or call `Dispose()` / `using var` 或调用 `Dispose()` |
| 7 | **Canceling between ValidateAsync and DownloadAndVerifyAsync / 在 ValidateAsync 与 DownloadAndVerifyAsync 之间取消** | Early Dispose → ObjectDisposedException / 提前释放 → ObjectDisposedException | Use the same CancellationToken to control lifecycle / 使用同一个 CancellationToken 控制生命周期 |
| 8 | **Skipping UpdateFound check before download / 下载前跳过 UpdateFound 检查** | Downloads when no update exists / 无更新时也触发下载 | Always check `UpdateFound` / `IsUpdateAvailable` first / 始终先检查 `UpdateFound` / `IsUpdateAvailable` |
| 9 | **Ignoring FilePath null after download failure / 下载失败后忽略 FilePath 为空** | Null passed to LaunchInstallerAsync → NRE / 空值传递给 LaunchInstallerAsync → 空引用异常 | Check `prepared.Success && prepared.FilePath is not null` / 检查 `prepared.Success && prepared.FilePath is not null` |
| 10 | **Mixing MAUI API pattern in Avalonia code (or vice versa) / 在 Avalonia 代码中使用 MAUI API 模式（反之亦然）** | Compile errors / 编译错误 | Avalonia: 3 steps; MAUI: 2 steps — keep separate / Avalonia：3 步；MAUI：2 步 — 保持独立 |
| 11 | **MAUI CurrentVersion left empty / MAUI CurrentVersion 留空** | `ValidateAsync` throws `ArgumentException` / `ValidateAsync` 抛出 `ArgumentException` | `UpdateOptions.CurrentVersion` is required / `UpdateOptions.CurrentVersion` 是必需的 |
| 12 | **Blocking I/O in event callbacks / 在事件回调中阻塞 I/O** | Update pipeline stalls, UI freezes / 更新流水线停滞，UI 冻结 | Only update UI state in events; fire-and-forget async work / 仅在事件中更新 UI 状态；异步工作使用 fire-and-forget |

---

## ✅ Integration Verification Checklist / 集成验证清单

### NuGet & Build / 构建
- [ ] NuGet package installed (`GeneralUpdate.Avalonia.Android` or / 或 `GeneralUpdate.Maui.Android`)
- [ ] `dotnet build` succeeds with 0 errors / `dotnet build` 成功，0 错误
- [ ] TargetFramework is `netX.X-android`

### AndroidManifest
- [ ] `<provider>` element added inside `<application>` / `<provider>` 元素已添加到 `<application>` 中
- [ ] `android:authorities` matches code's `FileProviderAuthority` exactly / `android:authorities` 与代码中的 `FileProviderAuthority` 完全一致
- [ ] `generalupdate_file_paths.xml` created at the correct path / `generalupdate_file_paths.xml` 在正确路径创建
- [ ] `cache-path` `path` attribute matches the last segment of `DownloadDirectoryPath` (e.g., `update/`) / `cache-path` 的 `path` 属性与 `DownloadDirectoryPath` 的最后一段匹配（如 `update/`）

### Code Integration / 代码集成
- [ ] Bootstrap uses `using var` or explicit `Dispose()` / Bootstrap 使用 `using var` 或显式 `Dispose()`
- [ ] Events registered before calling `ValidateAsync` / 事件在调用 `ValidateAsync` 之前注册
- [ ] `ValidateAsync` result checked for `UpdateFound` before downloading / 下载前检查 `ValidateAsync` 结果的 `UpdateFound`
- [ ] `DownloadAndVerifyAsync` result checked for `Success` before installing / 安装前检查 `DownloadAndVerifyAsync` 结果的 `Success`
- [ ] `FileProviderAuthority` matches manifest / `FileProviderAuthority` 与 manifest 一致

### Server / 服务端
- [ ] Version-info endpoint returns non-empty `sha256` / 版本信息接口返回非空 `sha256`
- [ ] `downloadUrl` returns a directly accessible APK (or auth configured via `HttpDownloadOptions.AuthProvider`) / `downloadUrl` 返回可直接访问的 APK（或通过 `HttpDownloadOptions.AuthProvider` 配置认证）
- [ ] `version` uses 4-segment format (e.g., `2.3.0.0`) / `version` 使用 4 段格式（如 `2.3.0.0`）

### Permissions / 权限
- [ ] `INTERNET` permission (usually present by default) / `INTERNET` 权限（通常默认已存在）
- [ ] `REQUEST_INSTALL_PACKAGES` permission (Android 8.0+, required for APK install) / `REQUEST_INSTALL_PACKAGES` 权限（Android 8.0+，APK 安装必需）
- [ ] If `downloadUrl` is HTTP (not HTTPS), add `android:usesCleartextTraffic="true"` to manifest / 如果 `downloadUrl` 是 HTTP（非 HTTPS），在 manifest 中添加 `android:usesCleartextTraffic="true"`

---

## 🔗 Related Skills / 相关技能

- `/generalupdate-init` — Desktop (Windows/Linux/macOS) update integration / 桌面端更新集成
- `/generalupdate-strategy` — Silent background polling strategy / 静默后台轮询策略
- `/generalupdate-troubleshoot` — Runtime update failure diagnosis / 运行时更新故障诊断
- `/generalupdate-ui` — Avalonia/MAUI update progress UI reference / Avalonia/MAUI 更新进度 UI 参考
