# GeneralUpdate 故障排查参考手册

> 本清单基于 GitHub Issues（#308–#517）和 Gitee Issues（30个）的真实问题整理

---

## 🟥 安装与配置问题

### Q1: 升级进程没启动 / "FileNotFoundException: upgrade application not found"

| 项目 | 内容 |
|------|------|
| **来源** | GitHub #485、Gitee #ID3H5V |
| **根因** | UpgradeApp.exe 未随主程序一起发布，或路径配置错误 |
| **诊断** | 检查 `UpdatePath` + `UpdateAppName` 指向的文件是否存在 |
| **修复** | |
| | 1. 确认 UpgradeApp.exe 已放置在 `UpdatePath` 目录 |
| | 2. OSS 模式下 Upgrade.exe 必须放在 `update/` 子目录 |
| | 3. 检查 csproj 的发布配置是否包含 Upgrade 项目输出 |

```xml
<!-- 确保发布后目录结构： -->
/InstallPath
  ├── MyApp.exe          ← MainAppName
  ├── generalupdate.manifest.json
  └── update/
      └── UpgradeApp.exe  ← UpdateAppName
```

---

### Q2: "Method not found" NuGet 版本冲突

| 项目 | 内容 |
|------|------|
| **来源** | Gitee #I7MCA5 |
| **根因** | Client 和 Upgrade 使用不同版本的 GeneralUpdate NuGet，内部 DLL 版本不匹配 |
| **修复** | Client 和 Upgrade 项目务必使用**完全相同版本**的 `GeneralUpdate.Core` NuGet 包 |

```xml
<!-- Client.csproj 和 Upgrade.csproj 版本号必须一致 -->
<PackageReference Include="GeneralUpdate.Core" Version="5.*" />
```

---

### Q3: "Specified argument was out of range of valid values"

| 项目 | 内容 |
|------|------|
| **来源** | Gitee #I893U8 |
| **根因** | UpdateRequest 中版本号格式异常或缺少必要字段 |
| **修复** | 检查版本号格式是否为 4 段式（如 "1.0.0.0"），确保 UpdateUrl 和 AppSecretKey 已配置 |

---

## 🟥 双进程与 IPC 问题

### Q4: Linux 下 Environment.GetEnvironmentVariable("ProcessInfo") 为空

| 项目 | 内容 |
|------|------|
| **来源** | Gitee #ID4ZF5 |
| **根因** | Linux 上环境变量作用域问题，旧版使用 EnvironmentVariableTarget.User 或 Machine 可能不生效 |
| **修复** | 使用最新版（已改用加密文件 IPC）或 配置 `NamedPipeIpcProvider` |

---

### Q5: IPC 加密文件被防病毒软件隔离

| 项目 | 内容 |
|------|------|
| **来源** | 代码审计 #1,#2 |
| **根因** | IPC 文件路径固定为 `%TEMP%/GeneralUpdate/ipc/process_info.enc`，防病毒软件可能拦截 |
| **修复** | 使用 NamedPipe IPC 替代，参考 `generalupdate-advanced` 的 `NamedPipeIPC.cs` |

---

## 🟥 OSS 更新问题

### Q6: OSS 模式下下载完成但没有更新

| 项目 | 内容 |
|------|------|
| **来源** | GitHub #485、#487 |
| **根因** | 多个问题： |
| | 1. OSS 不区分 Main/Upgrade 更新（`HasMainUpdate` 和 `HasUpgradeUpdate` 总是相同值） |
| | 2. SSL 验证策略不覆盖文件下载 |
| **修复** | 显式设置 SSL 策略并确保 UpgradeApp.exe 在 update/ 子目录 |

```csharp
// OSS 模式下额外配置
.SetOption(Option.PatchEnabled, false)  // OSS 下关闭差分
```

---

## 🟥 静默更新问题

### Q7: 静默模式不生效 / 更新没有触发

| 项目 | 内容 |
|------|------|
| **来源** | GitHub #484、#471、Gitee #IJQ0Q5 |
| **根因** | |
| | 1. `ProcessExit` 事件不保证触发（FailFast / TerminateProcess / Ctrl+C） |
| | 2. manifest.json 默认非空字段阻塞自动发现 |
| | 3. 静默模式下 PatchMiddleware 抛出异常阻断流程 |
| **修复** | |

```csharp
// 1. 显式触发更新（替代 ProcessExit 依赖）
.OnAppClosing(() => bootstrap.SilentOrchestrator?.TryLaunchUpgrade());

// 2. manifest.json 中字段正确配置
{
  "MainAppName": "MyApp.exe",
  "ClientVersion": "1.0.0.0"  // 必须填写，不能为空
}

// 3. 静默模式关闭差分
.SetOption(Option.PatchEnabled, false)
```

