from __future__ import annotations

from pathlib import Path


def read_text_from_file(file_path: str | Path) -> str:
    path = Path(file_path)
    suffix = path.suffix.lower()

    if suffix in {".txt", ".md", ".markdown"}:
        return path.read_text(encoding="utf-8", errors="ignore")

    if suffix == ".pdf":
        from pypdf import PdfReader

        reader = PdfReader(str(path))
        return "\n".join(page.extract_text() or "" for page in reader.pages)

    if suffix == ".docx":
        from docx import Document

        doc = Document(str(path))
        return "\n".join(p.text for p in doc.paragraphs)

    if suffix == ".rtf":
        from striprtf.striprtf import rtf_to_text

        raw = path.read_text(encoding="utf-8", errors="ignore")
        return rtf_to_text(raw)

    raise ValueError(
        f"Unsupported file type: {suffix}. Supported: .txt, .md, .pdf, .docx, .rtf"
    )
