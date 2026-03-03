from __future__ import annotations

import re
import importlib
from collections import Counter
from dataclasses import dataclass
from typing import Any


@dataclass
class RiskRule:
    name: str
    severity: str
    pattern: str
    description: str


STOPWORDS = {
    "the",
    "and",
    "or",
    "to",
    "of",
    "in",
    "for",
    "a",
    "an",
    "is",
    "are",
    "be",
    "this",
    "that",
    "with",
    "by",
    "as",
    "on",
    "at",
    "from",
    "it",
    "its",
    "shall",
    "will",
    "may",
}

RISK_RULES = [
    RiskRule(
        name="Unlimited Liability",
        severity="high",
        pattern=r"\bunlimited liability\b|\bliability shall not be limited\b",
        description="Potentially uncapped liability exposure.",
    ),
    RiskRule(
        name="Auto-Renewal",
        severity="medium",
        pattern=r"\bautomatically renew\w*\b|\bauto[-\s]?renew\w*\b",
        description="Contract may renew without explicit re-approval.",
    ),
    RiskRule(
        name="Broad Indemnity",
        severity="high",
        pattern=r"\bindemnif\w+\b.{0,80}\bany and all\b",
        description="Broad indemnity language can create significant obligations.",
    ),
    RiskRule(
        name="One-sided Termination",
        severity="medium",
        pattern=r"\bmay terminate\b.{0,80}\bat any time\b",
        description="Termination rights may be one-sided or too broad.",
    ),
    RiskRule(
        name="No Data Protection",
        severity="high",
        pattern=r"\bpersonal data\b|\bconfidential information\b",
        description="Data-related clauses detected; verify privacy/security obligations are explicit.",
    ),
    RiskRule(
        name="Governing Law Missing",
        severity="low",
        pattern=r"\bgoverning law\b|\bjurisdiction\b",
        description="Governing law or jurisdiction should usually be clearly defined.",
    ),
]


