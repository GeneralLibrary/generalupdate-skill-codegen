# GeneralUpdate 故障排查参考手册（完整版）

> 覆盖 50+ 症状，均来自 GitHub Issues（#308–#517）、Gitee Issues（30个）、
> 及全面代码审计（17 CRITICAL/HIGH 项 + 14 MEDIUM 项 + 10 INFO 项）
> 排查日期：2026-06-16

---

## 使用方式

按症状查找 → 确认根因 → 应用修复。如果症状不匹配，运行通用诊断流程（见底部）。

---

## 🔴 一级：致命/阻断性故障

### C1. 升级进程没启动 / "FileNotFoundException: upgrade application not found"

| 来源 | 根因 | 诊断 |
|------|------|------|
| #485, #ID3H5V | UpgradeApp.exe 未随主程序发布 | 检查 `UpdatePath` + `UpdateAppName` |

**修复**：
1. UpgradeApp.exe **必须**从首个版本就和主程序一起发布
2. OSS 模式下 Upgrade.exe 必须放在 `update/` 子目录（#485）
3. `generalupdate.manifest.json` 中 `UpdateAppName` 必须包含 `.exe`

```
发布目录结构：
/InstallPath
  ├── MyApp.exe
  ├── generalupdate.manifest.json
  └── update/
      └── UpgradeApp.exe   ← 必须存在
```

---

### C2. "Method not found" — NuGet 版本冲突

| 来源 | 根因 | 诊断 |
|------|------|------|
| #I7MCA5 | Client 和 Upgrade 使用不同版本 NuGet | 检查两个项目的 csproj |

**修复**：Client.csproj 和 Upgrade.csproj 使用完全相同版本：
```xml
<PackageReference Include="GeneralUpdate.Core" Version="5.*" />
```

---

### C3. BSOD / 内存溢出 / 进程崩溃 — BSDIFF 整数溢出

| 来源 | 代码审计 #3, #4 |
|------|----------------|
| **根因** | `BsdiffDiffer.WriteInt64` 对 `long.MinValue` 求反溢出；control 值 `> int.MaxValue` 转型截断产生负值 |

**影响**：恶意构造的 patch 文件或超过 2GB 的正常 patch 可导致进程崩溃或 OOM
**修复**：更新到 v10.4.6+（#514 已修复）。如无法更新，在差分引擎中添加 `MaxInputFileSize` 限制

---

### C4. 备份递归嵌套 → PathTooLongException（路径超长）

| 来源 | #501 |
|------|------|
| **根因** | `StorageManager.Backup()` 在 `InstallPath` **内部**创建备份目录，且空列表 `new List<string>()` 不触发默认跳过目录逻辑 |

**修复**：
```csharp
// 使用 Configinfo 的 SkipDirectorys 属性
var config = new Configinfo
{
    // ...
    SkipDirectorys = new List<string> { ".backups", "backup-" }
};
```

---

### C5. ZIP 解压路径穿越 — 恶意包可覆盖任意文件

| 来源 | 代码审计 #7 |
|------|-----------|
| **根因** | `ZipCompressionStrategy.Decompress` 只做 `Regex.Replace` 清理，未验证 `Path.GetFullPath(combinedPath).StartsWith(Path.GetFullPath(unZipDir))` |

**影响**：攻击者通过 `../../evil.exe` 条目逃逸到任意目录
**修复**：更新到 v10.4.6+（已修复）。旧版本手动添加路径校验

---

### C6. 硬编码 AES 密钥 — IPC 加密形同虚设

| 来源 | 代码审计 #1, #2, #14 |
|------|---------------------|
| **根因** | AES 密钥由常量 `SHA256("GeneralUpdate.IPC.EnvironmentProvider.v1")` 派生，IV 16 字节中仅第 1 字节非零 |

**影响**：任何拿到反编译代码的人可解密 IPC 文件
**修复**：使用 NamedPipe IPC（见 advanced/templates/NamedPipeIPC.cs）；或部署 DPAPI 加密

---

### C7. 跨租户数据泄漏（服务端）

| 来源 | 代码审计 #15 |
|------|------------|
| **影响范围** | 11 处服务端漏洞：包/客户端/分组/升级记录/文件/租户隔离均缺失 |

