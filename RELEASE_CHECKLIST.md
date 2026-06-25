# Release Checklist — GeneralUpdate Skill CodeGen

> Use this checklist before **every release** to ensure consistent quality.

## 🔢 Version & Manifest

- [ ] Version number is consistent across all files:
  - [ ] `SKILL.md` (line ~49)
  - [ ] `README.md`, `README.en.md`, `README.zh-Hans.md`
  - [ ] `skill.json`
  - [ ] `.claude-plugin/plugin.json`
  - [ ] `.claude-plugin/marketplace.json` (metadata + plugin)
  - [ ] `cli/package.json`
- [ ] `SKILL.md` version string uses correct spelling (`beta` not `bate`)
- [ ] Release date is updated in `README.en.md`

## 🧪 Validation

> Run `python3 .claude/scripts/generate.py --list` to confirm all 336 combinations resolve.

- [ ] CI — **Python search engine tests**
  ```bash
  python3 -m pytest .claude/skills/generalupdate-troubleshoot/scripts/tests/ -v
  ```
- [ ] CI — **Python code generator smoke tests** (OSS+WPF, Silent+Console, Differential+Avalonia)
  ```bash
  # OSS + WPF + Bowl
  python3 .claude/scripts/generate.py --strategy oss --framework wpf-layui --bowl \
    --project-name TestApp --version 1.0.0.0 -o ${TMPDIR:-/tmp}/verify-oss

  # Silent + Console
  python3 .claude/scripts/generate.py --strategy silent --framework console \
    --project-name MyService --version 2.0.0.0 -o ${TMPDIR:-/tmp}/verify-silent

  # Differential + Avalonia
  python3 .claude/scripts/generate.py --strategy differential --framework avalonia-semiursa \
    --project-name CrossApp --version 3.1.0.0 -o ${TMPDIR:-/tmp}/verify-diff

  # Verify output is valid
  for f in $(find ${TMPDIR:-/tmp}/verify-oss ${TMPDIR:-/tmp}/verify-silent ${TMPDIR:-/tmp}/verify-diff -name "*.cs" -o -name "*.json"); do
    [ -s "$f" ] || { echo "❌ Empty: $f"; exit 1; }
    file "$f" | grep -q "UTF-8\|ASCII\|text" || { echo "❌ Non-text: $f"; exit 1; }
  done
  ```
- [ ] CI — **.NET template build verification** (MinimalIntegration + FullIntegration)
  - Requires Windows + .NET SDK 10.0.x
  - See `.github/workflows/ci.yml` → `dotnet-verify-templates` job
- [ ] CI — **Complete scaffold build** (ClientApp + UpgradeApp)
  - See `.github/workflows/ci.yml` → `dotnet-verify-scaffold` job
- [ ] CI — **CLI TypeScript compilation**
  ```bash
  cd cli
  npm ci --ignore-scripts
  npx tsc --noEmit
  ```

## 🔄 Sync

- [ ] Run asset sync (source → CLI bundle):
  ```bash
  python3 .claude/scripts/_sync_all.py --apply
  ```
- [ ] Verify sync is complete:
  ```bash
  python3 .claude/scripts/_sync_all.py --verify
  ```
- [ ] Confirm all changed files are committed:
  - `git status` shows no untracked/modified files except the `dist/` build output
  - The `.gitignore` includes `cli/dist/` (if tracking, add `cli/node_modules/` too)

## 📦 CLI Package

- [ ] CLI dependencies installed (`cd cli && npm ci --ignore-scripts`)
- [ ] TypeScript compiled successfully (`cd cli && npm run build`)
- [ ] `cli/dist/` exists and contains `.js` output
- [ ] Confirm CLI runs locally:
  ```bash
  node cli/dist/index.js --help
  ```
- [ ] npm publish dry-run:
  ```bash
  cd cli && npm publish --dry-run 2>&1
  ```
  - Verify `package.json` files list includes `dist/`
  - Verify no unintended files (node_modules, .ts) are included

## 🚀 GitHub Release

- [ ] All changes pushed to remote
- [ ] CI passes on `main` branch (or target release branch)
- [ ] Release tag created matching package version (e.g. `v0.0.2-beta.1`)
- [ ] GitHub Release created (via `release.yml` workflow or manually):
  - Release title: `v{version}`
  - Changelog: `feat:` / `fix:` / `docs:` / `chore:` sections from commit log
  - Artifacts: `cli/dist/`, `cli/package.json`
- [ ] (Optional) npm publish:
  ```bash
  cd cli && npm publish
  ```

## ✅ Post-Release

- [ ] Verify npm package is installable:
  ```bash
  npx gskill-cli --help
  ```
- [ ] Verify marketplace listing updates (if auto-ingested)
- [ ] Update Gitee mirror if applicable
- [ ] Announce release in relevant channels

---

## 📊 Pre-Release Health Check

| Check | Command | Expected |
|-------|---------|----------|
| Search engine | `python3 search.py "method not found" -n 3 --json` | ≥1 match |
| Strategy lookup | `python3 search.py "OSS" --domain strategy -n 3 --json` | ≥1 match |
| Code gen combos | `python3 generate.py --list` | 336 combinations |
| No old API refs | `grep -r "Configinfo\|Common\.Shared" .claude/scripts/generate/templates/` | 0 |
| No `TODO`/`FIXME` | `grep -r "TODO\|FIXME" .claude/skills/ --include="*.md" --include="*.cs"` | 0 |