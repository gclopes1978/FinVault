import unittest

from parsers.trading212_statement import Trading212StatementParser


class FakePage:
    def __init__(self, page_number, words):
        self.page_number = page_number
        self.words = words

    def extract_words(self, **_):
        return self.words


def make_header(labels, top=20):
    words = []
    x = 10

    for label in labels:
        for token in label.split():
            width = max(len(token) * 2, 4)
            words.append({
                "text": token,
                "x0": x,
                "x1": x + width,
                "top": top,
                "bottom": top + 8,
            })
            x += width + 2
        x += 100

    return words


class Trading212StatementParserTests(unittest.TestCase):
    def test_continues_section_to_following_page(self):
        title = make_header(["Conta Invest - transações e dividendos"])
        header = make_header(["Hora", "Tipo", "Montante"], top=20)
        row = make_header(["2026-01-02", "Dividend", "12,50"], top=40)

        result = Trading212StatementParser.parse(FakePdf([
            FakePage(1, title),
            FakePage(2, header + row),
        ]))

        tables = result["investCashTransactions"]
        self.assertEqual(len(tables), 1)
        self.assertEqual(tables[0]["page"], 2)
        self.assertEqual(tables[0]["records"], [{
            "time": "2026-01-02",
            "type": "Dividend",
            "amount": "12,50",
        }])

    def test_merges_wrapped_transaction_lines(self):
        title = make_header(["Conta Invest - transações e dividendos"])
        header = make_header(["Hora", "Tipo", "Montante"], top=20)
        transaction = (
            make_header(["2026-01-02"], top=40)
            + make_header(["", "Deposit", "100.00"], top=44.5)
            + make_header(["10:32:00"], top=49)
            + make_header(["Footer"], top=80)
        )

        result = Trading212StatementParser.parse(FakePdf([
            FakePage(1, title),
            FakePage(2, header + transaction),
        ]))

        self.assertEqual(
            result["investCashTransactions"][0]["records"],
            [{
                "time": "2026-01-02 10:32:00",
                "type": "Deposit",
                "amount": "100.00",
            }],
        )

    def test_splits_adjacent_account_currency_headers(self):
        words = [
            {"text": "RENDIMENTO", "x0": 750, "x1": 780, "top": 83, "bottom": 91},
            {"text": "MOEDA", "x0": 750, "x1": 762, "top": 90, "bottom": 98},
            {"text": "DA", "x0": 763, "x1": 769, "top": 90, "bottom": 98},
            {"text": "CONTA)VALOR", "x0": 750, "x1": 780, "top": 97, "bottom": 105},
            {"text": "MOEDA", "x0": 781, "x1": 793, "top": 97, "bottom": 105},
            {"text": "DA", "x0": 794, "x1": 800, "top": 97, "bottom": 105},
            {"text": "CONTA)", "x0": 801, "x1": 816, "top": 97, "bottom": 105},
        ]
        lines = Trading212StatementParser._group_words_into_lines(words)

        positions = Trading212StatementParser._detect_column_positions(
            lines,
            "investPositions",
        )

        self.assertIn("account_currency_return", positions)
        self.assertIn("account_currency_value", positions)
        self.assertGreater(
            positions["account_currency_value"]
            - positions["account_currency_return"],
            10,
        )

    def test_recognizes_compound_trading212_headers(self):
        cases = {
            "investTransactions": [
                "Prazo de execução", "Instrumento", "ISIN", "ID de ordem",
                "Direção", "Quantidade", "Preço de execução", "Valor",
                "Tipo de ordem", "Local de execução", "Sessão", "Taxa FX",
                "Taxa FX", "Governamentais", "Voltar", "Valor",
            ],
            "investPositions": [
                "Instrumento", "ISIN", "Quantidade", "Preço médio", "Preço",
                "Instrumento", "Instrumento", "Taxa FX",
                "Rendimento moeda da conta", "Valor moeda da conta",
            ],
            "dividends": [
                "Instrumento", "ISIN", "País emissor", "Ativos elegíveis",
                "Data de pagamento", "Valor por ação", "Instrumento",
                "Taxa de WHT", "WHT", "Taxa FX", "Conta",
            ],
            "cryptoTransactions": [
                "Prazo de execução", "Símbolo", "Ativo", "ID de ordem",
                "ID de execução", "Direção", "Quantidade",
                "Preço de execução", "Valor", "P/L realizados",
            ],
            "cryptoPositions": [
                "Símbolo", "Ativo", "Quantidade", "Preço de abertura",
                "Preço", "P/L não realizados", "Valor",
            ],
        }

        for table_type, labels in cases.items():
            with self.subTest(table_type=table_type):
                lines = Trading212StatementParser._group_words_into_lines(
                    make_header(labels)
                )
                header = Trading212StatementParser._find_header(
                    lines,
                    0,
                    table_type,
                )

                self.assertIsNotNone(header)
                self.assertGreaterEqual(
                    len(header["column_positions"]),
                    Trading212StatementParser.MIN_HEADER_MATCHES[table_type],
                )
                self.assertEqual(
                    set(Trading212StatementParser.COLUMNS[table_type])
                    - set(header["column_positions"]),
                    set(),
                )


class FakePdf:
    def __init__(self, pages):
        self.pages = pages


if __name__ == "__main__":
    unittest.main()