**修复**：升级到 GeneralSpacestation 最新版；紧急措施：为每个租户部署独立实例

| 具体漏洞 | 所在文件 | 影响 |
|---------|---------|------|
| GroupId 过滤条件取反 | `ClientService.cs:36-37` | 分组查询返回错误客户端 |
| UserService 可改 TenantId | `UserService.cs:338-356` | 租户间权限提升 |
| 升级记录无租户 ID | `UpgradeService.cs:242-256` | 租户隔离彻底失效 |
| 全局包可见于所有租户 | `UpgradeService.cs:49-57` | 跨租户数据暴露 |
| 文件删除无租户过滤 | `FileService.cs:98-108` | 任意文件可删 |

---

### C8. PushJob 静默吞异常 — Quartz 不知作业失败

| 来源 | 代码审计 #16 |
|------|------------|
| **根因** | `PushJob.Execute` 被 `try-catch(Exception)` 包裹只 `LogError`，Quartz 不触发重试 |

**影响**：推送任务对运维完全不可见
**修复**：在 catch 中 rethrow 或移除外层 catch

---

## 🟠 二级：高优先级 / 场景阻断

### H1. 静默模式不生效

| 来源 | #484, #471, #443, #IJQ0Q5 |
|------|--------------------------|
| **根因**（多重）： | |
| | ① `ProcessExit` 事件不保证触发（FailFast/TerminateProcess/Ctrl+C 下不触发） |
| | ② `manifest.json` 默认非空字段阻塞 `AppMetadataDiscoverer.Discover()` |
| | ③ 静默模式下 `PatchMiddleware` 抛出异常（未注入 DiffPipeline） |
| | ④ `manifest.json` 的 MainAppName 默认值 "GeneralUpdate.Core.exe" 在静默启动时阻塞身份发现 |

