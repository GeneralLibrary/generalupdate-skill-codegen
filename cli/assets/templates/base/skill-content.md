# {{TITLE}}

{{DESCRIPTION}}

## When to Use

- You need to integrate GeneralUpdate auto-update into a .NET application
- You need Bootstrap configuration code (Minimal, Standard, or Full)
- You need the dual-project (Client + Upgrade) scaffolding
- You need update UI code for your framework
- You are troubleshooting update failures

## Learning Path

```
New to GeneralUpdate? Start here:
1. README.md → Developer roadmap + 5-question decision tree
2. GeneralUpdate skill suite may activate on keywords like "auto update"
3. If troubleshooting: search known issues with scripts/search.py
```

## Available Sub-Skills

| Skill | Description |
|-------|-------------|
| `generalupdate-init` | Bootstrap config + scaffolding |
| `generalupdate-ui` | Update UI for 6 frameworks |
| `generalupdate-strategy` | 6 strategy decision tree |
| `generalupdate-advanced` | Bowl, IPC, AOT, Pipeline |
| `generalupdate-troubleshoot` | 50+ known issues |

## Quick Start

```
python3 .claude/scripts/generate.py --framework wpf --strategy standard
```

This generates: Bootstrap.cs, manifest.json, UpgradeProgram.cs, DeploymentChecklist.md, IssuesWarning.md
