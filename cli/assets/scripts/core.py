#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
GeneralUpdate Search Core - BM25 search engine for troubleshooting issues and strategies.
"""
import csv
import re
import os
from pathlib import Path
from math import log
from collections import defaultdict

# Allow DATA_DIR override via environment variable (for testing)
_default_data_dir = Path(__file__).resolve().parent.parent / "data"
DATA_DIR = Path(os.environ.get("GENERALUPDATE_DATA_DIR", str(_default_data_dir)))
MAX_RESULTS = 3

CSV_CONFIG = {
    "issue": {
        "file": "issues.csv",
        "search_cols": ["symptom_en", "symptom_zh", "cause", "keywords"],
        "output_cols": ["id", "severity", "symptom_en", "symptom_zh", "cause", "solution", "code_ref", "workaround"],
    },
    "strategy": {
        "file": "strategies.csv",
        "search_cols": ["name", "description", "best_for", "keywords"],
        "output_cols": ["id", "name", "description", "server_required", "best_for", "pros", "cons", "keywords"],
    },
}

class BM25:
    """BM25 ranking algorithm for text search"""
    def __init__(self, k1=1.5, b=0.75):
        self.k1 = k1
        self.b = b
        self.corpus = []
        self.doc_lengths = []
        self.avgdl = 0
        self.idf = {}
        self.doc_freqs = defaultdict(int)
        self.N = 0

    def tokenize(self, text):
        """Tokenize text: split CJK into character unigrams + bigrams, keep English words."""
        text = str(text).lower()
        result = []

        # Split into CJK and non-CJK segments
        segments = re.split(r'([一-鿿㐀-䶿]+)', text)
        for seg in segments:
            if not seg:
                continue
            if re.match(r'^[一-鿿㐀-䶿]+$', seg):
                # CJK: char unigrams + bigrams
                chars = list(seg)
                # Unigrams
                result.extend(chars)
                # Bigrams
                for i in range(len(chars) - 1):
                    result.append(chars[i] + chars[i+1])
            else:
                # English/alphanumeric: clean punctuation, keep words
                cleaned = re.sub(r'[^\w\s]', ' ', seg)
                for w in cleaned.split():
                    w = w.strip()
                    if len(w) > 1 and not w.isdigit():
                        result.append(w)

        return result

    def fit(self, corpus):
        self.corpus = corpus
        self.doc_lengths = [len(doc) for doc in corpus]
        self.avgdl = sum(self.doc_lengths) / len(corpus) if corpus else 0
        self.N = len(corpus)

        for doc in corpus:
            seen = set()
            for term in doc:
                if term not in seen:
                    self.doc_freqs[term] += 1
                    seen.add(term)

        for term, df in self.doc_freqs.items():
            self.idf[term] = log((self.N - df + 0.5) / (df + 0.5) + 1.0)

    def score(self, query_terms, doc_idx):
        doc = self.corpus[doc_idx]
        doc_len = self.doc_lengths[doc_idx]
        score = 0.0
        for term in query_terms:
            if term in self.idf:
                tf = doc.count(term)
                score += self.idf[term] * (tf * (self.k1 + 1)) / (tf + self.k1 * (1 - self.b + self.b * doc_len / self.avgdl))
        return score

    def search(self, query, top_n=MAX_RESULTS):
        query_terms = self.tokenize(query)
        if not query_terms or not self.corpus:
            return []
        scores = [(i, self.score(query_terms, i)) for i in range(len(self.corpus))]
        scores.sort(key=lambda x: x[1], reverse=True)
        return [(idx, score) for idx, score in scores[:top_n] if score > 0]


def load_csv(filepath):
    """Load CSV file and return list of dicts."""
    full_path = DATA_DIR / filepath
    if not full_path.exists():
        return []
    with open(full_path, 'r', encoding='utf-8') as f:
        return list(csv.DictReader(f))


def search(query, domain="issue", max_results=MAX_RESULTS):
    """Search across issues or strategies domain."""
    if domain not in CSV_CONFIG:
        return {"error": f"Unknown domain: {domain}. Available: {list(CSV_CONFIG.keys())}"}

    config = CSV_CONFIG[domain]
    data = load_csv(config["file"])

    if not data:
        return {"error": f"No data found for domain: {domain}"}

    # Build corpus
    corpus = []
    for row in data:
        doc_text = " ".join(str(row.get(col, "")) for col in config["search_cols"])
        corpus.append(BM25().tokenize(doc_text))

    bm25 = BM25()
    bm25.fit(corpus)
    results = bm25.search(query, max_results)

    output = []
    for idx, score in results:
        row = data[idx]
        row["_score"] = round(score, 2)
        output.append(row)

    return {
        "domain": domain,
        "query": query,
        "file": config["file"],
        "count": len(output),
        "results": output,
    }
