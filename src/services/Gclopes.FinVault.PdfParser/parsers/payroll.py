import pdfplumber
import re
from decimal import Decimal

# Rubric configuration table (provided by the user via Payroll.json).
# Each Codigo maps to a canonical OutputField and indicates which columns
# that rubric has on the PDF line (Data, Quantidade, Taxa, valor_unitario),
# and whether it is an Abono (earning) or Desconto (deduction).
#
# There are two payslip versions, distinguished by code range:
#   - Version 1: 5-digit codes (>= 10000)
#   - Version 2: shorter codes (< 10000)
# Codes never overlap between versions, so the version is derived directly
# from the code found on each line. Any code not present in this table is
# discarded.
CODE_TABLE = {
    # --- Version 1 (5-digit codes) ---
    10000: {"OutputField": "Vencimento_Base", "Data": False, "Quantidade": False, "Taxa": False, "valor_unitario": False, "Abono": True},
    10450: {"OutputField": "Isencao_Horario", "Data": False, "Quantidade": False, "Taxa": False, "valor_unitario": False, "Abono": True},
    20210: {"OutputField": "Subsidio_Alimentacao", "Data": False, "Quantidade": True, "Taxa": False, "valor_unitario": True, "Abono": True},
    33328: {"OutputField": "Ajudas_Custo", "Data": False, "Quantidade": True, "Taxa": False, "valor_unitario": True, "Abono": True},
    57000: {"OutputField": "Retencao_IRS_1", "Data": False, "Quantidade": False, "Taxa": False, "valor_unitario": False, "Abono": False},
    57060: {"OutputField": "Taxa_Retencao_IRS_1", "Data": False, "Quantidade": False, "Taxa": True, "valor_unitario": False, "Abono": False},
    65920: {"OutputField": "Retencao_Seguranca_Social", "Data": False, "Quantidade": True, "Taxa": True, "valor_unitario": False, "Abono": False},
    63270: {"OutputField": "Retencao_IRS_Fonte", "Data": False, "Quantidade": True, "Taxa": True, "valor_unitario": False, "Abono": False},
    75550: {"OutputField": "Seguro_Saude_Plus", "Data": False, "Quantidade": False, "Taxa": False, "valor_unitario": False, "Abono": False},
    57100: {"OutputField": "Retencao_IRS_2", "Data": False, "Quantidade": False, "Taxa": True, "valor_unitario": False, "Abono": False},
    13600: {"OutputField": "Subsidio_Natal", "Data": False, "Quantidade": True, "Taxa": True, "valor_unitario": False, "Abono": False},
    13310: {"OutputField": "Subsidio_Ferias", "Data": False, "Quantidade": True, "Taxa": True, "valor_unitario": False, "Abono": True, "IgnoreRate": True},
    57200: {"OutputField": "Retencao_IRS_3", "Data": False, "Quantidade": False, "Taxa": True, "valor_unitario": False, "Abono": False},

    # --- Version 2 (short codes) ---
    1001: {"OutputField": "Vencimento_Base", "Data": True, "Quantidade": False, "Taxa": False, "valor_unitario": False, "Abono": True},
    1002: {"OutputField": "Isencao_Horario", "Data": True, "Quantidade": True, "Taxa": False, "valor_unitario": False, "Abono": True},
    422: {"OutputField": "Subsidio_Alimentacao", "Data": True, "Quantidade": True, "Taxa": False, "valor_unitario": True, "Abono": True},
    1551: {"OutputField": "Seguro_Poupanca_Reforma", "Data": True, "Quantidade": False, "Taxa": False, "valor_unitario": False, "Abono": True},
    # Updated per user confirmation: Kilometros Isento = Quantidade (km) x valor_unitario (price/km)
    1583: {"OutputField": "Ajudas_Custo", "Data": True, "Quantidade": True, "Taxa": False, "valor_unitario": True, "Abono": True},
    1632: {"OutputField": "Subsidio_Ferias", "Data": True, "Quantidade": True, "Taxa": False, "valor_unitario": False, "Abono": True},
    1566: {"OutputField": "Deducao_Seguro_Poupanca_Reforma", "Data": True, "Quantidade": False, "Taxa": True, "valor_unitario": False, "Abono": False},
    350: {"OutputField": "Retencao_Seguranca_Social", "Data": True, "Quantidade": False, "Taxa": True, "valor_unitario": False, "Abono": False},
    401: {"OutputField": "Retencao_IRS_1", "Data": True, "Quantidade": False, "Taxa": True, "valor_unitario": False, "Abono": False},
    403: {"OutputField": "Retencao_IRS_2", "Data": True, "Quantidade": False, "Taxa": True, "valor_unitario": False, "Abono": False},
}

