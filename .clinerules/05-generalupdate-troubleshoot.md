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

## C级 (Critical) — 阻塞升级

| 问题 | 原因 | 排查/解决 |
|------|------|-----------|
| **升级没启动** | `LaunchAsync()` 未调用 / UpgradeApp.exe 未部署 | 确认 `Bootstrap.LaunchAsync()` 在 `Main()` 中调用 |
| **Method not found** | Client 和 Upgrade NuGet 版本不一致 | 统一 NuGet 版本, 清理 bin/obj 后重新生成 |
| **路径超长 (>260)** | Windows 路径限制 | 缩短安装路径 |
| **IPC 暴露** | IPC 加密密钥硬编码 | 使用强 AppSecretKey; 更新到 v10.4.6+ |
| **跨租户泄露** | 服务端多租户隔离缺失 | 每个租户独立 ProductId + AppSecretKey |

## H级 (High) — 严重但不阻塞启动

| 问题 | 原因 | 排查/解决 |
|------|------|-----------|
| **无限循环更新** | manifest.json 版本号未回写 | 更新到 v10.4.6+（已修复 WriteBack） |
| **OSS 无更新** | Bucket 配置错误 / versions.json 格式不对 | curl 测试 OSS URL 是否可下载 |
| **文件占用** | 目标文件被占用 | 关闭主进程后更新; 排除杀软扫描目录 |

## M级 (Medium) — 功能降级

| 问题 | 原因 | 排查/解决 |
|------|------|-----------|
| **进度不动** | `IDownloadService` 未正确绑定 | 确认桥接实现 |
| **差分更新失败** | 旧版本基准文件不匹配 | 校验原始文件哈希 |
| **ZIP 遍历写入** | 恶意 ZIP 含 `../` 路径 | v10.4.6+ 已修复 |
| **AOT 编译失败** | 反射代码未适配 NativeAOT | 添加 `[DynamicDependency]` |
| **SignalR 重连慢** | RetryDelay 配置过长 | 调整重连参数 |

## L级 (Low) — 非关键

| 问题 | 原因 | 排查/解决 |
|------|------|-----------|
| **分发包过大** | 未使用差分 | 安装 `GeneralUpdate.Differential` |
| **首次更新慢** | CDN 冷启动 | 预热 CDN |

## 日志文件位置

| 平台 | 默认路径 |
|------|----------|
| Windows | `%TEMP%/GeneralUpdate/logs/` |
| Linux | `/tmp/GeneralUpdate/logs/` |

## 安全注意事项

- AppSecretKey 管理 — 硬编码在客户端是最后手段; 优先从启动参数或环境变量注入