---

### Q8: 静默模式更新完后自动启动了应用

| 项目 | 内容 |
|------|------|
| **来源** | Gitee #IJQ0Q5 |
| **根因** | 静默模式在更新完成后默认启动应用 |
| **修复** | |

```csharp
// 不自动启动更新后的应用
.SetOption(Option.Silent, true)
// 当前版本通过 LauchClientAfterUpdate 控制
```

---

## 🟥 差分更新问题

### Q9: 增量更新报错 / patch 应用失败

| 项目 | 内容 |
|------|------|
| **来源** | Gitee #II75WI、#I8T0QX |
| **根因** | |
| | 1. 旧 patch 临时文件残留（TempPath 未清理） |
| | 2. 文件已被上次部分更新修改，hash 不匹配 |
| **修复** | |

```csharp
// 每次更新前清理临时目录
.SetOption(Option.AutoCleanTemp, true)

// 或手动清理：
if (Directory.Exists(tempDir))
    Directory.Delete(tempDir, true);
```

---

### Q10: 不同目录有同名文件时封包出错

| 项目 | 内容 |
|------|------|
| **来源** | Gitee #II77NS |
| **根因** | `DefaultCleanMatcher` 未使用相对路径匹配同名文件 |
| **修复** | 使用自定义 CleanMatcher，确保包含相对路径比较 |

```csharp
// 服务端构建差分包时使用：
var diffPipeline = new DiffPipelineBuilder()
    .UseCleanMatcher(new CustomRelativePathCleanMatcher())
    .Build();
```

---

### Q11: 多级文件夹结构更新后文件位置错乱

| 项目 | 内容 |
|------|------|
| **来源** | Gitee #I59QRI |
| **根因** | 子目录中的文件被错误地更新到根目录，而非保持原有文件夹层级 |
| **建议** | 确保差分包中文件路径包含完整相对路径；使用最新版（已修复） |

---

## 🟥 界面与用户反馈

### Q12: 中文文件名在更新后显示乱码

| 项目 | 内容 |
|------|------|
| **来源** | Gitee #I502QQ |
| **根因** | ZIP 解压未指定编码，默认使用 ASCII 而非 GBK/UTF-8 |
| **修复** | |

```csharp
// 在配置中指定编码
.SetOption(Option.Encoding, CompressionEncoding.UTF8)
// 或在 ZipCompressionStrategy 中指定 System.Text.Encoding.Default
```

---

### Q13: 版本号更新后出现异常字符或错误版本号

| 项目 | 内容 |
|------|------|
| **来源** | Gitee #I8TNPE |
| **根因** | 版本链计算逻辑缺陷，中间版本号被累积或污染 |
| **建议** | 更新到最新版（v5.0+），该问题已在重构中修复 |

---

## 🟥 文件系统问题

### Q14: 更新时文件被占用 / "file in use"

| 项目 | 内容 |
|------|------|
| **来源** | GitHub #479、Gitee #ID3UDN |
| **根因** | |
| | 1. 进程退出后文件句柄未完全释放就立即覆盖 |
| | 2. DiffPipeline.CopyUnknownFiles 在文件锁定时报错 |
| **修复** | |

```csharp
// 增加 GracefulExit 等待时间
// 或在 CopyUnknownFiles 中添加重试逻辑
.SetOption(Option.DownloadTimeout, 120)
```

---

### Q15: 备份目录递归嵌套导致 PathTooLongException

| 项目 | 内容 |
|------|------|
| **来源** | GitHub #501 |
| **根因** | `StorageManager.Backup()` 在 InstallPath 内部创建备份目录，且空列表 `new List<string>()` 不会触发默认跳过目录逻辑 |
| **修复** | 使用最新版（已修复）或在配置中显式指定跳过目录 |

---

## 🟥 升级后问题

### Q16: 更新成功后版本号未更新 / 版本号回退

| 项目 | 内容 |
|------|------|
| **来源** | GitHub #467、#475 |
| **根因** | manifest.json 未 WriteBack 更新版本号，下次检查时认为有新版本 |
| **修复** | 使用 v5.0+（已实现 WriteBack），旧版本需手动在 `OnAfterUpdateAsync` hook 中更新 manifest |

```csharp
// 如果使用旧版本，在 hooks 中手动更新：
public async Task OnAfterUpdateAsync(UpdateContext context)
{
    var manifestPath = Path.Combine(
        context.InstallPath, "generalupdate.manifest.json");
    var manifest = JsonSerializer.Deserialize<ManifestInfo>(
        await File.ReadAllTextAsync(manifestPath));
    manifest.ClientVersion = context.LastVersion;
    await File.WriteAllTextAsync(manifestPath,
        JsonSerializer.Serialize(manifest));
}
```

