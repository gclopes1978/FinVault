import pdfplumber
import re
from decimal import Decimal, ROUND_HALF_UP

class PensionFundParser:
    """Parses AR Banco Invest Fund-based Pension Plan (PPR) statement."""
    
    @staticmethod
    def parse(pdf: pdfplumber.PDF) -> dict:
        full_text = ""
        for page in pdf.pages:
            full_text += (page.extract_text() or "") + "\n"
            
        # Regex explanation:
        # 1. Matches the date (YYYY-MM-DD)
        # 2. Skips the name (.*?)
        # 3. Captures the three numeric values (Quantity, Price, Valuation)
        # We use re.DOTALL to ensure the regex matches even if there are newlines
        pattern = r"(\d{4}-\d{2}-\d{2})\s+([\d\.]+,\d+)\s+([\d\.]+,\d+)\s+([\d\.]+,\d+)"
        
        # We search specifically for the pattern in the text
        match = re.search(pattern, full_text)
        
        if not match:
            return {"date": "Unknown", "quantity": 0.0, "price": 0.0, "valuation": 0.0}
            
        date_str, qty_str, price_str, val_str = match.groups()

        def to_decimal(value_str) -> Decimal:
            # Remove thousands separator (.), keep comma for decimal (,)
            clean = value_str.replace(".", "").replace(",", ".")
            return Decimal(clean)

        quantity_dec = to_decimal(qty_str)
        unitPrice_dec = to_decimal(price_str)
        valuation_dec = to_decimal(val_str)
        
        # Financial validation logic
        expected_valuation = (quantity_dec * unitPrice_dec).quantize(Decimal("0.01"), rounding=ROUND_HALF_UP)
        
        # We allow a small tolerance for rounding differences
        if quantity_dec > 0 and abs(expected_valuation - valuation_dec) > Decimal("0.05"):
            raise ValueError(
                f"Validation failed: Qty({quantity_dec}) * Price({unitPrice_dec}) "
                f"= {expected_valuation}, but Valuation in PDF is {valuation_dec}."
            )

        return {
            "Date": date_str,
            "Quantity": float(quantity_dec),
            "UnitPrice": float(unitPrice_dec),
            "Valuation": float(valuation_dec)
        }