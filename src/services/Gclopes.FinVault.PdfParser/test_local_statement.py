import os
import json
import re
import pdfplumber
from parsers.bank_statement import BankStatementParser

def generate_statement_test_output():
    pdf_path = r"C:\Projects\FinVault\Data\uploads\202606.pdf"
    txt_path = r"C:\Projects\FinVault\Data\uploads\202606.txt"
    raw_txt_path = r"C:\Projects\FinVault\Data\uploads\202606_raw.txt"
    
    print(f"Reading PDF from: {pdf_path}")
    
    if not os.path.exists(pdf_path):
        print(f"[Error] The file {pdf_path} does not exist.")
        return

    try:
        with pdfplumber.open(pdf_path) as pdf:
            # 1. Save raw text for layout analytics
            full_text = ""
            for page in pdf.pages:
                full_text += page.extract_text() or ""
            with open(raw_txt_path, "w", encoding="utf-8") as raw_file:
                raw_file.write(full_text)

            # 2. Parse statement data
            payload = BankStatementParser.parse(pdf)
            
            # Generate the standard JSON layout string
            json_output = json.dumps(payload, indent=4, ensure_ascii=False)
            
            # REGEX FIX: Using \g<1> to clearly separate group ID from the literal '0'
            json_output = re.sub(r'(:\s*-?\d+\.\d)(?!\d)', r'\g<1>0', json_output)
            
            with open(txt_path, "w", encoding="utf-8") as txt_file:
                txt_file.write(json_output)
                
            print(f"[Success] Parser payload saved to: {txt_path}")
            print(json_output)
            
    except Exception as e:
        print(f"[Exception Error] Failed to process PDF: {str(e)}")

if __name__ == "__main__":
    generate_statement_test_output()