import pdfplumber
import re
from decimal import Decimal

class PensionInsuranceParser:
    """Parses Lusitânia Vida insurance movements into a structured list."""
    
    @staticmethod
    def parse(pdf: pdfplumber.PDF) -> list:
        full_text = ""
        for page in pdf.pages:
            full_text += (page.extract_text() or "") + "\n"
        
        # Regex to capture: [Movement Description] [Receipt/Optional] [Date] [Debit] [Credit] [Balance]
        # We look for lines that contain a date (DD/MM/YYYY)
        pattern = r"(.+?)\s+(\d{9}\s+)?(\d{2}/\d{2}/\d{4})\s+€([\d\.,]+)\s+€([\d\.,]+)\s+€([\d\.,]+)"
        
        movements = []
        
        for match in re.finditer(pattern, full_text):
            desc, receipt, date_str, debit, credit, balance = match.groups()
            
            # Helper to clean numbers: 99.999,99 -> 99999.99
            def to_float(val_str) -> float:
                clean = val_str.replace(".", "").replace(",", ".")
                return float(clean)

            # Reformat Date to YYYY-MM-DD
            day, month, year = date_str.split('/')
            formatted_date = f"{year}-{month}-{day}"
            
            movements.append({
                "movement": desc.strip(),
                "receipt": receipt.strip() if receipt else None,
                "date": formatted_date,
                "debit": float(to_float(debit)),   # Garante que é float
                "credit": float(to_float(credit)), # Garante que é float
                "balance": float(to_float(balance))
            })
            
        return movements