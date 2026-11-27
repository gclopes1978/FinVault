import re

class DocumentClassifier:
    """Classifies the type of financial document based on PDF text content using rule-based keyword matching."""
    
    @staticmethod
    def classify(text: str) -> str:
        text_lower = re.sub(r"\s+", " ", text.lower())

        if re.search(r"\btrading\s*212\b", text_lower) and (
            "conta invest" in text_lower or "conta crypto" in text_lower
        ):
            return "trading212_statement"

        # 1. Check for Bank Statement (activobank)
        if "activobank" in text_lower and "extrato" in text_lower:
            return "bank_statement"
        
        # 2. Check for Lusitânia Vida (Pension Insurance)
        if "movimento recibo data movimento débito crédito saldo" in text_lower:
            return "pension_insurance"
            
        # 3. Check for AR PPR Banco Invest (Pension Fund)
        if "bancoinvest" in text:
            return "pension_fund"
            
        return "unknown"