import unittest

from classifier import DocumentClassifier


class Trading212ClassificationTests(unittest.TestCase):
    def test_classifies_trading212_statement_with_line_breaks(self):
        text = "Trading 212\nConta Invest - transações executadas"

        self.assertEqual(
            DocumentClassifier.classify(text),
            "trading212_statement",
        )

    def test_does_not_classify_unrelated_invest_account_text(self):
        text = "Conta Invest - resumo das posições em aberto"

        self.assertEqual(DocumentClassifier.classify(text), "unknown")


if __name__ == "__main__":
    unittest.main()
