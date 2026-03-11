import asyncio
from io import BytesIO

import pytest
from docx import Document
from fastapi.testclient import TestClient

import main
from main import GeminiAPIError, GeminiSummarizer, app, get_summarizer


class StubSummarizer:
    model = "stub-gemini"

    async def summarize_document(
        self,
        document_text: str,
        document_title: str | None = None,
        max_summary_words: int = 200,
    ) -> str:
        return (
            f"Summary for {document_title or 'Untitled legal document'}: "
            f"{document_text[:80]} (limit {max_summary_words})"
        )


class ErrorSummarizer:
    model = "stub-gemini"

    async def summarize_document(
        self,
        document_text: str,
        document_title: str | None = None,
        max_summary_words: int = 200,
    ) -> str:
        raise GeminiAPIError(429, "Gemini API rate limit reached. Please try again shortly.")


@pytest.fixture
def client():
    app.dependency_overrides[get_summarizer] = lambda: StubSummarizer()
    test_client = TestClient(app)
    try:
        yield test_client
    finally:
        test_client.close()
        app.dependency_overrides.clear()


def make_docx_bytes(*paragraphs: str) -> bytes:
    document = Document()
    for paragraph in paragraphs:
        document.add_paragraph(paragraph)

    buffer = BytesIO()
    document.save(buffer)
    return buffer.getvalue()


def test_root_endpoint(client: TestClient) -> None:
    response = client.get("/")

    assert response.status_code == 200
    assert response.json() == {"message": "Legal document summarizer API is running."}


def test_summarize_endpoint_returns_summary_for_json_text(client: TestClient) -> None:
    payload = {
        "document_title": "Lease Agreement",
        "document_text": "Tenant must pay rent by the first day of each month and keep the property clean.",
        "max_summary_words": 90,
    }

    response = client.post("/summarize", json=payload)

    assert response.status_code == 200
    body = response.json()
    assert body["model"] == "stub-gemini"
    assert "Lease Agreement" in body["summary"]
    assert "Tenant must pay rent" in body["summary"]


def test_summarize_endpoint_accepts_text_file_upload(client: TestClient) -> None:
    response = client.post(
        "/summarize",
        data={"document_title": "Service Agreement", "max_summary_words": "120"},
        files={
            "file": (
                "agreement.txt",
                b"Alpha Corp must pay Beta LLC within 15 days of each invoice.",
                "text/plain",
            )
        },
    )

    assert response.status_code == 200
    body = response.json()
    assert body["model"] == "stub-gemini"
    assert "Service Agreement" in body["summary"]
    assert "Alpha Corp must pay Beta LLC" in body["summary"]


def test_summarize_endpoint_accepts_docx_upload(client: TestClient) -> None:
    docx_bytes = make_docx_bytes(
        "This Employment Agreement begins on March 1, 2026.",
        "The employee must maintain confidentiality and receives a salary of $80,000.",
    )

    response = client.post(
        "/summarize",
        data={"max_summary_words": "150"},
        files={
            "file": (
                "employment-agreement.docx",
                docx_bytes,
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            )
        },
    )

    assert response.status_code == 200
    body = response.json()
    assert body["model"] == "stub-gemini"
    assert "employment-agreement" in body["summary"]
    assert "Employment Agreement begins on March 1, 2026" in body["summary"]


def test_summarize_endpoint_accepts_pdf_upload_when_text_is_extracted(client: TestClient, monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.setattr(main, "extract_text_from_pdf_bytes", lambda _bytes: "PDF contract text with payment and termination clauses.")

    response = client.post(
        "/summarize",
        data={"document_title": "PDF Contract", "max_summary_words": "110"},
        files={"file": ("contract.pdf", b"%PDF-1.4 sample", "application/pdf")},
    )

    assert response.status_code == 200
    body = response.json()
    assert body["model"] == "stub-gemini"
    assert "PDF Contract" in body["summary"]
    assert "payment and termination clauses" in body["summary"]


def test_summarize_endpoint_rejects_blank_document_text(client: TestClient) -> None:
    response = client.post(
        "/summarize",
        json={"document_text": "   ", "max_summary_words": 120},
    )

    assert response.status_code == 400
    assert response.json() == {"detail": "document_text must not be empty."}


def test_summarize_endpoint_rejects_unsupported_upload_type(client: TestClient) -> None:
    response = client.post(
        "/summarize",
        files={"file": ("archive.zip", b"fake-zip", "application/zip")},
        data={"max_summary_words": "120"},
    )

    assert response.status_code == 415
    assert "Unsupported file type" in response.json()["detail"]


def test_summarize_endpoint_rejects_empty_extracted_upload(client: TestClient) -> None:
    response = client.post(
        "/summarize",
        files={"file": ("blank.txt", b"   \n\n", "text/plain")},
        data={"max_summary_words": "120"},
    )

    assert response.status_code == 400
    assert response.json() == {"detail": "No readable text could be extracted from the uploaded document."}


def test_summarize_endpoint_returns_upstream_errors_for_uploads() -> None:
    app.dependency_overrides[get_summarizer] = lambda: ErrorSummarizer()
    with TestClient(app) as client:
        response = client.post(
            "/summarize",
            files={"file": ("settlement.txt", b"Confidential settlement agreement text.", "text/plain")},
            data={"max_summary_words": "120"},
        )
    app.dependency_overrides.clear()

    assert response.status_code == 429
    assert response.json() == {"detail": "Gemini API rate limit reached. Please try again shortly."}


def test_extract_text_from_docx_bytes_reads_paragraphs() -> None:
    docx_bytes = make_docx_bytes("First clause.", "Second clause.")

    assert main.extract_text_from_docx_bytes(docx_bytes) == "First clause.\nSecond clause."


def test_extract_text_from_rtf_bytes_removes_basic_rtf_markup() -> None:
    rtf_bytes = br"{\rtf1\ansi\b Confidentiality clause.\par Payment within 30 days.}"

    assert main.extract_text_from_rtf_bytes(rtf_bytes) == "Confidentiality clause.\nPayment within 30 days."


def test_extract_summary_reads_gemini_candidates() -> None:
    payload = {
        "candidates": [
            {
                "content": {
                    "parts": [
                        {"text": "First paragraph."},
                        {"text": "Second paragraph."},
                    ]
                }
            }
        ]
    }

    assert GeminiSummarizer._extract_summary(payload) == "First paragraph.\nSecond paragraph."


def test_summarize_document_requires_api_key() -> None:
    summarizer = GeminiSummarizer(api_key="")

    with pytest.raises(GeminiAPIError) as exc:
        asyncio.run(summarizer.summarize_document("Non-disclosure agreement text."))

    assert exc.value.status_code == 500
    assert exc.value.detail == "GEMINI_API_KEY is not configured."

