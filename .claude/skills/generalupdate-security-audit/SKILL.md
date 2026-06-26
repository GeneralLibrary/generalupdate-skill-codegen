---
name: generalupdate-security-audit
description: |
  Security audit guide for GeneralUpdate deployments. Covers IPC encryption,
  AppSecretKey strength, HTTPS enforcement, cross-tenant isolation, ZipSlip
  protection, and dependency vulnerability scanning. Generates audit report
  with severity ratings and remediation steps.
  Triggers on: "security", "audit", "安全审计", "安全审查", "漏洞",
  "vulnerability", "penetration", "渗透测试", "encryption", "IPC encryption",
  "AppSecretKey", "HTTPS", "cross-tenant", "ZipSlip", "路径穿越",
  "hardcoded key", "硬编码", "密钥泄露", "信息泄露", "privilege",
  "权限提升", "security review", "安全检查".
when_to_use: |
  - User asks about security of their GeneralUpdate deployment
  - User wants to audit their update pipeline for vulnerabilities
  - User is deploying in a multi-tenant environment
  - User's security team requires a compliance review
  - User mentions penetration testing or security assessment
  - User is using GeneralUpdate in a regulated industry (finance, healthcare, gov)
  - Run AFTER generalupdate-init as a post-integration check
allowed-tools: "Read, Write, Edit, Glob, Grep"
---

# 🔒 GeneralUpdate Security Audit Guide / GeneralUpdate 安全审计指南

Comprehensive coverage of security risk surfaces for GeneralUpdate deployments. Based on code audit findings (17 CRITICAL/HIGH items) and best practices.

全面覆盖 GeneralUpdate 部署的安全风险面。基于代码审计发现（17 CRITICAL/HIGH 项）和最佳实践。

---

## 📋 Pre-Audit Information Collection / 审计前信息收集

Collect the following information before beginning the audit. **Ask the user when anything is unclear:**

在开始审计之前收集以下信息。**如有不明确之处，请向用户确认：**

```
### Deployment Environment / 部署环境
- Deployment mode / 部署模式: ______ (Intranet 内网 / Public 公网 / Hybrid 混合)
- Tenant mode / 租户模式: ______ (Single-tenant 单租户 / Multi-tenant 多租户)
- Number of clients / 客户端数量: ______
- Client OS / 客户端操作系统: ______ (Windows / Linux / macOS / Mixed 混合)

### Server / 服务端
- Backend type / 后端类型: ______ (GeneralSpacestation / Custom 自定义 / OSS)
- Transport protocol / 传输协议: ______ (HTTP / HTTPS)
- Auth method / 认证方式: ______ (Bearer / Basic / HMAC / None 无)
- Is API publicly accessible / API 是否公开访问: ______ (Yes 是 / No 否, with network isolation 有网络隔离)

### Client / 客户端
- GeneralUpdate version / GeneralUpdate 版本: ______
- Using IPC / 是否使用 IPC: ______ (Yes 是 / No 否)
- Using Bowl / 是否使用 Bowl: ______ (Yes 是 / No 否)
- Using Differential / 是否使用 Differential: ______ (Yes 是 / No 否)
```

---

## Security Audit Matrix / 安全审计矩阵

