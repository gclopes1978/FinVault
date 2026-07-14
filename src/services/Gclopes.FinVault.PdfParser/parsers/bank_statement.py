import pdfplumber
import re
from datetime import datetime
from decimal import Decimal

class BankStatementParser:
    """Parses bank statements using strict regex boundary anchors to prevent description numeric collisions."""
    
    @staticmethod
    def parse(pdf: pdfplumber.PDF) -> dict:
        transactions = []
        full_text = ""
        
        for page in pdf.pages:
            full_text += page.extract_text() or ""
            
        lines = full_text.split("\n")
        inside_transactions = False
        
        statement_year = datetime.now().year
        statement_month = 1
        initial_balance = 0.0
        final_balance = 0.0
        
        # Internal tracker using Decimal for absolute financial accuracy
        running_balance_dec = Decimal("0.00")
        
        year_header_pattern = r"EXTRATO DE\s+(\d{4})/(\d{2})"
        date_block_pattern = r"^(\d{1,2})[\.:](\d{1,2})\s+(\d{1,2})[\.:](\d{1,2})"
        
        # Matches a clean financial block (optional thousands space, dot, two decimals)
        money_val_regex = r"-?\d{1,3}(?:\s\d{3})*\.\d{2}"

        for line in lines:
            line_str = line.strip()
            
            if (year_match := re.search(year_header_pattern, line_str)):
                statement_year = int(year_match.group(1))
                statement_month = int(year_match.group(2))
                continue

            if "A TRANSPORTAR" in line_str or "TRANSPORTE" in line_str:
                continue
                
            if "LANC" in line_str or "VALOR DESCRITIVO" in line_str:
                continue

            # Capture Initial Balance
            if "SALDO INICIAL" in line_str:
                inside_transactions = True
                balance_match = re.search(rf"({money_val_regex})\s*$", line_str)
                if balance_match:
                    clean_bal = balance_match.group(1).replace(" ", "")
                    running_balance_dec = Decimal(clean_bal)
                    initial_balance = float(running_balance_dec)
                continue
                
            if not inside_transactions:
                continue
                
            # Capture Final Balance and stop loops
            if "SALDO FINAL" in line_str:
                balance_match = re.search(rf"({money_val_regex})\s*$", line_str)
                if balance_match:
                    clean_bal = balance_match.group(1).replace(" ", "")
                    final_balance = float(Decimal(clean_bal))
                break
                
            if any(marker in line_str for marker in ["SALDO DISPONIVEL", "ULTRAPASSAGEM DE CREDITO", "TAXA ANUAL NOMINAL", "RESUMO DE CONTA"]):
                break
                
            # Check if line starts with the two transaction dates
            date_match = re.match(date_block_pattern, line_str)
            if date_match:
                # Look specifically for TWO well-formed money blocks at the end of the line
                # This guarantees that the '50' from the description cannot be sucked in,
                # because money blocks must follow the 1-3 digit thousands rule separated by spaces.
                line_money_pattern = rf"\s+({money_val_regex})\s+({money_val_regex})\s*$"
                money_match = re.search(line_money_pattern, line_str)
                
                if money_match:
                    raw_amount_str = money_match.group(1)
                    raw_balance_str = money_match.group(2)
                    
                    amount_cleaned = raw_amount_str.replace(" ", "")
                    balance_cleaned = raw_balance_str.replace(" ", "")
                    
                    try:
                        parsed_amount = Decimal(amount_cleaned)
                        new_balance = Decimal(balance_cleaned)
                        
                        # MATHEMATICAL TRUTH LAYER
                        math_diff = abs(new_balance - running_balance_dec)
                        tx_type = "C" if new_balance > running_balance_dec else "D"
                        
                        # If the parsed text matches the math, great. 
                        # Otherwise, the mathematical delta rules to avoid PDF text shift glitches.
                        if parsed_amount == math_diff:
                            tx_amount = float(parsed_amount)
                        else:
                            tx_amount = float(math_diff)
                            
                        running_balance_dec = new_balance
                        
                        # Cut the description strictly before the starting space of the amount block
                        description = line_str[date_match.end():money_match.start()].strip()
                        
                        # Date Mapping (M.DD)
                        month_launch = int(date_match.group(1))
                        day_launch = int(date_match.group(2))
                        month_month = int(date_match.group(3))
                        day_value = int(date_match.group(4))
                        
                        # Year Transitions
                        target_year_launch = statement_year
                        if statement_month == 1 and month_launch == 12:
                            target_year_launch = statement_year - 1
                        elif statement_month == 12 and month_launch == 1:
                            target_year_launch = statement_year + 1
                            
                        target_year_value = statement_year
                        if statement_month == 1 and month_month == 12:
                            target_year_value = statement_year - 1
                        elif statement_month == 12 and month_month == 1:
                            target_year_value = statement_year + 1
                            
                        full_launch_date = f"{str(day_launch).zfill(2)}-{str(month_launch).zfill(2)}-{target_year_launch}"
                        full_value_date = f"{str(day_value).zfill(2)}-{str(month_month).zfill(2)}-{target_year_value}"
                        
                        transactions.append({
                            "date": full_launch_date,
                            "valueDate": full_value_date,
                            "description": description,
                            "amount": tx_amount,
                            "type": tx_type,
                            "balance": float(new_balance)
                        })
                    except Exception:
                        continue
                        
        return {
            "initialBalance": initial_balance,
            "finalBalance": final_balance,
            "transactions": transactions
        }