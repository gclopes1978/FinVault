"""
Local tester for PayrollParser.

Reads a real payroll PDF, runs it through PayrollParser, and writes the
exact payload that would be sent via REST to Service C (payroll import)
into a .txt file for inspection — without actually calling the service.

Usage:
    python test_local_payroll.py
"""

import json
import uuid
from datetime import datetime, timezone

import pdfplumber

from parsers.payroll import PayrollParser

# --- Configuration -----------------------------------------------------

pdf_path = r"C:\Projects\FinVault\Data\uploads\Dezembro_2021_SF.pdf"
output_path = r"C:\Projects\FinVault\Data\uploads\Dezembro_2021_SF_payload.txt"
raw_txt_path = r"C:\Projects\FinVault\Data\uploads\Dezembro_2021_SF_raw.txt"
# --- Run parser ----------------------------------------------------------

with pdfplumber.open(pdf_path) as pdf:
    items = PayrollParser.parse(pdf)
# 1. Save raw text for layout analytics
full_text = ""
for page in pdf.pages:
    full_text += page.extract_text() or ""
with open(raw_txt_path, "w", encoding="utf-8") as raw_file:
    raw_file.write(full_text)
# --- Build the same envelope the dispatcher sends to Service C ----------

payload = {
    "sourceDocumentId": str(uuid.uuid4()),
    "data": items,
}

# --- Write result to a txt file for inspection ---------------------------

with open(output_path, "w", encoding="utf-8") as f:
    f.write(f"# Generated at: {datetime.now(timezone.utc).isoformat()}\n")
    f.write(f"# Source PDF: {pdf_path}\n")
    f.write(f"# Items found: {len(items)}\n\n")
    json.dump(payload, f, ensure_ascii=False, indent=2)

print(f"Parsed {len(items)} item(s).")
print(f"Payload written to: {output_path}")