| # | Check Item / 检查项 | Severity / 严重度 | Description / 描述 | Remediation / 修复措施 |
|---|--------|--------|------|---------|
| S01 | **AppSecretKey Strength / AppSecretKey 强度** | 🔴 CRITICAL | Key too short, alphabetic only, identical to sample code / 密钥长度不足、纯字母、与示例代码相同 | Use ≥ 32 chars random key with upper+lower+digits+symbols / 使用 ≥ 32 字符，大小写+数字+符号的随机密钥 |
| S02 | **IPC Encryption / IPC 加密** | 🔴 CRITICAL | Default IPC encryption key hardcoded in binary / 默认 IPC 加密密钥硬编码在二进制中 | Ensure AppSecretKey is unique and consistent across server and client / 确保 AppSecretKey 唯一且服务端/客户端一致 |
| S03 | **HTTPS Transport / HTTPS 传输** | 🟠 HIGH | UpdateUrl uses HTTP instead of HTTPS / UpdateUrl 使用 HTTP 而非 HTTPS | Enforce HTTPS in production; configure HSTS / 生产环境强制 HTTPS；配置 HSTS |
| S04 | **ZipSlip Path Traversal / ZipSlip 路径穿越** | 🔴 CRITICAL | ZIP extraction does not validate ../ paths / 解压 ZIP 时未验证 ../ 路径 | Validate that archive entry paths stay within the target directory / 验证压缩包条目路径是否在目标目录内 |
| S05 | **Multi-tenant Isolation / 多租户隔离** | 🔴 CRITICAL | Server does not isolate tenants by ProductId / 服务端未按 ProductId 隔离租户 | Add tenant identity verification middleware on server / 服务端添加租户身份验证中间件 |
| S06 | **Event Log Leakage / 事件日志泄露** | 🟡 MEDIUM | ExceptionEventArgs logs may contain sensitive paths / ExceptionEventArgs 日志可能包含敏感路径 | Sanitize before logging; filter paths and keys / 脱敏后记录，过滤路径和密钥 |
| S07 | **Differential Package Signing / 差分包签名** | 🟠 HIGH | Differential patches have no digital signature verification / 差分补丁无数字签名验证 | Apply Authenticode signing to update packages / 对更新包进行 Authenticode 签名 |
| S08 | **Temp Directory Permissions / 临时目录权限** | 🟡 MEDIUM | Temporary extraction directory permissions may be overly broad / 临时解压目录权限可能过大 | Set to current-user read/write only / 设置为仅为当前用户可读写 |
| S09 | **OSS Bucket Permissions / OSS Bucket 权限** | 🟠 HIGH | Update package storage bucket set to public-read / 更新包存储 Bucket 设为公共读 | Set to private; use pre-signed URLs / 设置为私有，使用预签名 URL |
| S10 | **Dependency Vulnerabilities / 依赖版本漏洞** | 🟡 MEDIUM | GeneralUpdate and its dependencies may have known CVEs / GeneralUpdate 及其依赖可能存在已知 CVE | Regularly check NuGet dependency security advisories / 定期检查 NuGet 依赖安全公告 |
| S11 | **Rollback Attack / 回滚攻击** | 🟠 HIGH | Attacker can submit a downgraded version to force old install / 攻击者可提交降级版本号强制安装旧版本 | Server validates version is monotonically increasing / 服务端校验版本号单调递增 |
| S12 | **Download Integrity / 下载完整性** | 🟠 HIGH | Downloaded update package has no integrity verification / 下载的更新包无完整性校验 | Ensure Pipeline includes HashMiddleware / 确保 Pipeline 包含 HashMiddleware |
| S13 | **Bowl Privilege Escalation / Bowl 提权** | 🟡 MEDIUM | Bowl crash daemon running with high privileges may be abused / Bowl 崩溃守护以高权限运行可能被滥用 | Run Bowl with least necessary privileges / 以最小必要权限运行 Bowl |
| S14 | **Information Leakage via Manifest / 信息泄露通过 manifest** | 🔵 LOW | ProductId and version numbers in manifest.json can be enumerated / manifest.json 中的 ProductId、版本号可被枚举 | Do not expose manifest files in non-public environments / 非公开环境下不暴露 manifest 文件 |

---

## Audit Report Output Format / 审计报告输出格式

After completing the audit, output in the following format:

完成审计后按以下格式输出：

```
## 🔒 GeneralUpdate Security Audit Report / GeneralUpdate 安全审计报告

### Summary / 概要
- Project / 项目: ______
- Audit date / 审计日期: ______
- Overall rating / 总体评分: A/B/C/D/F
- Critical issues / 严重问题: ______
- High risk / 高风险: ______
- Medium risk / 中风险: ______
- Low risk / 低风险: ______

### Critical Issues (Must fix immediately) / 严重问题（必须立即修复）
- S01 AppSecretKey Strength / AppSecretKey 强度: ⚠️ Current key length is X, needs ≥ 32 / 当前密钥长度为 X，需要 ≥ 32
  Fix / 修复: ______

### High Risk (Fix as soon as possible) / 高风险（建议尽快修复）
...

### Medium Risk (Assess then fix) / 中风险（评估后修复）
...

### Low Risk (Documented) / 低风险（记录在案）
...

### Remediation Priority / 修复建议优先级
1. Immediate / 立即：S01, S03, S04
2. This week / 本周：S05, S07, S09
3. This month / 本月：S08, S10, S11
```

---

## Security Configuration Checklist / 安全配置检查清单

- [ ] AppSecretKey length ≥ 32 chars, mixed upper+lower+digits+symbols / AppSecretKey 长度 ≥ 32 字符，混合大小写+数字+符号
- [ ] HTTPS in production / 生产环境使用 HTTPS
- [ ] IPC file encoding set to Encoding.UTF8 / IPC 文件编码设为 Encoding.UTF8
- [ ] Pipeline includes HashMiddleware for integrity verification / Pipeline 包含 HashMiddleware 做完整性校验
- [ ] OSS Bucket permissions set to private / OSS Bucket 权限设为私有
- [ ] Server isolates tenants by ProductId / 服务端按 ProductId 隔离租户
- [ ] Version numbers strictly monotonically increasing / 版本号严格单调递增
- [ ] Update packages signed with Authenticode / 更新包进行 Authenticode 签名
- [ ] Zip extraction has path traversal protection / Zip 解压有路径穿越防护
- [ ] No sensitive information recorded in logs / 日志中不记录敏感信息

---

## Related Skills / 相关技能

- `/generalupdate-init` — Fix issues found in audit / 修复审计发现的问题
- `/generalupdate-advanced` — IPC replacement, custom authentication / IPC 替换、自定义认证
- `/generalupdate-troubleshoot` — Reference for known security issues / 已知安全问题参考
