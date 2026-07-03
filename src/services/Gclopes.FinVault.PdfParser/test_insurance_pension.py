import pdfplumber
import os
from parsers.pension_insurance import PensionInsuranceParser


def run_test():
    file_path = r"C:\Projects\FinVault\Data\uploads\LUSITANIA_PPR.pdf" # Ajusta o nome do ficheiro
    
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
        
    except Exception as e:
        print(f"[ERROR] Parsing failed: {e}")

if __name__ == "__main__":
    run_test()