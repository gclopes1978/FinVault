import pdfplumber
import os
from parsers.pension_insurance import PensionInsuranceParser
from datetime import datetime, timezone


def run_test():
    file_path = r"C:\Projects\FinVault\Data\uploads\LUSITANIA_PPR.pdf" # Ajusta o nome do ficheiro
    output_path = r"C:\Projects\FinVault\Data\uploads\LUSITANIA_PPR.txt"
    print("=" * 60)
    print(" FINVAULT - INTEGRATION TEST: INSURANCE PENSION PARSER ")
    print("=" * 60)

    if not os.path.exists(file_path):
        print(f"[ERROR] File not found: {file_path}")
        return

    try:
        with pdfplumber.open(file_path) as pdf:
            result = PensionInsuranceParser.parse(pdf)
            
        print("[SUCCESS] Document parsed successfully!")
        print("\n--- TARGET JSON PAYLOAD ---")
        import json
        print(json.dumps(result, indent=4, ensure_ascii=False))
        print("---------------------------")
        
        with open(output_path, "w", encoding="utf-8") as f:
            f.write(f"# Generated at: {datetime.now(timezone.utc).isoformat()}\n")
            f.write(f"# Source PDF: {file_path}\n")
            f.write(f"# Items found: {len(result)}\n\n")
            json.dump(result, f, ensure_ascii=False, indent=2)
    except Exception as e:
        print(f"[ERROR] Parsing failed: {e}")

if __name__ == "__main__":
    run_test()