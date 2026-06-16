#!/usr/bin/env python3
"""Unit tests for GeneralUpdate BM25 search engine."""
import sys
import os

# Set DATA_DIR before importing core — test runs from scripts/ but data is at ../data
os.environ["GENERALUPDATE_DATA_DIR"] = os.path.normpath(
    os.path.join(os.path.dirname(__file__), '..', '..', 'data')
)

sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..'))

import json
from core import search, CSV_CONFIG, DATA_DIR


def test_csv_files_exist():
    """All configured CSV data files must exist."""
    for domain, config in CSV_CONFIG.items():
        filepath = DATA_DIR / config["file"]
        assert filepath.exists(), f"Missing CSV: {filepath}"
        print(f"  ✓ {config['file']} exists")


def test_issues_csv_has_all_severities():
    """Issues CSV must contain entries for all severity levels: C, H, M, L."""
    # Use a broad query that matches many issues
    result = search("update error fail crash bug", "issue", 100)
    assert "error" not in result, f"Search error: {result.get('error')}"
    severities = {row["severity"] for row in result["results"]}
    for sev in ["C", "H", "M", "L"]:
        assert sev in severities, f"Missing severity level: {sev} (found: {severities})"
    print(f"  ✓ All severities (C/H/M/L) present: found {len(result['results'])} matching entries")


def test_issues_csv_required_columns():
    """Verify required columns exist by reading raw CSV."""
    import csv
    filepath = DATA_DIR / CSV_CONFIG["issue"]["file"]
    with open(filepath, 'r', encoding='utf-8') as f:
        rows = list(csv.DictReader(f))
    required = ["id", "severity", "symptom_en", "symptom_zh", "cause", "solution"]
    for row in rows:
        for col in required:
            assert col in row and row[col], f"Missing column '{col}' in issue {row.get('id')}"
    print(f"  ✓ All required columns present in {len(rows)} issues")


def test_strategies_csv_has_all_6():
    """Must have exactly 6 strategy entries."""
    import csv
    filepath = DATA_DIR / CSV_CONFIG["strategy"]["file"]
    with open(filepath, 'r', encoding='utf-8') as f:
        rows = list(csv.DictReader(f))
    assert len(rows) == 6, f"Expected 6 strategies, got {len(rows)}"
    ids = {r["id"] for r in rows}
    for i in range(1, 7):
        sid = f"S{i:02d}"
        assert sid in ids, f"Missing strategy: {sid}"
    print(f"  ✓ All 6 strategies present")


def test_chinese_search_upgrade_not_start():
    """"升级后启动不了" should match C1 (top result)."""
    result = search("升级后启动不了", "issue", 3)
    assert "error" not in result
    assert len(result["results"]) > 0
    top = result["results"][0]
    assert top["id"] == "C1", f"Expected C1, got {top['id']}"
    assert top["_score"] > 0, f"Zero score for C1"
    print(f"  ✓ '升级后启动不了' → C1 (score={top['_score']})")


def test_chinese_search_garbled():
    """"中文乱码" should match H1 (top result)."""
    result = search("中文乱码", "issue", 3)
    assert "error" not in result
    assert len(result["results"]) > 0
    top = result["results"][0]
    assert top["id"] == "H1", f"Expected H1, got {top['id']}"
    print(f"  ✓ '中文乱码' → H1 (score={top['_score']})")


def test_english_search_method_not_found():
    """"method not found" should match C2."""
    result = search("method not found", "issue", 3)
    assert "error" not in result
    assert len(result["results"]) > 0
    top = result["results"][0]
    assert top["id"] == "C2", f"Expected C2, got {top['id']}"
    print(f"  ✓ 'method not found' → C2 (score={top['_score']})")


def test_english_search_zip_slip():
    """"zip slip path traversal" should match C6."""
    result = search("zip slip path traversal", "issue", 3)
    assert "error" not in result
    assert len(result["results"]) > 0
    top = result["results"][0]
    assert top["id"] == "C6", f"Expected C6, got {top['id']}"
    print(f"  ✓ 'zip slip path traversal' → C6 (score={top['_score']})")


def test_strategy_search_oss():
    """"OSS no backend" should match OSS strategy."""
    result = search("OSS no backend server", "strategy", 3)
    assert "error" not in result
    assert len(result["results"]) > 0
    top = result["results"][0]
    assert top["id"] == "S02", f"Expected S02 (OSS), got {top['id']}"
    print(f"  ✓ 'OSS no backend' → S02 (score={top['_score']})")


def test_strategy_search_signalr():
    """"pus" should match SignalR push strategy."""
    result = search("push real-time connection", "strategy", 3)
    assert "error" not in result
    assert len(result["results"]) > 0
    top = result["results"][0]
    assert top["id"] == "S06", f"Expected S06 (SignalR), got {top['id']}"
    print(f"  ✓ 'push real-time' → S06 (score={top['_score']})")


def test_search_json_output():
    """Search output should have correct JSON structure."""
    result = search("升级后启动不了", "issue", 3)
    assert "error" not in result
    assert result["domain"] == "issue"
    assert result["query"] == "升级后启动不了"
    assert result["file"] == "issues.csv"
    assert result["count"] >= 1
    assert len(result["results"]) >= 1
    # Each result should have _score
    for row in result["results"]:
        assert "_score" in row
    print(f"  ✓ JSON structure correct")


def test_search_invalid_domain():
    """Invalid domain should return error."""
    result = search("test", "invalid_domain")
    assert "error" in result
    print(f"  ✓ Invalid domain returns error")


def test_search_no_results():
    """Search with gibberish should return 0 results."""
    result = search("zzzzzzzxxxxxxyyyyyyy", "issue", 3)
    assert "error" not in result
    assert result.get("count", 0) == 0
    print(f"  ✓ Gibberish search returns 0 results")


def test_bm25_scoring_differentiation():
    """Different queries should produce different top results."""
    r1 = search("garbled encoding chinese", "issue", 1)
    r2 = search("zip slip path traversal", "issue", 1)
    top1_id = r1["results"][0]["id"]
    top2_id = r2["results"][0]["id"]
    assert top1_id != top2_id, f"Two different queries returned same top result: {top1_id}"
    print(f"  ✓ BM25 differentiates queries: {top1_id} vs {top2_id}")


def test_all_strategies_searchable():
    """Each of the 6 strategies should be findable by keyword."""
    queries = ["standard client-server", "oss", "silent background", "differential delta", "cross version", "signalr push"]
    for i, q in enumerate(queries):
        r = search(q, "strategy", 1)
        assert r["count"] >= 1, f"Strategy {i+1} not found by query: {q}"
    print(f"  ✓ All 6 strategies searchable")


if __name__ == "__main__":
    print(f"\n🧪 GeneralUpdate Search Engine Tests\n")
    tests = [fn for fn in dir() if fn.startswith("test_")]
    passed = 0
    failed = 0
    for name in tests:
        try:
            globals()[name]()
            print(f"  PASS  {name}")
            passed += 1
        except Exception as e:
            print(f"  FAIL  {name}: {e}")
            failed += 1
    print(f"\n{'='*40}")
    print(f"  Total: {passed + failed} | ✅ {passed} | ❌ {failed}")
    if failed:
        sys.exit(1)
