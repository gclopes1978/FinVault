import os
import sys
import json
import pdfplumber
# Assuming your PensionFundParser class is inside parsers/pension_fund.py
from parsers.pension_fund import PensionFundParser
from datetime import datetime, timezone

def test_real_file():
    print("=" * 60)
    print(" FINVAULT - INTEGRATION TEST: PENSION FUND PARSER ")
    print("=" * 60)

    # Absolute path to your local PDF statement
    pdf_path = r"C:\Projects\FinVault\Data\uploads\AR_PPR.pdf"
    output_path = r"C:\Projects\FinVault\Data\uploads\AR_PPR.txt"
    raw_txt_path = r"C:\Projects\FinVault\Data\uploads\AR_PPR_raw.txt"
    
    print(f"\n[INFO] Target file path: {pdf_path}")

    # 1. Check if the directory and file actually exist before attempting to read
    if not os.path.exists(pdf_path):
        print(f"\n[CRITICAL ERROR] File not found at the specified location.")
        print(f"Please verify if the file exists or if the path is correct.")
        sys.exit(1)

    print("[INFO] File located successfully. Initializing pdfplumber...")

    # 2. Open the real PDF file and execute the parser
    try:
        with pdfplumber.open(pdf_path) as pdf:
            full_text = ""
            for page in pdf.pages:
                full_text += page.extract_text()
        with open(raw_txt_path, "w", encoding="utf-8") as raw_file:
                raw_file.write(full_text)
        json_payload = PensionFundParser.parse(pdf)
        print("\n[SUCCESS] Document parsed completely without validation errors!")
        print("\n--- TARGET JSON PAYLOAD FOR THE SERVICE ---")
        with open(output_path, "w", encoding="utf-8") as f:
            f.write(f"# Generated at: {datetime.now(timezone.utc).isoformat()}\n")
            f.write(f"# Source PDF: {pdf_path}\n")
            f.write(f"# Items found: {len(json_payload)}\n\n")
            json.dump(json_payload, f, ensure_ascii=False, indent=2)
            
        # Print the exact formatted JSON that would be sent to your FinVault frontend/service
        print(json.dumps(json_payload, indent=4, ensure_ascii=False))
        print("-" * 43)

    except ValueError as val_err:
        print(f"\n[VALIDATION FAILED] The parser rejected the file due to data inconsistency:")
        print(f" > {val_err}")
        sys.exit(1)
        
    except Exception as err:
        print(f"\n[CRITICAL ERROR] Failed to process the real PDF file:")
        print(f" > {err}")
        sys.exit(1)

if __name__ == "__main__":
    test_real_file()
    print("\n" + "=" * 60)