#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Sync all source artifacts from .claude/ to cli/assets/ before release.
This is the single entry point for ensuring CLI bundles are up-to-date.

Usage:
    python3 .claude/scripts/_sync_all.py          # Dry run (verbose)
    python3 .claude/scripts/_sync_all.py --apply   # Actually copy files
    python3 .claude/scripts/_sync_all.py --verify  # Only check for differences
"""
import argparse
import filecmp
import os
import shutil
import sys
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent.parent  # .claude/scripts/ -> repo root
CLAUDE_DIR = REPO_ROOT / ".claude"
CLI_ASSETS_DIR = REPO_ROOT / "cli" / "assets"
SKILL_DIR = CLAUDE_DIR / "skills"

# Sync mappings: (source_relative, dest_relative, description)
SYNC_MAP = [
    # All 7 skills (to cli/assets/skills/)
    ("skills", "skills", "All 7 skill directories"),
    # Troubleshoot scripts + data (to cli/assets/scripts/ + cli/assets/data/)
    ("skills/generalupdate-troubleshoot/scripts", "scripts", "BM25 search engine"),
    ("skills/generalupdate-troubleshoot/data", "data", "Issues + strategies CSV"),
    # Code generator
    ("scripts/generate.py", "scripts/generate.py", "Parameterized code generator"),
    ("scripts/generate", "scripts/generate", "Generator templates"),
]


def sync_file(src: Path, dst: Path, apply: bool, dry_run: bool) -> str:
    """Sync one file or directory. Returns status string."""
    if not src.exists():
        return f"⚠️  SOURCE MISSING: {src.relative_to(REPO_ROOT)}"

    if dst.exists():
        if src.is_file():
            up_to_date = filecmp.cmp(src, dst, shallow=False)
        else:
            up_to_date = _dirs_equal(src, dst)
        if up_to_date:
            return f"✓  UP TO DATE: {src.relative_to(REPO_ROOT)}"

    if dry_run or not apply:
        return f"→  NEEDS SYNC: {src.relative_to(REPO_ROOT)}  →  {dst.relative_to(REPO_ROOT)}"

    # Apply the sync
    dst.parent.mkdir(parents=True, exist_ok=True)
    if src.is_file():
        shutil.copy2(src, dst)
    else:
        if dst.exists():
            shutil.rmtree(dst)
        shutil.copytree(src, dst)
    return f"✅  SYNCED: {src.relative_to(REPO_ROOT)}"


def _dirs_equal(a: Path, b: Path) -> bool:
    """Check if all files in source exist and match in destination (b may have extras)."""
    if not b.exists():
        return False

    def _files(p: Path):
        return sorted(
            (f for f in p.rglob("*") if f.is_file() and "__pycache__" not in str(f)),
            key=lambda x: str(x).lower(),
        )

    afiles = _files(a)

    for fa in afiles:
        rel = fa.relative_to(a)
        fb = b / rel
        if not fb.exists():
            return False
        if not filecmp.cmp(fa, fb, shallow=False):
            return False

    return True


def main():
    parser = argparse.ArgumentParser(description="Sync .claude/ source to cli/assets/")
    parser.add_argument("--apply", action="store_true", help="Actually copy files (default: dry-run)")
    parser.add_argument("--verify", action="store_true", help="Only check, exit 1 if out of sync")
    args = parser.parse_args()

    dry_run = not args.apply
    if dry_run and not args.verify:
        print("═══ DRY RUN ═══  Use --apply to actually copy\n")

    statuses = []
    all_ok = True

    for src_rel, dst_rel, desc in SYNC_MAP:
        src = CLAUDE_DIR / src_rel
        dst = CLI_ASSETS_DIR / dst_rel
        status = sync_file(src, dst, args.apply, dry_run)
        statuses.append((desc, status))
        if status.startswith("⚠️"):
            all_ok = False

    print(f"\n═══ Summary ({'DRY RUN' if dry_run else 'APPLIED'}) ═══\n")
    for desc, status in statuses:
        print(f"  {status}")

    if args.verify:
        needs_sync = any(status.startswith("→") for _, status in statuses)
        if not all_ok or needs_sync:
            print("\n❌ Verify FAILED: sources are out of sync")
            sys.exit(1)
        print("\n✅ Verify PASSED: all sources are in sync")
        sys.exit(0)


if __name__ == "__main__":
    main()
