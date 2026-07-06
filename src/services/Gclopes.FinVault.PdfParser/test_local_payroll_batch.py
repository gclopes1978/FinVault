"""
Batch local tester for PayrollParser.

Processes every PDF file found in a folder. For each PDF:
  1. Prints the raw extracted text (for visual inspection / debugging).
  2. Runs PayrollParser against it.
  3. Writes the exact payload that would be sent via REST to Service C
     (payroll import) into a matching .txt file, without calling the
     service.

Usage:
    python test_local_payroll_batch.py
"""

import json
import uuid
from datetime import datetime, timezone
from pathlib import Path

import pdfplumber

from parsers.payroll import PayrollParser

# --- Configuration -----------------------------------------------------

input_folder = Path(r"C:\Projects\FinVault\Data\uploads\Recibos_Vencimento")
output_folder = input_folder  # payloads are written next to the source PDFs

# --- Process every PDF in the folder ------------------------------------

pdf_files = sorted(input_folder.glob("*.pdf"))

if not pdf_files:
    print(f"No PDF files found in: {input_folder}")

for pdf_path in pdf_files:
    print("=" * 80)
    print(f"FILE: {pdf_path.name}")
    print("=" * 80)

    with pdfplumber.open(pdf_path) as pdf:
        # Print raw text of every page, for visual inspection
        for page_number, page in enumerate(pdf.pages, start=1):
            raw_text = page.extract_text() or ""
            print(f"--- Page {page_number} raw text ---")
            print(raw_text)
            print()

        items = PayrollParser.parse(pdf)

    payload = {
        "sourceDocumentId": str(uuid.uuid4()),
        "data": items,
    }

    output_path = output_folder / f"{pdf_path.stem}_payload.txt"
    with open(output_path, "w", encoding="utf-8") as f:
        f.write(f"# Generated at: {datetime.now(timezone.utc).isoformat()}\n")
        f.write(f"# Source PDF: {pdf_path}\n")
        f.write(f"# Items found: {len(items)}\n\n")
        json.dump(payload, f, ensure_ascii=False, indent=2)

    print(f"Parsed {len(items)} item(s). Payload written to: {output_path}")
    print()

print("=" * 80)
print(f"Done. Processed {len(pdf_files)} file(s).")