_VERSION_1_THRESHOLD = 10000  # codes >= this value belong to Version 1


def _get_version(code: int) -> str:
    return "v1" if code >= _VERSION_1_THRESHOLD else "v2"


# --- Number formats observed on real payslip lines ------------------------
#
# Version 1 (5-digit codes):
#   Amount:    space thousands separator, dot decimal   e.g. "1 500.00"
#   Quantity:  space thousands separator, comma decimal  e.g. "1 234,56"
#   UnitValue: dot decimal, 3 digits, no thousands        e.g. "6.032"
#   Rate alone (only numeric field on the line): comma decimal e.g. "23,00"
#   Rate with a Quantidade on the same line: same shape as UnitValue e.g. "11.000"
#
# Version 2 (short codes):
#   All fields (amount, quantity, rate, unit value) use the classic PT
#   money format: optional dot thousands separator, comma decimal, 2 digits
#   e.g. "500,00", "1.500,00"
_DATE = r"\d{4}/\d{2}"  # Always YYYY/MM, no day

_V1_AMOUNT = r"-?\d{1,3}(?:\s\d{3})*\.\d{2}"
_V1_QUANTITY = r"-?\d{1,3}(?:\s\d{3})*,\d{2}"
_V1_UNIT_VALUE = r"-?\d+\.\d{3}"
_V1_RATE_ALONE = _V1_QUANTITY
_V1_RATE_WITH_QUANTITY = _V1_UNIT_VALUE

_V2_MONEY = r"-?\d{1,3}(?:\.\d{3})*,\d{2}"  # used for every numeric field in Version 2

_LINE_START_PATTERN = re.compile(r"^(\d+)\s+(.*)$")


def _parse_number(value_str: str, decimal_separator: str) -> Decimal:
    """
    Converts a numeric token to Decimal.
    decimal_separator: the character used as the decimal point in value_str
    ("," or "."). Any space or dot used purely as a thousands separator is
    stripped first (dots are only stripped when comma is the decimal
    separator, since in that case dots can only mean thousands grouping).
    """
    clean = value_str.replace(" ", "").strip()
    if decimal_separator == ",":
        clean = clean.replace(".", "").replace(",", ".")
    return Decimal(clean)


def _build_expected_fields(config: dict, version: str) -> list:
    """
    Builds the ordered list of (field_name, pattern, decimal_separator)
    expected on a line, based on the CODE_TABLE flags for this rubric and
    the payslip version (which determines the number format).
    """
    fields = []
    if config["Data"]:
        fields.append(("date", _DATE, None))

    if version == "v1":
        if config["Quantidade"]:
            fields.append(("quantity", _V1_QUANTITY, ","))
        if config["Taxa"]:
            if config["Quantidade"]:
                fields.append(("rate", _V1_RATE_WITH_QUANTITY, "."))
            else:
                fields.append(("rate", _V1_RATE_ALONE, ","))
        if config["valor_unitario"]:
            fields.append(("unitValue", _V1_UNIT_VALUE, "."))
    else:  # v2 — every field uses the same classic PT money format
        if config["Quantidade"]:
            fields.append(("quantity", _V2_MONEY, ","))
        if config["Taxa"]:
            fields.append(("rate", _V2_MONEY, ","))
        if config["valor_unitario"]:
            fields.append(("unitValue", _V2_MONEY, ","))

    return fields


