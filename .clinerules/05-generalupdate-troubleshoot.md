---
description: "GeneralUpdate troubleshooting — 50+ issues C/H/M/L, diagnostic workflow"
globs: ["**/*.cs", "**/*.csproj", "**/*.json"]
tags: ["dotnet", "generalupdate"]
---

# 故障排查

## 诊断: 版本一致? manifest正确? UpgradeApp存在? 策略可访问? 日志异常?

## C级: 升级没启动/Method not found/路径超长/IPC暴露/跨租户泄露
## H级: 静默不生效/无限循环/OSS无更新/文件占用

## 命令: curl测试API, cat manifest验证, ls检查UpgradeApp

---

## 诊断流程 (5步)

1. **版本号** — `generalupdate.manifest.json` 的 `ClientVersion` / `UpgradeClientVersion` 是否与服务端返回的版本一致? 4段式版本(string)严格匹配。
2. **manifest 字段** — `MainAppName`、`UpdateAppName`、`ProductId`、`InstallPath`、`UpdatePath` 是否填写正确且与服务端一致?
3. **UpgradeApp 存在** — 主应用目录下是否存在 `Upgrade` 子目录及 `Upgrade.exe` (或对应平台可执行文件)? 随首个版本一同发布的。
4. **策略可访问** — `SetSource(url, key)` 配置的 API/OSS 端点能否正常响应? OSS 策略需要 `SetOSSInfo` 配置正确。
5. **日志异常** — 检查 `logs/` 目录下 GeneralUpdate 日志, 搜索 `Exception` / `Error` / `Failed` 关键字。

## C级 (Critical) — 阻塞升级

| 问题 | 原因 | 排查/解决 |
|------|------|-----------|
| **升级没启动** | `LaunchAsync()` 未调用 / 调用顺序错误 / 主进程未检测到新版本 | 确认 `Bootstrap.LaunchAsync()` 在 `Main()` 中调用, 且 `SetOption(AppType.Client)` 正确 |
| **Method not found** | 引用的 GeneralUpdate 版本与服务端不匹配 / 混用不同大版本 | 统一 NuGet 版本, 清理 bin/obj 后重新生成 |
| **路径超长 (>260)** | Windows 路径限制, `InstallPath` / `UpdatePath` 过深 | 缩短安装路径; 或在项目配置中启用长路径支持 (`app.manifest` / `runtimeconfig`) |
| **IPC 暴露** | EncryptedFile IPC 写入明文 / NamedPipe 未鉴权 | 启用加密传输; NamedPipe 设置 `AccessControl` 限制连接用户; 不使用 `AutoFallback` 回退到明文 |
| **跨租户泄露** | 多租户部署共用 `ProductId` / `AppSecretKey` | 每个租户分配独立 `ProductId` 和 `AppSecretKey`; 服务端鉴权时校验租户标识 |

## H级 (High) — 严重但不阻塞启动

| 问题 | 原因 | 排查/解决 |
|------|------|-----------|
| **静默更新不生效** | `UpdateMode.Silent` 配置缺失 / 权限不足无法后台启动 | 确认 `SetOption(UpdateMode.Silent)` 且应用有后台运行权限; 检查 `LaunchCommand` 参数是否正确传递 |
| **无限循环更新** | `ClientVersion` 与服务端最新版本不一致导致不断检测到"新版本" | 验证更新后是否更新了本地 `generalupdate.manifest.json` 的版本号; 确认服务端已标记该版本为"已发布"状态 |
| **OSS 无更新** | Bucket 配置错误 / 文件命名不匹配 / 签名过期 | `curl` 测试 OSS URL 是否直接可下载; 确认 `SetOSSInfo` 的 endpoint/bucket/region 正确 |
| **文件占用** | 更新时目标文件正在被使用 (主进程/杀软/搜索索引) | 关闭主进程后更新; 使用 `Bowl` 守护进程管理进程生命周期; 排除杀软扫描目录 |

## M级 (Medium) — 功能降级

