import pdfplumber
import re

class PayrollParser:
    """Parses company payslips to extract generic payroll information."""
    
    @staticmethod
    def parse(pdf: pdfplumber.PDF) -> dict:
        full_text = ""
        for page in pdf.pages:
            full_text += page.extract_text() or ""
            
        # Generic extraction samples for target parsing matching payslip anatomy
        net_salary_match = re.search(r"(?:Líquido a Receber|Valor Líquido):\s*(\d+[\.,]\d{2})", full_text)
        base_salary_match = re.search(r"(?:Vencimento Base|Salário Base):\s*(\d+[\.,]\d{2})", full_text)
        
        def parse_float(match):
            return float(match.group(1).replace(".", "").replace(",", ".")) if match else 0.0

        return {
            "baseSalary": parse_float(base_salary_match),
            "netSalary": parse_float(net_salary_match)
        }