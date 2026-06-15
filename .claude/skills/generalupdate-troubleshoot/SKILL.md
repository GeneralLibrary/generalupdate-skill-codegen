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

## 工作流程

```
1. 症状收集
   ├── 用户描述的症状是什么？
   ├── 错误信息/堆栈是什么？
   ├── GeneralUpdate 版本号？
   ├── 平台（Windows/Linux/macOS）？
   └── 更新策略（标准/OSS/静默）？

2. 症状匹配 → 查找 `reference.md`
   ├── 找到匹配的 Q/C/H/M/L → 给出根因 + 修复 + 代码
   └── 未找到匹配 → 执行通用诊断流程（6步骤）

3. 提供修复
   ├── 具体的代码修改、配置调整、版本升级建议
   └── 预防措施（如何避免再发生）

4. 验证
   └── 确认修复后问题解决
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
