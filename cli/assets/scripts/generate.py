#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
GeneralUpdate Code Generator — generates production-ready C# integration code.
Usage: python3 scripts/generate.py --framework wpf --strategy oss --output ./Generated

Supports 336 combinations: 4 scenes × 6 strategies × 7 UI frameworks × 2 Bowl

Combinations:
  Scenes:    None, UpgradeOnly, MainOnly, Both
  Strategies: standard, oss, silent, differential, cvp, push
  Frameworks: wpf-原生, wpf-layui, wpf-wpfdevelopers, winforms-antdui, avalonia-semiursa, maui, console
  Bowl:      yes, no
"""
import argparse
import json
import os
import re
import sys
from pathlib import Path
from string import Template

SCRIPT_DIR = Path(__file__).parent
TEMPLATES_DIR = SCRIPT_DIR / "generate" / "templates"

# ============ CONFIG MATRIX ============

STRATEGIES = {
    "standard": {
        "name": "Standard Client-Server",
        "slug": "standard",
        "description": "Standard dual-process update with GeneralSpacestation backend",
    },
    "oss": {
        "name": "OSS Object Storage",
        "slug": "oss",
        "description": "Update via S3/MinIO/cloud object storage; no backend server needed",
        "warning": "OSS模式不区分 Main/Upgrade 更新包。Upgrade.exe 必须放在 update/ 子目录。",
    },
    "silent": {
        "name": "Silent Update",
        "slug": "silent",
        "description": "Background polling update with minimal user interruption",
    },
    "differential": {
        "name": "Differential Update",
        "slug": "differential",
        "description": "Delta patch update to save bandwidth (BSDIFF/HDiffPatch)",
        "warning": "差分包大小建议不超过 2GB，避免 BSDIFF 整数溢出（v10.5.0-rc.1 已修复 #514）。",
    },
    "cvp": {
        "name": "Cross-Version CVP",
        "slug": "cvp",
        "description": "Skip intermediate versions and jump directly to target version",
        "warning": "跨版本跳转需要服务端 API 兼容性验证，避免跳转后 API 不匹配。",
    },
    "push": {
        "name": "SignalR Push",
        "slug": "push",
        "description": "Server-initiated push update via SignalR real-time connection",
        "warning": "HubConnection Dispose 后必须置 null。离线客户端可能错过推送。",
    },
}

UI_FRAMEWORKS = {
    "wpf-原生": {"class": "WpfNative", "uses_xaml": True, "needs_dispatcher": True},
    "wpf-layui": {"class": "WpfLayUI", "uses_xaml": True, "needs_dispatcher": True},
    "wpf-wpfdevelopers": {"class": "WpfDevelopers", "uses_xaml": True, "needs_dispatcher": True},
    "winforms-antdui": {"class": "WinFormsAntdUI", "uses_xaml": False, "needs_dispatcher": True},
    "avalonia-semiursa": {"class": "AvaloniaSemiUrsa", "uses_xaml": True, "needs_dispatcher": True},
    "maui": {"class": "MauiApp", "uses_xaml": True, "needs_dispatcher": False},
    "console": {"class": "ConsoleApp", "uses_xaml": False, "needs_dispatcher": False},
}


def load_template(name):
    path = TEMPLATES_DIR / name
    if not path.exists():
        print(f"⚠️ Template not found: {path}")
        return ""
    return path.read_text(encoding="utf-8")


def render(template_text, variables):
    """Simple {{PLACEHOLDER}} substitution with defaults for optional sections."""
    result = template_text
    for key, value in variables.items():
        result = result.replace("{{" + key + "}}", str(value))

    # Handle optional sections: {{#KEY}}content{{/KEY}} or {{^KEY}}content{{/KEY}}
    # Simple positive conditional blocks
    def replace_conditional(match):
        key = match.group(1)
        content = match.group(2)
        if variables.get(key, False) and str(variables.get(key, "")).lower() in ("true", "yes", "1"):
            return content
        return ""

    result = re.sub(r"\{\{#(\w+)\}\}(.*?)\{\{/\1\}\}", replace_conditional, result, flags=re.DOTALL)

    # Negative conditional blocks {{^KEY}}...{{/KEY}}
    def replace_negative(match):
        key = match.group(1)
        content = match.group(2)
        if not variables.get(key, False) or str(variables.get(key, "")).lower() in ("false", "no", "0", ""):
            return content
        return ""

    result = re.sub(r"\{\{\^(\w+)\}\}(.*?)\{\{/\1\}\}", replace_negative, result, flags=re.DOTALL)

    return result


def generate_bootstrap(strategy, framework, with_bowl, scenes, variables):
    """Generate Bootstrap.cs integration code."""
    templ = load_template("Bootstrap.cs.template")

    listener_blocks = []

    if framework == "console":
        # Console just writes to stdout
        listeners_templ = load_template("listeners_console.cs.template")
    elif framework.startswith("wpf") or framework == "avalonia-semiursa":
        listeners_templ = load_template("listeners_mvvm.cs.template")
    elif framework == "winforms-antdui":
        listeners_templ = load_template("listeners_winforms.cs.template")
    elif framework == "maui":
        listeners_templ = load_template("listeners_maui.cs.template")
    else:
        listeners_templ = load_template("listeners_console.cs.template")

    listeners_code = render(listeners_templ, variables)

    bowl_notice = ""
    if with_bowl:
        bowl_notice = load_template("bowl_notice.cs.template")
        bowl_notice = render(bowl_notice, variables)

    strategy_notice = ""
    if strategy in STRATEGIES and STRATEGIES[strategy].get("warning"):
        strategy_notice = f"// ⚠️ {STRATEGIES[strategy]['warning']}\n"

    code = render(templ, {
        **variables,
        "LISTENERS": listeners_code,
        "BOWL_NOTICE": bowl_notice,
        "STRATEGY_WARNING": strategy_notice,
    })

    return code


def generate_manifest(variables):
    """Generate generalupdate.manifest.json."""
    templ = load_template("manifest.json.template")
    return render(templ, variables)


def generate_upgrade_program(variables):
    """Generate UpgradeProgram.cs."""
    templ = load_template("UpgradeProgram.cs.template")
    return render(templ, variables)


def generate_deployment_checklist(strategy, framework, with_bowl, variables):
    """Deployment checklist Markdown."""
    templ = load_template("DeploymentChecklist.md.template")
    return render(templ, variables)


def generate_issue_warnings(strategy, variables):
    """Generate known issue warnings for the specific config combination."""
    warnings_map = {
        "oss": """⚠️ OSS 特有已知问题:
  - H4: OSS 不区分 Main/Upgrade 更新包，接受此行为
  - H5: Upgrade.exe 必须放在 update/ 子目录
  - L7: 示例代码中 OSS endpoint/bucket 写死，建议用环境变量
  - M13: OssClient.AppType 值 3-4 在 v10.5.0-rc.1 中可用""",
        "silent": """⚠️ 静默更新特有已知问题:
  - H2: 无限升级循环 — 确保 manifest.json 版本号正确
  - M19: 静默通知可能不尊重系统的免打扰设置
  - M9: 升级进程超时 — 大文件操作建议增加超时时间""",
        "differential": """⚠️ 差分更新特有已知问题:
  - C3: BSDIFF 整数溢出 — 差分包 < 2GB
  - M1: 不要额外引用 GeneralUpdate.Differential（已嵌入 Core）
  - L3: 差分 clean/dirty 参数缺少验证，建议手动检查路径
  - L5: 进程内存跟踪使用 private bytes 而非 working set""",
        "cvp": """⚠️ CVP 跨版本特有已知问题:
  - H8: 跨版本跳转跳过 API 兼容性检查 — 服务端需要验证
  - C7: 多租户跨租户版本泄露风险 — 确认 ProductId 隔离""",
        "push": """⚠️ SignalR 推送特有已知问题:
  - H10: HubConnection Dispose 后重连崩溃 — 置 null
  - M11: 推送更新无送达确认 — 建议实现 ACK 机制
  - M9: 慢操作超时场景下连接可能断开""",
        "standard": """⚠️ 标准策略已知问题（非特有但常见）:
  - C1: UpgradeApp.exe 必须随首个版本发布
  - C2: Client/Upgrade NuGet 版本必须一致
  - M5: InstallPath 使用相对路径导致文件解析失败
  - M6: UpdateUrl 返回空响应体时做 null 检查""",
    }
    warning = warnings_map.get(strategy, "该策略组合暂无特别预警。")

    t = load_template("IssuesWarning.md.template")
    return render(t, {**variables, "WARNINGS_LIST": warning})


def generate_strategy_checks(strategy):
    """Return strategy-specific checklist items."""
    checks = {
        "oss": """- [ ] Bucket 权限设置为私有
