import re

class DocumentClassifier:
    """Classifies the type of financial document based on PDF text content using rule-based keyword matching."""
    
    @staticmethod
    def classify(text: str) -> str:
        # 1. Check for Bank Statement (ActivoBank)
        if "ActivoBank" in text and "Extrato" in text:
            return "bank_statement"
        
        # 2. Check for Payroll / Payslip
        if "Recibo de Vencimento" in text and re.search(r"NIF:\s*\d{9}", text):
            return "payroll"
            
        # 3. Check for Lusitânia Vida (Pension Insurance)
        if "Lusitania Vida" in text and "PPR" in text:
            return "pension_insurance"
            
        # 4. Check for AR PPR Banco Invest (Pension Fund)
        if "PLANOS DE POUPANÇA - AR PPR" in text:
            return "pension_fund"
            
        return "unknown"