**修复**：
```csharp
// ① 应用关闭时显式触发（替代 ProcessExit 依赖）
public void OnAppClosing()
{
    // v10.4.6 稳定版无 SilentOrchestrator
    // 应用正常退出即可，GeneralUpdate 内部处理
}

// ② manifest 字段确保填写正确的版本号

---

### H2. 无限升级循环（每次启动都检查到"新版本"）

| 来源 | #475, #467, 代码审计 #20 |
|------|------------------------|
| **根因**（多重）： | |
| | ① 场景判断与 DownloadPlan 不一致（服务端说有更新但无包可下载） |
| | ② manifest.json 未 WriteBack 版本号 |
| | ③ Version 为 null/空 → 被转为默认值 "1.0.0.0" → 永远比服务端旧 |

**修复**：
1. 更新到 v10.4.6+（已修复无限升级循环 + 场景判断）

---

### H3. 循环：Process.Start 启动进程后未检查返回值

| 来源 | 代码审计 H2 |
|------|-----------|
| **根因** | 5 个 Strategy 文件中 `Process.Start()` 返回值未检查（null → 静默失败） |

**修复**：更新到 v10.4.6+（已修复，失败时抛异常）

---

### H4. UpdateReporter 注入不生效 / ReportUrl 未配置抛异常

| 来源 | #470 |
|------|------|
| **根因** | `UpdateReporter<T>()` 注册的实现未被消费；`ProcessInfo` 构造函数将 `ReportUrl` 作为必填 |

**修复**：更新到 v10.4.6+（已修复）

---

### H5. Sync-over-async 死锁 — GetAwaiter().GetResult()

| 来源 | #451, 代码审计 #6 |
|------|-----------------|
| **根因** | `AppDomain.ProcessExit` 事件处理程序同步调用 `.GetAwaiter().GetResult()`，在 WPF/WinForms 的 SynchronizationContext 上死锁 |

**影响**：桌面应用使用静默模式时，进程退出可能挂起
**修复**：更新到 v10.4.6+（已修复，改为 `ConfigureAwait(false)` + Task.Run）

---

### H6. 前钩子 (SafeOnBeforeUpdateAsync) 异常时返回 true（应返回 false）

| 来源 | 代码审计 H4 |
|------|-----------|
| **根因** | `ClientStrategy.cs:1015-1026` 异常时返回 `true`（放行更新），应返回 `false`（中止） |

**影响**：Hooks 中的 `OnBeforeUpdateAsync` 即使抛异常也会继续更新
**修复**：更新到 v10.4.6+

---

### H7. Scenario = Both 误判 — DownloadPlan 为空但判断为有更新

| 来源 | #465, #475 |
|------|-----------|
| **根因** | `HttpDownloadSource.ListAsync()` 只检查 `Body.Count > 0`，未验证 `AppType` 匹配；`DownloadPlanBuilder.Build()` 版本过滤后可能返回空列表 |

**修复**：更新到 v10.4.6+（已修复场景判断逻辑）

---

### H8. OSS 模式：下载完成但没有更新

| 来源 | #485, #487 |
|------|-----------|
| **根因** | ① OSS 不区分 Main/Upgrade 更新（HasMainUpdate 和 HasUpgradeUpdate 总是相同）② SSL 验证不覆盖文件下载 |

**修复**：更新到 v10.4.6+（已修复场景判断逻辑）

---

### H9. PatchMiddleware 在静默模式必定抛异常

| 来源 | #471 |
|------|------|
| **根因** | `SilentPollOrchestrator.CreateStrategy()` 创建裸 `WindowsStrategy`，未注入 `DiffPipeline` |

**修复**：更新到 v10.4.6+（已修复）

---

### H10. HttpClient 无限超时

| 来源 | 代码审计 M2 |
|------|-----------|
| **根因** | `HttpClientProvider.Shared` 设置 `Timeout = InfiniteTimeSpan` |

**修复**：更新到 v10.4.6+（已设置为 5 分钟安全上网限）

---

### H11. 更新成功后版本号未 WriteBack

| 来源 | #467, #475 |
|------|-----------|
| **根因** | manifest.json 未更新，下次启动时版本号还是旧的 |

**修复**：更新到 v10.4.6+（已实现 WriteBack）。旧版本手动处理：
```csharp
/// v10.4.6 稳定版无 IUpdateHooks 接口。
/// 如需在更新后回写版本号，可在服务端处理。
```

---

## 🟡 三级：中等 / 需要关注

### M1. 增量更新报错：patch 应用失败

| 来源 | #II75WI, #I8T0QX |
|------|-----------------|
| **根因** | ① 旧 patch 临时文件残留 ② 文件已被修改导致 hash 不匹配 |

**修复**：
```csharp
// 每次更新前手动清理临时目录，避免残留文件
if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
```

### M2. 同名文件在不同目录时封包出错

| 来源 | #II77NS |
|------|---------|
| **根因** | `DefaultCleanMatcher.Match` 未使用相对路径匹配 |

**修复**：使用自定义 CleanMatcher：
```csharp
new DiffPipelineBuilder()
    .UseCleanMatcher(new CustomRelativePathCleanMatcher())
    .Build();