| 问题 | 原因 | 排查/解决 |
|------|------|-----------|
| **进度不动** | `IDownloadService` 未正确绑定 / UI 线程阻塞 | 确认桥接实现; 使用 `async/await` 避免阻塞 UI 线程 |
| **差分更新失败** | BSDIFF 补丁应用时数据不完整 / 旧版本基准文件不匹配 | 校验原始文件哈希; 确认差分策略仅在同源版本间使用 |
| **升级后配置丢失** | `UpdatePath` 未包含配置文件 / 更新后未合并 `appsettings.json` | 在更新流程中添加配置文件合并步骤; 或使用外部配置中心 |
| **信号量泄露** | IPC 信号量未正确释放, 重启后端口/资源被占用 | 重启系统; 检查 `Bowl` 是否正确清理资源; 监控信号量计数 |
| **ZIP 遍历写入** | 恶意更新的 ZIP 包包含 `../` 路径覆盖系统文件 | 解压时校验 ZIP 条目路径, 拒绝包含 `../` 或绝对路径的条目 |
| **AOT 编译失败** | 动态加载 / 反射代码未适配 NativeAOT | 添加 `[DynamicDependency]` 特性; 避免 `Assembly.Load` 等动态加载 |
| **SignalR 重连慢** | WebSocket 长连接断开后 `RetryDelay` 配置过长 | 降低重试延迟并限制最大重试次数, 或自定义重连策略 |
| **SSL 证书错误** | 自签名证书未受信 / 服务器 SNI 不匹配 | 配置 `SslValidationPolicy`; 或添加自定义证书验证委托 |

## L级 (Low) — 非关键

| 问题 | 原因 | 排查/解决 |
|------|------|-----------|
| **UI 闪烁** | 主/升级进程窗口切换 | 使用 `Silent` 模式; 或延长窗口淡入淡出过渡时间 |
| **日志过多** | `LogLevel.Debug` 未关闭 | 生产环境使用 `LogLevel.Warning` 或 `LogLevel.Error` |
| **分发包过大** | 包含完整文件而非差异包 | 启用 `Differential` 策略; 清理冗余依赖 (移除 `Microsoft.*` 运行时) |
| **首次更新慢** | OSS 冷启动 / CDN 首次回源 | 预热 CDN; 或发布版本前预上传到上传节点 |
| **多语言显示异常** | CultureInfo 在更新线程中未继承 | 在更新线程入口处设置 `CultureInfo.CurrentUICulture` |

## 诊断命令速查

```bash
# 测试 API 是否可访问
curl -v -X POST "http://your-server/Upgrade/Verification" -H "Content-Type: application/json" -d '{"appKey":"xxx","appType":1,"clientVersion":"1.0.0.1","productId":"xxx","platform":"windows","tenantId":"default"}'

# 验证 manifest.json 内容
cat /path/to/app/manifest.json | jq .

# 检查 UpgradeApp 是否存在
ls -la /path/to/app/Upgrade/Upgrade.exe

# 查看 OSS 文件列表
curl -v "https://your-bucket.oss-cn-hangzhou.aliyuncs.com/?prefix=updates/"

# 检查端口占用 (IPC 调试)
netstat -ano | findstr :54321

# 检查信号量残留 (Windows)
powershell "Get-CimInstance Win32_Semaphore | Select-Object Name, Count"
```

## 日志文件位置

| 平台 | 默认路径 |
|------|----------|
| Windows | `%LOCALAPPDATA%\GeneralUpdate\logs\{ProductId}\` |
| Linux | `~/.local/share/GeneralUpdate/logs/{ProductId}/` |
| macOS | `~/Library/Logs/GeneralUpdate/{ProductId}/` |

## 安全注意事项

- IPC 传输 — 不使用明文 `AutoFallback`, 始终启用加密
- ZIP 包校验 — 解压时校验 ZIP 条目路径, 拒绝 `../` 和绝对路径条目
- 签名验证 — 程序集签名: 在 `SetSource` 中提供公钥, 拒绝未签名更新
- AppSecretKey 管理 — 硬编码在客户端是最后手段; 优先从启动参数或环境变量注入
