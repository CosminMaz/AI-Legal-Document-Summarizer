# AI Legal Document Summarizer

A FastAPI service that accepts legal document text or uploaded files, sends the extracted content to the Gemini API, and returns a plain-language summary.

## What it does

- Exposes `POST /summarize`
- Accepts either raw JSON text or multipart file uploads
- Supports `.txt`, `.md`, `.pdf`, `.docx`, and `.rtf`
- Validates document size and request payloads
- Calls Gemini with a legal-document summary prompt
- Returns the generated summary and model name
- Surfaces upload and Gemini errors with API-friendly responses

## API contract

### Option 1: JSON text input

`POST /summarize`

```json
{
  "document_title": "Service Agreement",
  "document_text": "...legal document text...",
  "max_summary_words": 150
}
```

### Option 2: File upload input

Send `multipart/form-data` with:

- `file`: the legal document
- `document_title`: optional custom title
- `max_summary_words`: optional integer between `50` and `1000`

Supported upload formats:

- PDF: `.pdf`
- Word: `.docx`
- Text: `.txt`
- Markdown: `.md`
- Rich text: `.rtf`

> Note: legacy `.doc` files are not included in this version. Converting them to `.docx` is the easiest path.

## Response format

```json
{
  "summary": "Plain-language summary here",
  "model": "gemini-2.5-flash-lite"
}
```

## Setup

1. Create or activate a Python virtual environment.
2. Install dependencies:

```bash
pip install -r requirements.txt
```

3. Set your Gemini API key:

```bash
export GEMINI_API_KEY="your_api_key_here"
```

4. Start the API:

```bash
uvicorn main:app --reload
```

The API will be available at `http://127.0.0.1:8000`.
Swagger UI will be available at `http://127.0.0.1:8000/docs`.

## Quick test

### JSON request

```bash
curl -X POST http://127.0.0.1:8000/summarize \
  -H "Content-Type: application/json" \
  -d '{
    "document_title": "Employment Agreement",
    "document_text": "Employer hires Employee for a one-year term beginning March 1, 2026. Employee will receive a salary of $80,000 and must comply with confidentiality and non-compete clauses.",
    "max_summary_words": 120
  }'
```

### File upload request

```bash
curl -X POST http://127.0.0.1:8000/summarize \
  -F "file=@./sample-contract.docx" \
  -F "document_title=Sample Contract" \
  -F "max_summary_words=120"
```

## Run tests

```bash
pytest
```

## Notes

- The Gemini API key is read from the `GEMINI_API_KEY` environment variable.
- Uploaded files are limited to 10 MB.
- If Gemini rate-limits or is unavailable, the API returns a clear error response.
- If an uploaded file has no readable text, the API returns a validation error instead of sending an empty prompt upstream.
