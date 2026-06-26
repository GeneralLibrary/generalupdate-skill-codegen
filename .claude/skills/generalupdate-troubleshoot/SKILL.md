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

# GeneralUpdate Troubleshooting / GeneralUpdate 故障排查

A comprehensive diagnostic system covering 50+ known issues, all traceable to GitHub/Gitee Issues or code audit findings.

综合性诊断系统 — 覆盖 50+ 已知问题，均可追溯到 GitHub/Gitee Issue 或代码审计发现。

---

## User Symptom Extraction (Must Collect Before Diagnosis) / 用户症状提取（诊断前必须收集）

Collect the following required and optional information before starting diagnosis. Accurate and complete symptom data dramatically reduces time-to-fix.

在开始诊断前收集以下必填和可选信息。准确完整的症状数据能大幅缩短修复时间。

```
### Required / 必填信息

- Symptom description / 症状描述: ______
- Error message / stack trace / 错误信息/堆栈: ______
- GeneralUpdate version / GeneralUpdate 版本: ______
- Platform / 平台: ______（Windows / Linux / macOS）
- .NET version / .NET 版本: ______
- Update strategy / 更新策略: ______（Standard / OSS / Silent / Differential / Cross-version / Push）
    （标准 / OSS / 静默 / 差分 / 跨版本 / 推送）
- Recently changed configuration? / 最近是否改过配置: ______（Yes/No, what was changed?）
    （是/否，改了啥）

### Optional / 可选信息

- Exception in event listeners (ExceptionEventArgs)? / 事件监听中是否有异常（ExceptionEventArgs）: ______
- Logs available (Logs/generalupdate-trace *.log)? / 是否有日志（Logs/generalupdate-trace *.log）: ______
- Is the issue reproducible? / 问题是否可复现: ______（Yes/No, frequency / 是/否，频率）
- When did it first appear? / 首次出现时间点: ______
```

---

## Workflow / 工作流程

The diagnostic workflow follows four stages: collect symptoms, match against known issues, provide fixes, and verify resolution. Prioritize the BM25 search engine for precise matching before falling back to manual reference lookup.

诊断工作流程分为四个阶段：收集症状、匹配已知问题、提供修复方案、验证解决。优先使用 BM25 搜索引擎精确匹配已知问题，匹配不到再降级为手动查找参考文档。

```
1. Symptom Collection / 症状收集
   ├── What symptom did the user describe? / 用户描述的症状是什么？
   ├── What is the error message / stack trace? / 错误信息/堆栈是什么？
   ├── GeneralUpdate version? / GeneralUpdate 版本号？
   ├── Platform (Windows/Linux/macOS)? / 平台（Windows/Linux/macOS）？
   └── Update strategy (Standard/OSS/Silent)? / 更新策略（标准/OSS/静默）？

2. Symptom Matching / 症状匹配
   ├── Priority: python3 scripts/search.py "<symptom>" --domain issue
   │   │   优先：python3 scripts/search.py "<症状>" --domain issue
   │   └── Match found → provide root cause + fix + code / 匹配到 → 给出根因 + 修复 + 代码
   └── No match → fallback to full-text search in reference.md
       └── 未匹配 → 降级到 reference.md 全文搜索

3. Provide Fix / 提供修复
   ├── Concrete code changes, configuration adjustments, version upgrade recommendations
   │   具体的代码修改、配置调整、版本升级建议
   └── Preventive measures (how to avoid recurrence) / 预防措施（如何避免再发生）

4. Verification / 验证
   └── Confirm the issue is resolved after applying the fix
       确认修复后问题解决
```

## Symptom Search (Recommended) / 症状搜索（推荐）

Prefer using the BM25 search engine for precise matching of known issues, rather than manually searching through reference.md.

优先使用 BM25 搜索引擎精确匹配已知问题，而不是在 reference.md 中手动查找：

```bash
# Natural language search for known issues
# 自然语言搜索已知问题
python3 skills/generalupdate-troubleshoot/scripts/search.py "app won't start after update" --domain issue
python3 skills/generalupdate-troubleshoot/scripts/search.py "升级后应用启动不了" --domain issue
python3 skills/generalupdate-troubleshoot/scripts/search.py "MethodNotFound exception" --domain issue
python3 skills/generalupdate-troubleshoot/scripts/search.py "方法找不到 MethodNotFound" --domain issue
python3 skills/generalupdate-troubleshoot/scripts/search.py "Chinese garbled text" --domain issue
python3 skills/generalupdate-troubleshoot/scripts/search.py "中文乱码 garbled" --domain issue

# Search strategy-related issues
# 搜索策略相关问题
python3 skills/generalupdate-troubleshoot/scripts/search.py "OSS permission issue" --domain strategy
python3 skills/generalupdate-troubleshoot/scripts/search.py "OSS 权限问题" --domain strategy
```

## Severity Classification / 症状分级

Issues in reference.md are classified by severity. Match the severity level to prioritize your response — Critical issues demand immediate action.

reference.md 中的问题按严重度分级。根据严重级别确定响应优先级 — 致命问题需要立即处理。

| Level / 级别 | Color / 颜色 | Meaning / 含义 | Count / 数量 |
|:----:|:----:|------|:----:|
| C | 🔴 **Critical / 致命** | Blocking failures, data corruption, security vulnerabilities / 阻断性故障、数据损坏、安全漏洞 | 8 |
| H | 🟠 **High / 高** | Scenario-blocking, feature broken, requires upgrade / 场景阻断、功能失效、需要升级 | 11 |
| M | 🟡 **Medium / 中** | Functional anomaly, requires configuration adjustment / 功能异常、需要配置调整 | 20 |
| L | 🔵 **Low / 低** | Code smell, edge case, known behavior / 代码气味、边缘情况、已知行为 | 12 |