```

### M3. 多级文件夹结构更新后文件位置错乱

| 来源 | #I59QRI |
|------|---------|
| **根因** | 子目录文件被错误更新到根目录 |

**修复**：更新到最新版；确保差分包路径包含完整相对路径

### M4. 中文文件名乱码

| 来源 | #I502QQ |
|------|---------|
| **根因** | ZIP 解压未指定编码 |

**修复**：确保构建 ZIP 时使用 UTF-8 编码（编码由内部处理，无需在代码中设置）

### M5. 版本号出现异常字符

| 来源 | #I8TNPE |
|------|---------|
| **根因** | 版本链计算缺陷，中间版本号被污染 |

**修复**：更新到 v10.4.6+（已修复）

### M6. 文件被占用 / "file in use"

| 来源 | #479, #ID3UDN |
|------|-------------|
| **根因** | 进程退出后文件句柄未完全释放 |

**修复**：
```csharp
// 增加重试次数
var config = new Configinfo
{
    // ...
};
// 更新到 v10.4.6+ 内置重试逻辑
```

### M7. Linux 下 Environment.GetEnvironmentVariable("ProcessInfo") 为空

| 来源 | #ID4ZF5 |
|------|---------|
| **根因** | Linux 环境变量作用域问题 |

**修复**：使用最新版（已改用加密文件 IPC）或 NamedPipe IPC

### M8. Linux / macOS 更新后文件无执行权限

| 来源 | #ID5049 |
|------|---------|
| **根因** | 新文件缺少 Unix 可执行权限 |

**修复**：
```csharp
bootstrap.Hooks<UnixPermissionHooks>();
```

> ⚠️ v10.4.6 稳定版无 IUpdateHooks 接口。Linux 权限问题需手动处理 chmod +x。

### M9. IPC 加密文件被防病毒软件隔离

| 来源 | 代码审计 #1, #2 |
|------|----------------|
| **根因** | IPC 路径固定为 `%TEMP%/GeneralUpdate/ipc/process_info.enc` |

**修复**：使用 NamedPipe IPC 替代

### M10. 版本比较错误："1.0" 与 "1.0.0.0" 不等

| 来源 | #475, 服务端 #26 |
|------|----------------|
| **根因** | `System.Version` 将 "1.0" 解析为 `1.0.-1.-1`，`< "1.0.0.0"` |

**修复**：服务端和客户端版本号统一为 4 段式

### M11. Assembly.GetExecutingAssembly 获取版本号不正确

| 来源 | #I5O4KV |
|------|---------|
| **根因** | 应使用 `Assembly.GetEntryAssembly()`，而非 `GetExecutingAssembly()` |

**修复**：在 manifest.json 中显式填写 `ClientVersion`

### M12. SignalR 推送后无反应（ObjectDisposedException）

| 来源 | #402, 代码审计 #5 |
|------|-----------------|
| **根因** | `UpgradeHubService.DisposeAsync` 不置 null，重连时崩溃 |

**修复**：使用 `SafeHubConnection` 包装类（见 PushStrategy.cs）

### M13. Bowl 没有生成 dump 文件

| 来源 | #492 |
|------|------|
| **根因** | Bowl IPC 文件每次读取后自动删除，多进程竞争 |

**修复**：更新到 v10.4.6+（已修复 Bowl IPC 架构）；手动下载 procdump

### M14. 默认备份保留最多 3 个版本

| 来源 | 默认行为 |
|------|---------|
| **根因** | `StorageManager.CleanBackup` 只保留最近 3 个备份 |

**修复**：如需更多保留，在 StorageManager 中配置：
```csharp
// 注意：Option.BackupConfig 为不存在常量，需直接使用 StorageManager.BackupConfig
// GeneralUpdate 默认只保留最近 3 个备份
```

### M15. DefaultCleanMatcher 每次调用创建新 StorageManager 实例（并发不安全）

| 来源 | 代码审计 #17 |
|------|------------|
| **根因** | 实例级别持有 `_fileCount` 和 `ComparisonResult`，但被并行调用 |

**修复**：更新到 v10.4.6+ 或在 `DiffPipeline.CleanAsync` 中添加锁

### M16. HttpDownloadExecutor 不校验 Content-Length

| 来源 | 代码审计 #22 |
|------|------------|
| **根因** | `StreamDownloadAsync` 不验证下载字节数 |

**修复**：更新到 v10.4.6+（已添加校验）

### M17. OssStrategy.StartAppAsync 返回 Task.CompletedTask

| 来源 | 代码审计 #30 |
|------|------------|
| **根因** | `appName` 为空时直接返回，调用方无法区分"已启动"和"跳过" |

**修复**：显式检查空值并抛异常

### M18. EventManager 单例 — Dispose 后仍可访问

| 来源 | 代码审计 #11 |
|------|------------|
| **根因** | `Lazy<EventManager>` 单例，Dispose 后 `_lazy.Value` 返回已释放实例 |

**修复**：自行管理生命周期，在 Bootstrap 结束时调用 Clear

### M19. GeneralTracer.Dispose 清空全局 Trace.Listeners

| 来源 | 代码审计 #13 |
|------|------------|
| **根因** | `Dispose()` 调用 `Trace.Listeners.Clear()`，影响同一进程其他库的日志输出 |

**建议**：更新到 v10.4.6+（已改为只移除自己的 Listener）

### M20. GeneralTracer 日志只按天轮转，永不过期

| 来源 | 代码审计 #28 |
|------|------------|
| **根因** | `generalupdate-trace {yyyy-MM-dd}.log` 永不过期 |

**修复**：手动配置日志保留策略，或定期清理 `Logs/` 目录

---

## 🔵 四级：低优先 / 代码气味 / 已知行为

### L1. DefaultRetryPolicy 用字符串包含判断 HTTP 状态码

| 来源 | 代码审计 #10 |
|------|------------|
| **根因** | `s.Contains("500")` 可能误匹配 URL 或响应正文中的 "500" |
| **建议** | 使用 `HttpRequestException.StatusCode` 属性 |

### L2. OssDownloadSource 不区分 Main/Upgrade

| 来源 | 代码审计 #27 |
|------|------------|
| **根因** | 将 `HasMainUpdate` 和 `HasUpgradeUpdate` 都设为 `assets.Count > 0` |
| **建议** | OSS 模式接受此行为，或自行实现 IDownloadSource |

### L3. ProcessContract 构造函数空检查顺序错误

| 来源 | 代码审计 #9 |
|------|------------|
| **根因** | 先检查 `Directory.Exists(installPath)`，然后才 `?? throw` |
| **建议** | 小问题，不影响功能 |

### L4. ConfigurationMapper.MapToUpdateContext 静默接受 null

| 来源 | 代码审计 #20 |
|------|------------|
| **根因** | `source == null` 返回空的 `UpdateContext` |
| **建议** | 检查配置是否正确加载 |

### L5. StorageManager 跳过目录使用 string.Contains 匹配

| 来源 | 代码审计 #21 |
|------|------------|
| **根因** | `dirName.Contains("backup-")`，目录名 `backup-custom` 因包含 "backup-" 也被跳过 |
| **建议** | 影响小，如需精确控制使用自定义跳过策略 |

### L6. FileTreeComparer FAT32 时间精度 2 秒漏判

| 来源 | 代码审计 #18 |
|------|------------|
| **根因** | FAT32 文件系统时间戳精度 2 秒 |
| **建议** | 对 FAT32 卷添加哈希比对兜底 |

### L7. DiffPipeline.CopyUnknownFiles 用 Replace 截取相对路径

| 来源 | 代码审计 #31 |
|------|------------|
| **根因** | `file.FullName.Replace(targetPath, "")` 当 targetPath 出现在路径中间时出错 |
| **建议** | 使用 `StartsWith + Substring` |

### L8. StreamingHdiffDiffer 文件超限时截断

| 来源 | 代码审计 #32 |
|------|------------|
| **根因** | 超过 `MaxWindowSize` (默认 128MB) 时截断读取前 128MB |
| **建议** | 大文件使用全量更新替代差分 |

### L9. Bowl StorageHelper.Restore 无条件执行

| 来源 | 代码审计 #33 |
|------|------------|
| **根因** | `AutoRestore=true` 时无验证恢复结果 |
| **建议** | 回滚后增加校验 |

### L10. OssStrategy 版本比较可能抛异常

| 来源 | 代码审计 #23 |
|------|------------|
| **根因** | `new Version("")` 抛 ArgumentException |
| **建议** | 使用 `ParseVersion` 安全解析 |

### L11. 静默模式更新完自动启动应用

| 来源 | #IJQ0Q5 |
|------|---------|
| **建议** | v10.4.6 无 SilentAutoRestart 选项 |

### L12. OSS 模式下传的 ZIP 包编码无法解压

| 来源 | #I59Q5W, #I502QQ |
|------|----------------|
| **建议** | 构建 ZIP 时指定 UTF-8，上传前验证解压 |

---

## 📋 通用诊断流程

当用户报告的问题未在以上清单中找到时，执行系统性诊断：

### 步骤 1：版本检查
```
□ Client 和 Upgrade 使用相同 NuGet 版本号？
□ 使用最新稳定版（v5.0+ 推荐）？
```

### 步骤 2：配置文件检查
```
□ generalupdate.manifest.json 是否存在？
□ 格式是否正确（JSON 语法校验）？
□ ClientVersion 已填写（非空字符串）？
□ MainAppName 包含 .exe 扩展名？
□ UpdateAppName 指向存在的文件？
□ InstallPath 路径可访问？
```

### 步骤 3：双进程检查
```
□ UpgradeApp.exe 存在于发布目录？
□ Client 和 Upgrade 使用相同 AppSecretKey？
□ %TEMP%/GeneralUpdate/ipc/ 目录可写入？
□ 防病毒软件未隔离该目录？
```

### 步骤 4：策略配置检查
```
标准模式：
  □ UpdateUrl 可访问（HTTP 200）？
  □ /Upgrade/Verification 接口返回正确格式？
  □ AppSecretKey 与服务端一致？