class LegalDocumentAnalyzer:
    def __init__(
        self,
        model_name: str = "sshleifer/distilbart-cnn-12-6",
        max_chunk_words: int = 700,
    ) -> None:
        self.model_name = model_name
        self.max_chunk_words = max_chunk_words
        self._summarizer = None

    def analyze(self, text: str) -> dict[str, Any]:
        cleaned_text = self._normalize_whitespace(text)
        return {
            "summarization": self.summarize(cleaned_text),
            "clause_extraction": self.extract_clauses(cleaned_text),
            "risk_checks": self.risk_checks(cleaned_text),
        }

    def summarize(self, text: str) -> str:
        if len(text.split()) < 80:
            return text

        try:
            if self._summarizer is None:
                pipeline = importlib.import_module("transformers").pipeline

                self._summarizer = pipeline(
                    "summarization",
                    model=self.model_name,
                    tokenizer=self.model_name,
                )
            return self._ai_summary(text)
        except Exception:
            return self._extractive_summary(text)

    def extract_clauses(self, text: str) -> list[dict[str, str]]:
        clauses: list[dict[str, str]] = []
        sentence_chunks = [s.strip() for s in re.split(r"(?<=[.!?])\s+", text) if s.strip()]

        clause_patterns = {
            "Definitions": r"\bdefinitions?\b|\bmeans\b",
            "Payment Terms": r"\bpayment\b|\bfees?\b|\binvoice\b",
            "Confidentiality": r"\bconfidential\b|\bnon-disclosure\b|\bnda\b",
            "Liability": r"\bliabilit\w+\b|\blimitation of liability\b",
            "Indemnification": r"\bindemnif\w+\b",
            "Termination": r"\bterminat\w+\b|\bexpiration\b",
            "Intellectual Property": r"\bintellectual property\b|\bip rights\b|\bcopyright\b",
            "Dispute Resolution": r"\barbitration\b|\bdispute\b|\bmediation\b",
            "Governing Law": r"\bgoverning law\b|\bjurisdiction\b|\bvenue\b",
            "Data Protection": r"\bdata protection\b|\bprivacy\b|\bpersonal data\b|\bgdpr\b",
        }

        for clause_name, pattern in clause_patterns.items():
            matches = [s for s in sentence_chunks if re.search(pattern, s, flags=re.IGNORECASE)]
            if matches:
                clauses.append(
                    {
                        "clause": clause_name,
                        "evidence": " ".join(matches[:3]),
                    }
                )

        return clauses

    def risk_checks(self, text: str) -> list[dict[str, str]]:
        findings: list[dict[str, str]] = []

        for rule in RISK_RULES:
            matches = list(re.finditer(rule.pattern, text, flags=re.IGNORECASE | re.DOTALL))

            if rule.name == "Governing Law Missing" and not matches:
                findings.append(
                    {
                        "risk": rule.name,
                        "severity": rule.severity,
                        "description": "No explicit governing law/jurisdiction clause detected.",
                        "evidence": "Not found in text.",
                    }
                )
                continue

            if rule.name == "No Data Protection" and matches:
                has_security_wording = re.search(
                    r"\bencryption\b|\bsecurity\b|\btechnical and organizational measures\b",
                    text,
                    flags=re.IGNORECASE,
                )
                if has_security_wording:
                    continue

            if matches:
                evidence = self._extract_evidence_window(text, matches[0].start(), 180)
                findings.append(
                    {
                        "risk": rule.name,
                        "severity": rule.severity,
                        "description": rule.description,
                        "evidence": evidence,
                    }
                )

        return findings

    def _ai_summary(self, text: str) -> str:
        chunks = self._chunk_text(text, self.max_chunk_words)
        partials = []

        for chunk in chunks:
            result = self._summarizer(
                chunk,
                max_length=180,
                min_length=60,
                do_sample=False,
                truncation=True,
            )
            partials.append(result[0]["summary_text"].strip())

        if len(partials) == 1:
            return partials[0]

        merged = " ".join(partials)
        final = self._summarizer(
            merged,
            max_length=200,
            min_length=80,
            do_sample=False,
            truncation=True,
        )
        return final[0]["summary_text"].strip()

    def _extractive_summary(self, text: str, top_n: int = 5) -> str:
        sentences = [s.strip() for s in re.split(r"(?<=[.!?])\s+", text) if s.strip()]
        if len(sentences) <= top_n:
            return " ".join(sentences)

        words = re.findall(r"\b[a-zA-Z][a-zA-Z\-]{2,}\b", text.lower())
        freq = Counter(w for w in words if w not in STOPWORDS)

        scored = []
        for idx, sentence in enumerate(sentences):
            sent_words = re.findall(r"\b[a-zA-Z][a-zA-Z\-]{2,}\b", sentence.lower())
            score = sum(freq[w] for w in sent_words)
            scored.append((score, idx, sentence))

        best = sorted(scored, key=lambda x: x[0], reverse=True)[:top_n]
        best_in_order = sorted(best, key=lambda x: x[1])
        return " ".join(s for _, _, s in best_in_order)

    @staticmethod
    def _normalize_whitespace(text: str) -> str:
        text = text.replace("\u00a0", " ")
        text = re.sub(r"[ \t]+", " ", text)
        text = re.sub(r"\n{3,}", "\n\n", text)
        return text.strip()

    @staticmethod
    def _chunk_text(text: str, max_words: int) -> list[str]:
        words = text.split()
        if len(words) <= max_words:
            return [text]

        chunks = []
        for i in range(0, len(words), max_words):
            chunks.append(" ".join(words[i : i + max_words]))
        return chunks

    @staticmethod
    def _extract_evidence_window(text: str, pos: int, radius: int) -> str:
        start = max(pos - radius, 0)
        end = min(pos + radius, len(text))
        excerpt = text[start:end].strip()
        return re.sub(r"\s+", " ", excerpt)
