---
name: generalupdate-troubleshoot
description: |
  Diagnostic reference for common GeneralUpdate integration problems, powered by real
  GitHub and Gitee issue data. Covers 20+ symptoms with root causes and fixes.
  Triggers on: "update failed", "update not working", "升级失败", "更新失败",
  "升级进程没启动", "下载没反应", "版本号不对", "报错", "IPC error",
  "upgrade.exe not found", "file locked", "patch error", "silent mode not working",
  "multi-level folder structure update messed up", "Chinese filename garbled",
  "增量更新报错", "文件占用", "版本号异常", "process exit not firing",
  "bowl crash", "bowl not working", "method not found", "nuget error",
  "troubleshoot", "diagnose", "排查", "问题", "bug".
  Load this skill whenever the user reports an error, unexpected behavior, or failure.
when_to_use: |
  - User reports an error or failure during update
  - User says "it's not working" or "something went wrong"
  - User describes specific error messages or symptoms
  - User asks about known issues or bugs
  - User asks for debugging/diagnostic help
  - Used after generalupdate-init or generalupdate-ui when things go wrong
allowed-tools: "Read, Write, Edit, Glob, Grep, Bash"
---

# 🩺 GeneralUpdate 故障排查

基于真实 GitHub/Gitee Issues 的综合性诊断清单。

## 工作流程

1. **症状匹配** — 分析用户描述的错误症状，匹配已知问题清单
2. **诊断追问** — 如果症状不清晰，询问关键的诊断信息：
   - 使用 GeneralUpdate 版本号？
   - 在哪个平台（Windows/Linux/Mac）？
   - 用的是哪种更新策略（标准/OSS/静默）？
   - 工作目录是什么？manifest 文件存在吗？
   - 完整的错误堆栈是什么？
3. **提供修复** — 按问题给出修复方案、代码修改或配置调整
4. **预防建议** — 告知如何避免类似问题

## 症状 → 根因 → 修复 清单

见 `reference.md`