OSS 模式：
  □ versions.json URL 可下载？
  □ versions.json 格式正确？
  □ 版本号比较正常？

静默模式：
  □ ProcessExit 能触发（非 FailFast 场景）？
  □ 应用关闭时显式调用了 TryLaunchUpgrade()？
  □ manifest 字段全部正确填写？
```

### 步骤 5：日志检查
```
□ 查看 generalupdate-trace {yyyy-MM-dd}.log（位于 {BaseDir}/Logs/）
□ EventManager 是否触发了 Exception 事件？
□ AddListenerException 是否收到异常？
```

### 步骤 6：平台特定检查
```
Windows:
  □ 防病毒软件是否拦截 IPC 文件或临时目录？
  □ 管理员权限是否必要？

Linux/macOS:
  □ 文件可执行权限是否设置？
  □ 环境变量作用域是否正确？
  □ Mono 或 .NET 运行时版本兼容？

AOT:
  □ SignalR 使用 JSON 协议 + JsonSerializerContext？
  □ 反射调用被 preserve？
```

---

## 快速诊断命令

```bash
# 1. 检查 manifest 文件
if [ -f generalupdate.manifest.json ]; then cat generalupdate.manifest.json | python3 -m json.tool; fi

# 2. 检查升级程序是否存在
ls -la update/UpgradeApp.exe 2>/dev/null || echo "UpgradeApp.exe not found"

