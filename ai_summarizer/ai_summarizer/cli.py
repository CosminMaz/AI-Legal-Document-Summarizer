from __future__ import annotations

import argparse
import json
from pathlib import Path

from .analyzer import LegalDocumentAnalyzer
from .readers import read_text_from_file


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        prog="legal-ai",
        description="Analyze legal documents: summarization, clause extraction, and risk checks.",
    )
    parser.add_argument("input_file", help="Path to input file (.txt, .md, .pdf, .docx, .rtf)")
    parser.add_argument(
        "--output",
        "-o",
        help="Optional path to save JSON output.",
        default=None,
    )
    parser.add_argument(
        "--pretty",
        action="store_true",
        help="Print formatted JSON.",
    )
    return parser


def main() -> None:
    parser = build_parser()
    args = parser.parse_args()

    input_path = Path(args.input_file)
    if not input_path.exists():
        raise SystemExit(f"Input file not found: {input_path}")

    raw_text = read_text_from_file(input_path)
    analyzer = LegalDocumentAnalyzer()
    result = analyzer.analyze(raw_text)

    dump_kwargs = {"ensure_ascii": False}
    if args.pretty:
        dump_kwargs["indent"] = 2

    output_json = json.dumps(result, **dump_kwargs)

    if args.output:
        Path(args.output).write_text(output_json, encoding="utf-8")
        print(f"Analysis written to: {args.output}")
    else:
        print(output_json)


if __name__ == "__main__":
    main()