- [ ] Upgrade.exe 放在 update/ 子目录
- [ ] 接受 OSS 不区分 Main/Upgrade 的限制
- [ ] 包名包含版本号 (如 MyApp_1.0.0.0.zip)""",
        "silent": """- [ ] 轮询间隔 30-60 分钟
- [ ] 下载完成后通知用户重启
- [ ] 有 WiFi/流量限制考虑""",
        "differential": """- [ ] 服务端有差分包生成机制
- [ ] Pipeline 配置了 PatchMiddleware
- [ ] 差分包 < 2GB（避免 BSDIFF 整数溢出）
- [ ] Linux/macOS 补丁兼容性已验证""",
        "cvp": """- [ ] 服务端有 CVP 构建流水线
- [ ] 源/目标版本间 API 兼容性已验证
- [ ] 客户端数据库迁移已测试""",
        "push": """- [ ] HubConnection 生命周期管理
- [ ] 自动重连逻辑（3次，间隔递增）
- [ ] Dispose 时将连接置 null
- [ ] 推送失败降级到轮询""",
        "standard": """- [ ] GeneralSpacestation 或兼容后端已部署
- [ ] API 返回合法的版本列表
- [ ] 4 段式版本号返回""",
    }
    return checks.get(strategy, "- [ ] 基本配置已验证")


def generate(args):
    strategy = args.strategy
    framework = args.framework
    with_bowl = args.bowl
    scenes = args.scenes

    # Validate scenes
    VALID_SCENES = {"None", "UpgradeOnly", "MainOnly", "Both"}
    if scenes not in VALID_SCENES:
        print(f"❌ Invalid --scenes value '{scenes}'. Must be one of: {', '.join(sorted(VALID_SCENES))}")
        sys.exit(1)

    output_dir = Path(args.output)
    project_name = args.project_name or "MyApp"
    app_secret = args.app_secret_key or "CHANGE-ME-TO-A-32-CHAR-SECRET-KEY!"
    update_url = args.update_url or "https://your-server.com/Upgrade/Verification"
    version = args.version or "1.0.0.0"
    product_id = args.product_id or project_name.lower().replace(" ", "-") + "-001"

    from datetime import date
    today = date.today().isoformat()

    report_url = (args.report_url or "").strip() or update_url.rstrip('/').rstrip('/check').rstrip('/Verification').rstrip('/') + "/Report"

    bowl_lower = "yes" if with_bowl else "no"
    variables = {
        "PROJECT_NAME": project_name,
        "APP_SECRET_KEY": app_secret,
        "UPDATE_URL": update_url,
        "REPORT_URL": report_url,
        "CLIENT_VERSION": version,
        "PRODUCT_ID": product_id,
        "STRATEGY": strategy,
        "STRATEGY_NAME": STRATEGIES.get(strategy, {}).get("name", strategy),
        "FRAMEWORK": framework,
        "FRAMEWORK_CLASS": UI_FRAMEWORKS.get(framework, {}).get("class", "App"),
        "BOWL": bowl_lower,
        "BOWL_UPPER": "Yes" if with_bowl else "No",
        "SCENES": scenes,
        "INSTALL_PATH": "AppDomain.CurrentDomain.BaseDirectory",
        "DATE": today,
        "STRATEGY_CHECKS": generate_strategy_checks(strategy),
    }
    # ISSUE_WARNINGS depends on variables being fully constructed
    variables["ISSUE_WARNINGS"] = generate_issue_warnings(strategy, variables)

    # Generate files
    files = {}

    # Bootstrap
    bootstrap_code = generate_bootstrap(strategy, framework, with_bowl, scenes, variables)
    files["Client/Integration.cs"] = bootstrap_code

    # Manifest
    manifest_json = generate_manifest(variables)
    files["generalupdate.manifest.json"] = manifest_json

    # Upgrade program
    upgrade_code = generate_upgrade_program(variables)
    files["Upgrade/UpgradeProgram.cs"] = upgrade_code

    # Deployment checklist
    checklist = generate_deployment_checklist(strategy, framework, with_bowl, variables)
    files["DeploymentChecklist.md"] = checklist

    # Issue warnings
    warnings = generate_issue_warnings(strategy, variables)
    files["IssuesWarning.md"] = warnings

    # Write files
    for relpath, content in files.items():
        full_path = output_dir / relpath
        full_path.parent.mkdir(parents=True, exist_ok=True)
        full_path.write_text(content, encoding="utf-8")
        print(f"  ✓ {relpath}")

    print(f"\n✅ Generated {len(files)} files for {STRATEGIES[strategy]['name']} + {framework}")
    print(f"   Output: {output_dir.resolve()}")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="GeneralUpdate Code Generator")
    parser.add_argument("--framework", "-f", choices=list(UI_FRAMEWORKS.keys()), default="wpf-原生",
                        help="Target UI framework")
    parser.add_argument("--strategy", "-s", choices=list(STRATEGIES.keys()), default="standard",
                        help="Update strategy")
    parser.add_argument("--bowl", action="store_true", default=False,
                        help="Include Bowl crash daemon")
    parser.add_argument("--scenes", default="Both",
                        help="Update scenes: None/UpgradeOnly/MainOnly/Both (default: Both)")
    parser.add_argument("--output", "-o", default="./Generated",
                        help="Output directory (default: ./Generated)")
    parser.add_argument("--project-name", "-n", default="MyApp",
                        help="Project name (default: MyApp)")
    parser.add_argument("--app-secret-key", help="AppSecretKey (min 32 chars)")
    parser.add_argument("--update-url", help="Update API URL")
    parser.add_argument("--report-url", help="Report API URL (default: derived from update-url)")
    parser.add_argument("--version", "-v", default="1.0.0.0",
                        help="Client version (default: 1.0.0.0)")
    parser.add_argument("--product-id", help="Product ID (default: <project-name>-001)")
    parser.add_argument("--list", action="store_true", help="List all available combinations")

    args = parser.parse_args()

    if args.list:
        print("Available strategies:")
        for k, v in STRATEGIES.items():
            print(f"  {k:15s} - {v['name']}")
        print("\nAvailable UI frameworks:")
        for k, v in UI_FRAMEWORKS.items():
            print(f"  {k:20s}")
        print(f"\nTotal combinations: {len(STRATEGIES)} strategies × {len(UI_FRAMEWORKS)} frameworks × 2 Bowl × 4 scenes = {len(STRATEGIES) * len(UI_FRAMEWORKS) * 2 * 4}")
        sys.exit(0)

    generate(args)
