---
name: generalupdate-troubleshoot
description: |
  Complete diagnostic reference for ALL known GeneralUpdate issues — powered by 50+
  real GitHub/Gitee issues and full code audit findings (17 critical/high + 14 medium + 10 low).
  Covers: install failures, dual-process issues, IPC corruption, OSS failures, silent mode bugs,
  differential patch errors, cross-version (CVP) problems, Bowl crash daemon, SignalR push,
  file system issues, version comparison, cross-tenant leakage, and AOT compatibility.
  Provides root causes, fixes, workarounds, and a 6-step universal diagnostic workflow.
  Triggers on: "update failed", "not working", "error", "bug", "crash", "exception",
  "升级失败", "失败", "报错", "不行", "不工作", "没反应", "问题", "排查",
  "method not found", "file locked", "path too long", "Chinese garbled", "乱码",
  "version incorrect", "版本不对", "infinite loop", "死循环", "bowl not working",
  "push not working", "signalr error", "IPC error", "AOT trim", "跨租户", "权限",
  "permission denied", "access denied", "silent mode", "静默", "差分", "增量".
  Load this skill AUTOMATICALLY when user reports any error, unexpected behavior,
  or failure symptom — run the diagnostic workflow before escalating.
when_to_use: |
  - User reports an error, failure, crash, or unexpected behavior during ANY step
  - User says "it's not working", "something went wrong", has error screenshots
  - User describes specific error messages, exception stack traces, or symptoms
  - User asks about known bugs, issues, or problems with GeneralUpdate
  - User asks for debugging help, diagnostic steps, troubleshooting
  - User mentions Upgrade process not starting, silent mode not working, or Bowl issues
  - User reports version numbers being wrong or update loops
  - User is on Linux/macOS and has permission or IPC problems
  - User mentions cross-tenant or multi-tenant deployment of server
  - User asks about AOT trimming or NativeAOT compatibility
  - User just completed integration and hit an error — run diagnostics
  - Always run AFTER generalupdate-init or generalupdate-ui if the user hits issues
allowed-tools: "Read, Write, Edit, Glob, Grep, Bash"
---

# 🩺 GeneralUpdate 故障排查

综合性诊断系统 — 覆盖 50+ 已知问题，均可追溯到 GitHub/Gitee Issue 或代码审计发现。

---

## 📋 用户症状提取（诊断前必须收集）

```
### 必填信息
- 症状描述: ______
- 错误信息/堆栈: ______
- GeneralUpdate 版本: ______
- 平台: ______（Windows / Linux / macOS）
- .NET 版本: ______
- 更新策略: ______（标准 / OSS / 静默 / 差分 / 跨版本 / 推送）
- 最近是否改过配置: ______（是/否，改了啥）

### 可选信息
- 事件监听中是否有异常（ExceptionEventArgs）: ______
- 是否有日志（Logs/generalupdate-trace *.log）: ______
- 问题是否可复现: ______（是/否，频率）
- 首次出现时间点: ______
```

---

## 工作流程

```
1. 症状收集
   ├── 用户描述的症状是什么？
   ├── 错误信息/堆栈是什么？
   ├── GeneralUpdate 版本号？
   ├── 平台（Windows/Linux/macOS）？
   └── 更新策略（标准/OSS/静默）？

2. 症状匹配
   ├── 优先：python3 scripts/search.py "<症状>" --domain issue
   │   └── 匹配到 → 给出根因 + 修复 + 代码
   └── 未匹配 → 降级到 reference.md 全文搜索

3. 提供修复
   ├── 具体的代码修改、配置调整、版本升级建议
   └── 预防措施（如何避免再发生）

4. 验证
   └── 确认修复后问题解决
```

## 症状搜索（推荐）

优先使用 BM25 搜索引擎精确匹配已知问题，而不是在 reference.md 中手动查找：

```bash
# 自然语言搜索已知问题
python3 skills/generalupdate-troubleshoot/scripts/search.py "升级后应用启动不了" --domain issue
python3 skills/generalupdate-troubleshoot/scripts/search.py "方法找不到 MethodNotFound" --domain issue
python3 skills/generalupdate-troubleshoot/scripts/search.py "中文乱码 garbled" --domain issue

# 搜索策略相关问题
python3 skills/generalupdate-troubleshoot/scripts/search.py "OSS 权限问题" --domain strategy
```

## 症状分级

reference.md 中的问题按严重度分级：

| 级别 | 颜色 | 含义 | 数量 |
|:----:|:----:|------|:----:|
| C | 🔴 **致命** | 阻断性故障、数据损坏、安全漏洞 | 8 |
| H | 🟠 **高** | 场景阻断、功能失效、需要升级 | 11 |
| M | 🟡 **中** | 功能异常、需要配置调整 | 20 |
| L | 🔵 **低** | 代码气味、边缘情况、已知行为 | 12 |

**完整清单请查阅 `reference.md`**

---

## ✅ 通用诊断前检查清单

在深入诊断前，先快速排查最常见的原因：

### 运行环境检查
- [ ] 目标机器安装了正确的 .NET 运行时（版本与发布框架匹配）
- [ ] 目标机器上有写入权限（InstallPath 目录可写）
- [ ] 防火墙未阻断 UpdateUrl 的通信端口
- [ ] 磁盘空间充足（至少 2× 更新包大小）
- [ ] Linux/macOS：UpgradeApp 有 `chmod +x` 执行权限

### 版本检查
- [ ] Client 和 Upgrade 项目 NuGet 版本**完全一致**
- [ ] 服务端返回的版本号是 4 段式（如 1.0.0.0）
- [ ] manifest.json 中 `mainAppName` 与实际进程名匹配
- [ ] `AppType` 设置正确（Client = 1, Upgrade = 2）

### 配置检查
- [ ] `Configinfo` 的 6 个必填字段都已设置
- [ ] `UpdateUrl` 可通过 HTTP GET 访问并返回合法 JSON
- [ ] `AppSecretKey` 与服务端配置一致（长度 ≥ 16 字符）
- [ ] UpgradeApp.exe 存在于发布目录的 `update/` 子目录中

### 日志检查
- [ ] 查看 `Logs/generalupdate-trace-*.log`（如有）
- [ ] 检查事件监听中的 `ExceptionEventArgs`
- [ ] 检查 `MultiDownloadErrorEventArgs` 中的异常

---

## ⚠️ 诊断阶段的反模式

| # | 反模式 | 后果 | 正确做法 |
|---|--------|------|---------|
| 1 | **只看错误信息不看事件** | 错过 ExceptionEventArgs 中的详细信息 | 订阅所有 6 个事件 |
| 2 | **日志文件路径不对就认为无日志** | 漏掉关键诊断信息 | 在 InstallPath/Logs 下查找 |
| 3 | **只检查 Client 不检查 Upgrade 进程** | 问题在 Upgrade 端但诊断方向全错 | 两端都要检查 |
| 4 | **升级问题直接改代码** | 可能是服务端配置问题而非客户端 Bug | 优先检查服务端返回的版本信息 |
| 5 | **忽略 NuGet 版本一致性** | 方向错，"Method not found" 根因是版本不一致 | 第一个就要检查版本 |
| 6 | **只在 Debug 环境测试** | Release 环境可能缺少运行时文件 | 在发布/生产环境复现
