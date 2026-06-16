# Architecture & Data Flow

## Three Locations, One Source of Truth

| Location | Role | How to Maintain |
|----------|------|----------------|
| **`.claude/skills/`** | **Primary source** — all SKILL.md, templates, data, and scripts live here. Claude Code and Cursor read from here. | ✅ **Edit everything here.** This is the single source of truth. |
| **`cli/assets/`** | **CLI bundle** — shipped with `gskill-cli` npm package. Users who install via `npx gskill init` get a copy. | 🔄 **Sync before release:** `python3 .claude/scripts/_sync_all.py` copies from `.claude/skills/` to `cli/assets/` |
| **`<project>/.claude/skills/`** | **User project install** — created when user runs `gskill init --ai claude`. | ⏬ Created by CLI at install time from `cli/assets/` templates |

## What Gets Synced

```
.claude/skills/generalupdate-troubleshoot/
├── scripts/          →  cli/assets/scripts/
├── data/             →  cli/assets/data/

.claude/scripts/
├── generate.py       →  cli/assets/scripts/
└── generate/         →  cli/assets/scripts/generate/
```

## Why Not Symlinks?

Unlike ui-ux-pro-max which uses symlinks, this project uses **copy-based sync** because:

- **CLI deploys as an npm package** — npm doesn't follow symlinks when packaging
- **CI needs deterministic builds** — copies are explicit, not environment-dependent
- **Windows compatibility** — symlinks require admin rights or Developer Mode on Windows
- **Auditability** — `cli/assets/` is a snapshot that can be independently verified

## Sync Before Release Workflow

```bash
# 1. Make changes in .claude/skills/*/
# 2. Sync to CLI assets
python3 .claude/scripts/_sync_all.py
# 3. Verify
diff -r .claude/skills/generalupdate-troubleshoot/data cli/assets/data
# 4. Build & release
cd cli && npm run build
```
