import argparse
import json
import sys

import pdfplumber

from parsers.bank_statement import BankStatementParser
from parsers.pension_insurance import PensionInsuranceParser
from parsers.pension_fund import PensionFundParser
from parsers.trading212_statement import Trading212StatementParser

def classify_document(text: str) -> str:
    text_lower = text.lower()

    if "trading 212" in text_lower:
        return "trading212_statement"

    if "activobank" in text_lower and "extrato" in text_lower:
        return "bank_statement"

    if "movimento recibo data movimento débito crédito saldo" in text_lower:
        return "pension_insurance"

    if "bancoinvest" in text_lower:
        return "pension_fund"

    return "unknown"


def extract_document(file_path: str) -> dict:
    with pdfplumber.open(file_path) as pdf:
        if not pdf.pages:
            raise ValueError("The PDF contains no pages.")

        first_page_text = pdf.pages[0].extract_text() or ""
        document_type = classify_document(first_page_text)

        parsers = {
            "trading212_statement": Trading212StatementParser.parse,
            "bank_statement": BankStatementParser.parse,
            "pension_insurance": PensionInsuranceParser.parse,
            "pension_fund": PensionFundParser.parse,
        }

        parser = parsers.get(document_type)

        if parser is None:
            raise ValueError("The PDF document type is not supported.")

        return {
            "documentType": document_type,
            "data": parser(pdf),
        }


def main() -> int:
    argument_parser = argparse.ArgumentParser()
    argument_parser.add_argument("file", help="Path to the local PDF document")
    args = argument_parser.parse_args()

    try:
        result = extract_document(args.file)
        print(json.dumps(result, ensure_ascii=False))
        return 0
    except Exception as exception:
        print(str(exception), file=sys.stderr)
        return 1


if __name__ == "__main__":
    sys.exit(main())