# 3. 检查 IPC 文件（Windows）
ls -la /tmp/GeneralUpdate/ipc/ 2>/dev/null || echo "No IPC directory (expected before first update)"

# 4. 检查更新日志
if [ -d Logs ]; then cat Logs/generalupdate-trace*.log 2>/dev/null | tail -100; fi

# 5. 验证服务端 API
curl -s -X POST https://your-server.com/Upgrade/Verification \
  -H "Content-Type: application/json" \
  -d '{"appKey":"test","appType":1,"clientVersion":"1.0.0.0","productId":"test"}' | head -20

---

## Issue 索引（快速跳转）

| 范围 | 内容 | GitHub | Gitee |
|------|------|--------|-------|
| v5 重构 | 策略/配置/Bootstrap 重写 | #308–#361 | — |
| 扩展点修复 | 扩展点注入不消费 | #455, #457, #373 | — |
| 静默修复 | ProcessExit/PatchMiddleware | #471, #484 | #IJQ0Q5 |
| 场景判断 | Both 误判 / 无限循环 | #465, #475 | — |
| 差分问题 | 同名文件/残留 | — | #II77NS, #I8T0QX |
| 中文乱码 | ZIP 编码 | — | #I502QQ |
| 多级文件夹 | 文件位置错乱 | — | #I59QRI |
| Linux 权限 | 可执行权限 | — | #ID5049 |
| Linux IPC | 环境变量为空 | — | #ID4ZF5 |
| NuGet 版本 | Method not found | — | #I7MCA5 |
| 备份嵌套 | PathTooLongException | #501 | — |
| Bowl IPC | 文件读写冲突 | #492 | — |
| 推送 | SignalR 重连崩溃 | #402 | — |
| OSS | 不区分场景、SSL 覆盖 | #485, #487 | — |
| IPC 加密 | 固定密钥、固定路径 | 代码审计 #1, #2 | — |
| BSDIFF | 整数溢出、OOM | 代码审计 #3, #4 | — |
| 路径穿越 | ZIP 解压 | 代码审计 #7 | — |
| 跨租户 | 11 处服务端漏洞 | 代码审计 #15 | — |