**Refer to `reference.md` for the complete list / 完整清单请查阅 `reference.md`**

---

## Pre-Diagnostic Checklist / 通用诊断前检查清单

Before diving into deep diagnostics, quickly rule out the most common causes. This checklist catches approximately 60% of reported issues.

在深入诊断前，先快速排查最常见的原因。此检查清单能捕获约 60% 的上报问题。

### Runtime Environment Check / 运行环境检查

- [ ] Target machine has the correct .NET runtime installed (matching the publish framework)
      目标机器安装了正确的 .NET 运行时（版本与发布框架匹配）
- [ ] Target machine has write permission (InstallPath directory is writable)
      目标机器上有写入权限（InstallPath 目录可写）
- [ ] Firewall does not block communication to UpdateUrl port
      防火墙未阻断 UpdateUrl 的通信端口
- [ ] Sufficient disk space (at least 2x the update package size)
      磁盘空间充足（至少 2× 更新包大小）
- [ ] Linux/macOS: UpgradeApp has `chmod +x` execute permission
      Linux/macOS：UpgradeApp 有 `chmod +x` 执行权限

### Version Check / 版本检查

- [ ] Client and Upgrade project NuGet versions are **exactly identical**
      Client 和 Upgrade 项目 NuGet 版本**完全一致**
- [ ] Server returns version numbers in 4-segment format (e.g., 1.0.0.0)
      服务端返回的版本号是 4 段式（如 1.0.0.0）
- [ ] `mainAppName` in manifest.json matches the actual process name
      manifest.json 中 `mainAppName` 与实际进程名匹配
- [ ] `AppType` is set correctly (Client = 1, Upgrade = 2)
      `AppType` 设置正确（Client = 1, Upgrade = 2）

### Configuration Check / 配置检查

- [ ] All 6 required fields of `UpdateRequest` are set
      `UpdateRequest` 的 6 个必填字段都已设置
- [ ] `UpdateUrl` is reachable via HTTP GET and returns valid JSON
      `UpdateUrl` 可通过 HTTP GET 访问并返回合法 JSON
- [ ] `AppSecretKey` matches the server configuration (length >= 16 characters)
      `AppSecretKey` 与服务端配置一致（长度 ≥ 16 字符）
- [ ] UpgradeApp.exe exists in the `update/` subdirectory of the publish output
      UpgradeApp.exe 存在于发布目录的 `update/` 子目录中

### Log Check / 日志检查

- [ ] Check `Logs/generalupdate-trace-*.log` if available
      查看 `Logs/generalupdate-trace-*.log`（如有）
- [ ] Check `ExceptionEventArgs` in event listeners
      检查事件监听中的 `ExceptionEventArgs`
- [ ] Check exceptions in `MultiDownloadErrorEventArgs`
      检查 `MultiDownloadErrorEventArgs` 中的异常

---

## Diagnostic Anti-Patterns / 诊断阶段的反模式

These common mistakes waste time and lead to incorrect conclusions. Avoid them at all costs.

这些常见错误会浪费时间并导致错误结论。务必避免。

| # | Anti-Pattern / 反模式 | Consequence / 后果 | Correct Approach / 正确做法 |
|---|--------|------|---------|
| 1 | **Only checking error messages, ignoring events**
      **只看错误信息不看事件** | Misses detailed information in ExceptionEventArgs
      错过 ExceptionEventArgs 中的详细信息 | Subscribe to all 6 events
      订阅所有 6 个事件 |
| 2 | **Assuming no logs exist because the path is wrong**
      **日志文件路径不对就认为无日志** | Misses critical diagnostic information
      漏掉关键诊断信息 | Look under InstallPath/Logs
      在 InstallPath/Logs 下查找 |
| 3 | **Only checking Client, not the Upgrade process**
      **只检查 Client 不检查 Upgrade 进程** | Problem is in Upgrade but diagnosis goes in wrong direction
      问题在 Upgrade 端但诊断方向全错 | Check both sides
      两端都要检查 |
| 4 | **Modifying code directly for upgrade issues**
      **升级问题直接改代码** | Could be a server configuration issue, not a client bug
      可能是服务端配置问题而非客户端 Bug | Check server-returned version info first
      优先检查服务端返回的版本信息 |
| 5 | **Ignoring NuGet version consistency**
      **忽略 NuGet 版本一致性** | Wrong direction; "Method not found" root cause is version mismatch
      方向错，"Method not found" 根因是版本不一致 | Check versions first
      第一个就要检查版本 |
| 6 | **Only testing in Debug environment**
      **只在 Debug 环境测试** | Release environment may lack runtime files
      Release 环境可能缺少运行时文件 | Reproduce in publish/production environment
      在发布/生产环境复现 |

---

## Related Skills / 相关技能

- `/generalupdate-init` — Bootstrap configuration fixes / Bootstrap 配置修复
- `/generalupdate-ui` — Update UI issues / 更新界面问题
- `/generalupdate-strategy` — Strategy-specific failures / 策略特定故障
- `/generalupdate-advanced` — Advanced extension point issues / 高级扩展点问题
- `/generalupdate-migration` — Migration-related errors / 迁移相关错误
- `/generalupdate-mobile` — Mobile update issues / 移动端更新问题
- `/generalupdate-security-audit` — Security-related findings / 安全相关发现