def _final_amount_pattern(version: str) -> str:
    return _V1_AMOUNT if version == "v1" else _V2_MONEY


def _extract_trailing_tokens(remainder: str, config: dict, version: str) -> dict:
    """
    Extracts, in the expected order (Data, Quantidade, Taxa, valor_unitario,
    final Amount), the fields present for this rubric. The final Amount
    (the actual earning/deduction value) is always present, EXCEPT for
    Version 1 rubrics whose only expected field is the rate itself (e.g.
    57060 - "Taxa_Retencao_IRS_X"), where the rate value IS the data to
    store.
    """
    expected_fields = _build_expected_fields(config, version)
    result = {"date": None, "quantity": None, "rate": None, "unitValue": None, "amount": None}

    # Special case (Version 1 only): rubrics that only have a Taxa field
    # have no separate final amount on the line; the rate value itself is
    # the amount.
    if version == "v1" and expected_fields == [("rate", _V1_RATE_ALONE, ",")]:
        match = re.search(rf"({_V1_RATE_ALONE})\s*$", remainder)
        if match:
            value = _parse_number(match.group(1), ",")
            result["rate"] = float(value)
            result["amount"] = float(value)
            result["description"] = remainder[: match.start()].strip()
        return result

    amount_pattern = _final_amount_pattern(version)
    combined_pattern = r"\s+".join(rf"({pattern})" for _, pattern, _ in expected_fields)
    if combined_pattern:
        full_pattern = rf"{combined_pattern}\s+({amount_pattern})\s*$"
    else:
        full_pattern = rf"({amount_pattern})\s*$"

    match = re.search(full_pattern, remainder)
    if not match:
        return result

    groups = match.groups()
    for (field_name, _, decimal_separator), value in zip(expected_fields, groups[:-1]):
        if field_name == "date":
            result["date"] = value.replace("/", "-")
        else:
            result[field_name] = float(_parse_number(value, decimal_separator))

    amount_decimal_separator = "." if version == "v1" else ","
    result["amount"] = float(_parse_number(groups[-1], amount_decimal_separator))
    result["description"] = remainder[: match.start()].strip()
    return result


class PayrollParser:
    """Parses payroll slips (2 versions, distinguished by code range) using
    the CODE_TABLE configuration table provided by the user."""

    @staticmethod
    def parse(pdf: pdfplumber.PDF) -> list:
        items = []

        for page in pdf.pages:
            text = page.extract_text() or ""
            for raw_line in text.split("\n"):
                line = raw_line.strip()

                if not line:
                    continue

                # A leading "/" is just a marker character in this payslip
                # layout — discard the slash itself, not the whole line.
                if line.startswith("/"):
                    line = line[1:].lstrip()

                line_match = _LINE_START_PATTERN.match(line)
                if not line_match:
                    continue

                code_str, remainder = line_match.groups()
                code = int(code_str)

                config = CODE_TABLE.get(code)
                if config is None:
                    # Code not present in CODE_TABLE — discard the line
                    continue

                version = _get_version(code)
                extracted = _extract_trailing_tokens(remainder, config, version)
                if extracted["amount"] is None:
                    # Could not find the final amount on the line — skip
                    continue

                # Some rubrics have a Taxa value on the line purely for
                # positional matching, but the value itself is not meant
                # to be persisted (e.g. 13310 - Subsidio_Ferias).
                if config.get("IgnoreRate"):
                    extracted["rate"] = None

                items.append({
                    "code": code,
                    "field": config["OutputField"],
                    "description": extracted.get("description"),
                    "date": extracted["date"],
                    "quantity": extracted["quantity"],
                    "rate": extracted["rate"],
                    "unitValue": extracted["unitValue"],
                    "amount": extracted["amount"],
                    "type": "abono" if config["Abono"] else "desconto",
                })

        return items