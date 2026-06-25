# CLAUDE.md

This file provides guidance to AI agents (Claude Code, Cursor, etc.) when working with this repository.

## Project Overview

GeneralUpdate Skill CodeGen is a **Claude Code skill suite** for integrating [GeneralUpdate](https://github.com/GeneralLibrary/GeneralUpdate) (.NET auto-update) into any .NET application. It provides code generation, strategy decision tree, troubleshooting search, and multi-platform AI support.

## Quick Start (for AI agents)

When the user asks about GeneralUpdate or .NET auto-update:

1. **Root SKILL.md** is the entry point — read it first for the developer roadmap
2. **7 sub-skills** each have their own SKILL.md with step-by-step workflow
3. **Use the search engine** for troubleshooting (BM25, Python):
   ```bash
   python3 .claude/skills/generalupdate-troubleshoot/scripts/search.py "<symptom>" --domain issue
   ```
4. **Use the code generator** for production code:
   ```bash
   python3 .claude/scripts/generate.py --framework wpf --strategy oss --bowl
   ```

## Architecture

```
generalupdate-skill-codegen/
│
├── SKILL.md                       ← Entry point: developer roadmap + decision tree
├── RULES.md                       ← Technical rules (API, events, NuGet)
├── CLAUDE.md                      ← THIS FILE: AI agent guidance
│
├── .claude/
│   ├── skills/
│   │   ├── generalupdate-init/        ← Bootstrap + scaffolding
│   │   ├── generalupdate-ui/          ← Update UI windows
│   │   ├── generalupdate-strategy/    ← 6 strategy decision tree
│   │   ├── generalupdate-advanced/    ← Bowl, IPC, AOT, Pipeline
│   │   └── generalupdate-troubleshoot/ ← 50+ known issues + search engine
│   └── scripts/
│       └── generate.py               ← Parameterized code generator (336 combinations)
│
├── cli/                              ← CLI installer (gskill on npm)
│   ├── src/commands/init.ts          ← Install for any AI platform
│   ├── src/commands/generate.ts      ← Delegate to Python generator
│   ├── src/commands/uninstall.ts     ← Clean removal
│   └── src/utils/                    ← detect.ts, template.ts, github.ts, extract.ts
│
├── BUGS.md                           ← Full code audit
├── clinerules/                       ← Claude Code-specific rules
├── cursor/rules/                     ← Cursor-specific rules
│
└── .github/workflows/
    ├── ci.yml                        ← Validate builds + tests
    └── release.yml                   ← Publish CLI + GitHub Release
```

## Source of Truth Rules

**Edit only in these locations; everything else is auto-synced:**

| What | Where to Edit | Auto-Synced To |
|------|--------------|----------------|
| SKILL.md (root) | `SKILL.md` | - |
| Sub-skill content | `.claude/skills/*/SKILL.md` | - |
| Templates (.cs) | `.claude/skills/*/templates/` | `cli/assets/` (via sync) |
| Issues/strategies CSV | `.claude/skills/*/data/` | `cli/assets/data/` (via sync) |
| Python scripts | `.claude/skills/*/scripts/` | `cli/assets/scripts/` (via sync) |
| Code generator | `.claude/scripts/generate.py` | `cli/assets/scripts/` (via sync) |
| CLI source | `cli/src/` | Built to `cli/dist/` |
| Platform configs | `cli/assets/templates/platforms/` | - |

**Sync commands before release:**
```bash
# Sync all data/scripts to CLI assets
python3 .claude/scripts/_sync_all.py

# Or manually:
cp -r .claude/skills/generalupdate-troubleshoot/data/* cli/assets/data/
cp -r .claude/skills/generalupdate-troubleshoot/scripts/* cli/assets/scripts/
cp .claude/scripts/generate.py cli/assets/scripts/generate.py
```

## Search Commands

```bash
# Troubleshooting
python3 .claude/skills/generalupdate-troubleshoot/scripts/search.py "<query>" --domain issue

# Strategy lookup
python3 .claude/skills/generalupdate-troubleshoot/scripts/search.py "<query>" --domain strategy

# Code generation
python3 .claude/scripts/generate.py --list
python3 .claude/scripts/generate.py --framework wpf --strategy oss --bowl --project-name MyApp
```

## Testing

```bash
# Search engine tests (15 tests)
python3 .claude/skills/generalupdate-troubleshoot/scripts/tests/test_search.py

# CI validates: search, codegen, .NET compile, TypeScript
# See .github/workflows/ci.yml
```

## Git Workflow

Never push directly to `main`. Always:

1. Branch: `git checkout -b feat/...` or `fix/...`
2. Commit: conventional commits (feat:, fix:, docs:, chore:)
3. Push: `git push -u origin <branch>`
4. PR: Create PR against `main`

## Prerequisites

- Python 3.10+ (for search engine and code generator)
- .NET SDK 10.0+ (for template verification)
- Node.js 22+ (for CLI development)