---

### Q17: Linux / macOS 更新后文件缺少执行权限

| 项目 | 内容 |
|------|------|
| **来源** | Gitee #ID5049 |
| **根因** | 更新后新文件默认没有 Unix 可执行权限 |
| **修复** | |

```csharp
// 添加 UnixPermissionHooks
bootstrap.Hooks<UnixPermissionHooks>();

// 或自定义 hook：
public async Task<bool> OnBeforeStartAppAsync(UpdateContext context)
{
    if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
    {
        var app = Path.Combine(context.InstallPath, context.MainAppName);
        if (File.Exists(app))
            await UnixPermissionHooks.SetExecutablePermissionAsync(app);
    }
    return true;
}
```

---

## 🟥 版本检查问题

### Q18: 版本比较错误 / 认为有更新的版本但实际上没有

| 项目 | 内容 |
|------|------|
| **来源** | GitHub #475、Server #26 |
| **根因** | `UpgradeService.Verification` 中的版本比较使用 `System.Version`，"1.0" 与 "1.0.0.0" 比较结果不同 |
| **修复** | 确保服务端和客户端版本号均为 4 段式 |

---

### Q19: 无限升级循环

| 项目 | 内容 |
|------|------|
| **来源** | GitHub #475、代码审计 #20 |
| **根因** | |
| | 1. 场景判断与 DownloadPlan 不一致（服务端说有更新但实际没有可下载的包） |
| | 2. Version 为 null 或空时转为默认值 "1.0.0.0"，导致总是"有新版本" |
| **修复** | 确保 manifest.json 中的版本号正确；更新到 v5.0+（已修复场景判断逻辑） |

---

## 🟥 Push / SignalR 问题

### Q20: SignalR 推送更新后没有反应

| 项目 | 内容 |
|------|------|
| **来源** | GitHub #402、#5 (代码审计) |
| **根因** | `UpgradeHubService.DisposeAsync` 不置 null，重连时 `ObjectDisposedException` |
| **修复** | 使用 `SafeHubConnection` 包装类（见 `PushStrategy.cs`）或更新到 v5.0+ |

---

## 🟥 Bowl 崩溃守护问题

### Q21: Bowl 启动后没有生成 dump 文件

| 项目 | 内容 |
|------|------|
| **来源** | GitHub #492 |
| **根因** | Bowl 的 IPC 文件读写冲突，每次读取后自动删除文件，但如果 Bowl 与 Upgrade 进程同时读写，文件可能已被删 |
| **修复** | 使用最新版（已修复 IPC 架构）；手动下载 procdump 工具 |

---

## 🟥 构建与部署问题

### Q22: 上传的 ZIP 包无法解压 / 解压错误

| 项目 | 内容 |
|------|------|
| **来源** | Gitee #I59Q5W、#I502QQ |
| **根因** | ZIP 编码格式不兼容、多层目录结构处理异常 |
| **修复** | 上传前验证 ZIP 结构；在配置中显式设置压缩格式 |

---

### Q23: 当前版本号获取不正确（Assembly.GetExecutingAssembly）

| 项目 | 内容 |
|------|------|
| **来源** | Gitee #I5O4KV |
| **根因** | 旧版使用 `Assembly.GetExecutingAssembly()` 获取版本号，在 .NET 中应使用 `Assembly.GetEntryAssembly()` |
| **修复** | 确保 manifest.json 中显式填写 `ClientVersion`；更新到最新版（已修复） |

---

## 📋 通用诊断流程

当用户报告问题时，按以下流程排查：

```
1. 版本检查
   ├─ 确认 Client 和 Upgrade 使用相同版本的 NuGet 包
   └─ 确认使用最新发布版（而非旧版）

2. 配置文件检查
   ├─ generalupdate.manifest.json 是否存在、格式是否正确
   ├─ ClientVersion 是否填写（不能为空字符串）
   └─ UpdateAppName 是否指向存在的文件

3. 双进程检查
   ├─ UpgradeApp.exe 是否存在
   ├─ Client 和 Upgrade 的 AppSecretKey 是否一致
   └─ IPC 文件路径是否有权限写入

4. 策略配置检查
   ├─ 标准模式：UpdateUrl 是否可访问
   ├─ OSS 模式：versions.json 是否可下载
   └─ 静默模式：ProcessExit 是否能触发

5. 日志检查
   ├─ generalupdate-trace {yyyy-MM-dd}.log 中有无异常
   └─ EventManager 是否触发了 Exception 事件
```
