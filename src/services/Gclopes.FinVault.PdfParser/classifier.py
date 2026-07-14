import re

class DocumentClassifier:
    """Classifies the type of financial document based on PDF text content using rule-based keyword matching."""
    
    @staticmethod
    def classify(text: str) -> str:
        text_lower = text.lower()
        print(f"[DocumentClassifier] Classifying document based on text: {text_lower}...")  
        # 1. Check for Bank Statement (activobank)
        if "activobank" in text_lower and "extrato" in text_lower:
            return "bank_statement"
        
        # 2. Check for Payroll / Payslip
        if "recibo vencimento" in text_lower or "recibo de vencimento" in text_lower:
            return "payroll"
            
        # 3. Check for Lusitânia Vida (Pension Insurance)
        if "movimento recibo data movimento débito crédito saldo" in text_lower:
            return "pension_insurance"
            
        # 4. Check for AR PPR Banco Invest (Pension Fund)
        if "bancoinvest" in text:
            return "pension_fund"
            
        return "unknown"