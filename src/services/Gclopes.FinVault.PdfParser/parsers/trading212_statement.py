import re
import unicodedata


class Trading212StatementParser:
    """
    Extracts Trading212 activity statement tables using PDF word coordinates.

    Physical column positions are preserved, including repeated headers.
    No document content is logged or printed.
    """

    SECTIONS = {
        "investTransactions": "conta invest transacoes executadas",
        "investPositions": "conta invest resumo das posicoes em aberto",
        "investCashTransactions": "conta invest transacoes e dividendos",
        "cryptoTransactions": "conta crypto transacoes executadas",
        "cryptoPositions": "conta crypto resumo das posicoes em aberto",
        "cryptoCashTransactions": "conta crypto transacoes",
    }

    # Physical column order in the Trading212 report.
    COLUMNS = {
        "investTransactions": [
            "execution_date",
            "instrument",
            "isin",
            "order_id",
            "direction",
            "quantity",
            "execution_price",
            "execution_value",
            "order_type",
            "execution_venue",
            "session",
            "exchange_rate",
            "exchange_fee",
            "exchange_and_government_taxes",
            "return_value",
            "final_value",
        ],
        "investPositions": [
            "instrument",
            "isin",
            "quantity",
            "average_price",
            "current_price",
            "instrument_currency_return",
            "instrument_currency_value",
            "exchange_rate",
            "account_currency_return",
            "account_currency_value",
        ],
        "investCashTransactions": [
            "time",
            "type",
            "amount",
        ],
        "dividends": [
            "instrument",
            "isin",
            "issuer_country",
            "eligible_assets",
            "payment_date",
            "amount_per_share",
            "total_instrument_currency",
            "withholding_tax_rate",
            "withholding_tax",
            "exchange_rate",
            "total_account_currency",
        ],
        "cryptoTransactions": [
            "execution_date",
            "asset_symbol",
            "asset_name",
            "order_id",
            "execution_id",
            "direction",
            "quantity",
            "execution_price",
            "value",
            "realized_profit_loss",
        ],
        "cryptoPositions": [
            "asset_symbol",
            "asset_name",
            "quantity",
            "open_price",
            "current_price",
            "unrealized_profit_loss",
            "value",
        ],
        "cryptoCashTransactions": [
            "time",
            "type",
            "amount",
        ],
    }

    REQUIRED_RECORD_FIELDS = {
        "investTransactions": ["execution_date", "instrument", "quantity"],
        "investPositions": ["instrument", "quantity"],
        "investCashTransactions": ["time", "type", "amount"],
        "dividends": ["instrument", "payment_date", "amount_per_share"],
        "cryptoTransactions": ["execution_date", "asset_symbol", "quantity"],
        "cryptoPositions": ["asset_symbol", "quantity"],
        "cryptoCashTransactions": ["time", "type", "amount"],
    }

    HEADER_ALIASES = {
        "execution_date": [
            "prazo de execucao",
            "data de execucao",
            "hora de execucao",
        ],
        "instrument": ["instrumento"],
        "isin": ["isin"],
        "order_id": ["id de ordem"],
        "direction": ["direcao"],
        "quantity": ["quantidade"],
        "execution_price": ["preco de execucao"],
        "execution_value": ["valor"],
        "order_type": ["tipo de ordem"],
        "execution_venue": ["local de execucao"],
        "session": ["sessao"],
        "exchange_rate": ["taxa fx"],
        "exchange_fee": ["taxa fx"],
        "exchange_and_government_taxes": [
            "taxas de cambio e governamentais",
            "taxas de cambio",
            "governamentais",
        ],
        "return_value": ["voltar"],
        "final_value": ["valor"],
        "average_price": ["preco medio"],
        "current_price": ["preco"],
        "instrument_currency_return": [
            "rendimento moeda do instrumento",
            "instrumento",
        ],
        "instrument_currency_value": [
            "valor moeda do instrumento",
            "instrumento",
        ],
        "account_currency_return": [
            "rendimento moeda da conta",
        ],
        "account_currency_value": [
            "valor moeda da conta",
            "valor",
        ],
        "time": ["hora"],
        "type": ["tipo"],
        "amount": ["montante"],
        "issuer_country": ["pais emissor"],
        "eligible_assets": ["ativos elegiveis"],
        "payment_date": ["data de pagamento"],
        "amount_per_share": ["valor por acao"],
        "total_instrument_currency": [
            "montante total moeda do instrumento",
            "instrumento",
        ],
        "withholding_tax_rate": ["taxa de wht"],
        "withholding_tax": ["wht"],
        "total_account_currency": [
            "montante total moeda da conta",
            "conta",
        ],
        "asset_symbol": ["simbolo"],
        "asset_name": ["ativo"],
        "execution_id": ["id de execucao"],
        "open_price": ["preco de abertura"],
        "value": ["valor"],
        "unrealized_profit_loss": [
            "p l nao realizados",
            "lucros e perdas nao realizados",
        ],
        "realized_profit_loss": [
            "p l lucros e perdas realizados",
            "lucros e perdas realizados",
            "p l realizados",
            "realizados",
        ],
    }

    # Minimum number of recognized columns required to accept a header.
    MIN_HEADER_MATCHES = {
        "investTransactions": 8,
        "investPositions": 7,
        "investCashTransactions": 3,
        "dividends": 7,
        "cryptoTransactions": 7,
        "cryptoPositions": 5,
        "cryptoCashTransactions": 3,
    }
    MAX_ROW_LINE_GAP = 8

    @staticmethod
    def parse(pdf):
        result = {
            key: []
            for key in (
                "investTransactions",
                "investPositions",
                "investCashTransactions",
                "dividends",
                "cryptoTransactions",
                "cryptoPositions",
                "cryptoCashTransactions",
            )
        }

        section = None

        for page in pdf.pages:
            words = page.extract_words(
                x_tolerance=2,
                y_tolerance=3,
                keep_blank_chars=False,
                use_text_flow=False,
            )

            lines = Trading212StatementParser._group_words_into_lines(
                words
            )

            page_text = Trading212StatementParser._normalize(
                " ".join(word["text"] for word in words)
            )

            detected_section = Trading212StatementParser._detect_section(
                page_text
            )

            if detected_section is not None:
                section = detected_section

            if section is None:
                continue

            tables = Trading212StatementParser._find_tables(
                lines,
                section,
            )

            for table_index, table in enumerate(tables):
                result[table["type"]].append({
                    "page": page.page_number,
                    "tableIndex": table_index,
                    "columns": Trading212StatementParser.COLUMNS[
                        table["type"]
                    ],
                    "records": table["records"],
                })

        return result

    @staticmethod
    def _normalize(text):
        text = unicodedata.normalize("NFKD", text or "")
        text = "".join(
            char for char in text
            if not unicodedata.combining(char)
        )
        text = text.encode("ascii", errors="ignore").decode("ascii")

        return re.sub(r"[^a-z0-9]+", " ", text.lower()).strip()

    @staticmethod
    def _group_words_into_lines(words, tolerance=3):
        lines = []

        for word in sorted(
            words,
            key=lambda item: (item["top"], item["x0"]),
        ):
            line = next(
                (
                    item for item in lines
                    if abs(item["top"] - word["top"]) <= tolerance
                ),
                None,
            )

            if line is None:
                line = {
                    "top": word["top"],
                    "bottom": word["bottom"],
                    "words": [],
                }
                lines.append(line)

            line["words"].append(word)
            line["top"] = min(line["top"], word["top"])
            line["bottom"] = max(line["bottom"], word["bottom"])

        for line in lines:
            line["words"].sort(key=lambda item: item["x0"])
            line["text"] = " ".join(
                word["text"] for word in line["words"]
            )

        return sorted(lines, key=lambda item: item["top"])

    @staticmethod
    def _detect_section(page_text):
        matches = [
            (key, title)
            for key, title in Trading212StatementParser.SECTIONS.items()
            if title in page_text
        ]

        if not matches:
            return None

        return max(matches, key=lambda item: len(item[1]))[0]

    @staticmethod
    def _find_tables(lines, section):
        """
        Finds table headers and extracts rows until the next table header
        or section boundary.
        """
        candidates = []

        possible_types = [section]

        if section == "investCashTransactions":
            possible_types.append("dividends")

        for index in range(len(lines)):
            for table_type in possible_types:
                header = Trading212StatementParser._find_header(
                    lines,
                    index,
                    table_type,
                )

                if header is not None:
                    candidates.append(header)

        # Remove overlapping header candidates.
        candidates.sort(
            key=lambda item: (
                item["start"],
                -item["score"],
            )
        )

        selected = []

        for candidate in candidates:
            if selected and candidate["start"] < selected[-1]["end"]:
                if candidate["score"] > selected[-1]["score"]:
                    selected[-1] = candidate
                continue

            selected.append(candidate)

        tables = []

        for index, header in enumerate(selected):
            next_start = (
                selected[index + 1]["start"]
                if index + 1 < len(selected)
                else len(lines)
            )

            records = Trading212StatementParser._extract_records(
                lines,
                header,
                next_start,
            )

            tables.append({
                "type": header["type"],
                "records": records,
            })

        return tables

    @staticmethod
    def _find_header(lines, start, table_type):


        best = None

        # Headers may span multiple physical lines.
        for height in range(1, min(7, len(lines) - start) + 1):
            header_lines = lines[start:start + height]

            column_positions = (
                Trading212StatementParser._detect_column_positions(
                    header_lines,
                    table_type,
                )
            )

            if column_positions is None:
                continue

            score = len(column_positions)

            minimum = Trading212StatementParser.MIN_HEADER_MATCHES[
                table_type
            ]

            if score < minimum:
                continue

            candidate = {
                "type": table_type,
                "start": start,
                "end": start + height,
                "score": score,
                "column_positions": column_positions,
            }

            if best is None or score > best["score"]:
                best = candidate

        return best

    @staticmethod
    def _detect_column_positions(header_lines, table_type):
        """
        Finds physical column centers from the header words.

        Each header field is matched independently. Repeated names are
        distinguished by their horizontal position.
        """
        expected = Trading212StatementParser.COLUMNS[table_type]

        anchors = []

        for line_index, line in enumerate(header_lines):
            normalized_words = []

            for word in line["words"]:
                normalized = Trading212StatementParser._normalize(
                    word["text"]
                )

                if normalized:
                    parts = normalized.split()

                    if ")" in word["text"] and len(parts) == 2:
                        split = word["x0"] + (
                            word["x1"] - word["x0"]
                        ) * (len(parts[0]) + 1) / len(word["text"])
                        ranges = [
                            (parts[0], word["x0"], split, False),
                            (parts[1], split, word["x1"], True),
                        ]
                    else:
                        ranges = [(
                            normalized,
                            word["x0"],
                            word["x1"],
                            False,
                        )]

                    for text, x0, x1, separate in ranges:
                        normalized_words.append({
                            "text": text,
                            "x0": x0,
                            "x1": x1,
                            "top": word["top"],
                            "center": (x0 + x1) / 2,
                            "separate": separate,
                        })

            line_anchors = []

            for word in normalized_words:
                if (
                    line_anchors
                    and not word["separate"]
                    and abs(
                        line_anchors[-1]["last_center"] - word["center"]
                    ) < 20
                ):
                    anchor = line_anchors[-1]
                    anchor["words"].append(word)
                    anchor["last_center"] = word["center"]
                    anchor["center"] = sum(
                        item["center"] for item in anchor["words"]
                    ) / len(anchor["words"])
                else:
                    line_anchors.append({
                        "center": word["center"],
                        "last_center": word["center"],
                        "words": [word],
                    })

            for anchor in line_anchors:
                anchor["line_indices"] = {line_index}

            if line_anchors:
                line_anchors.sort(key=lambda item: item["center"])
                anchors.extend(line_anchors)

        if not anchors:
            return None

        merged = []

        for anchor in anchors:
            target = next(
                (
                    item for item in merged
                    if not item["line_indices"].intersection(
                        anchor["line_indices"]
                    )
                    and abs(item["center"] - anchor["center"]) < 10
                ),
                None,
            )

            if target is None:
                merged.append(anchor)
            else:
                target["words"].extend(anchor["words"])
                target["words"].sort(
                    key=lambda word: (word["top"], word["x0"])
                )
                target["line_indices"].update(anchor["line_indices"])
                target["center"] = sum(
                    word["center"] for word in target["words"]
                ) / len(target["words"])

        merged.sort(key=lambda item: item["center"])

        # Match expected field aliases to physical positions.
        positions = {}
        used_anchors = set()

        for field in expected:
            aliases = Trading212StatementParser.HEADER_ALIASES.get(
                field,
                [],
            )

            matches = []

            for anchor_index, anchor in enumerate(merged):
                if anchor_index in used_anchors:
                    continue

                text = " ".join(
                    word["text"] for word in anchor["words"]
                )

                if any(
                    Trading212StatementParser._matches_alias(text, alias)
                    for alias in aliases
                ):
                    matches.append((anchor_index, anchor["center"]))

            if matches:
                anchor_index, center = matches[0]
                positions[field] = center
                used_anchors.add(anchor_index)

        if not positions:
            return None

        return positions

    @staticmethod
    def _matches_alias(text, alias):
        normalized_alias = Trading212StatementParser._normalize(alias)

        return re.search(
            rf"(?<![a-z0-9]){re.escape(normalized_alias)}(?![a-z0-9])",
            text,
        ) is not None

    @staticmethod
    def _extract_records(lines, header, next_start):
        """
        Extracts rows using the horizontal positions identified in
        the header. Values are kept as strings.
        """
        positions = header["column_positions"]

        ordered_positions = sorted(
            positions.items(),
            key=lambda item: item[1],
        )

        if not ordered_positions:
            return []

        boundaries = []

        for index, (_, center) in enumerate(ordered_positions):
            if index == 0:
                left = float("-inf")
            else:
                left = (
                    ordered_positions[index - 1][1] + center
                ) / 2

            if index == len(ordered_positions) - 1:
                right = float("inf")
            else:
                right = (
                    center + ordered_positions[index + 1][1]
                ) / 2

            boundaries.append((ordered_positions[index][0], left, right))

        records = []
        current_record = None
        last_line_top = None

        for line in lines[header["end"]:next_start]:
            values = {
                field: []
                for field in positions
            }

            for word in line["words"]:
                center = (word["x0"] + word["x1"]) / 2

                for field, left, right in boundaries:
                    if left <= center < right:
                        values[field].append(word["text"])
                        break

            record = {
                field: " ".join(parts).strip() or None
                for field, parts in values.items()
            }

            if not any(value is not None for value in record.values()):
                continue

            if (
                current_record is None
                or last_line_top is None
                or line["top"] - last_line_top
                > Trading212StatementParser.MAX_ROW_LINE_GAP
            ):
                if current_record is not None:
                    records.append(current_record)
                current_record = record
            else:
                for field, value in record.items():
                    if value is not None:
                        if current_record[field] is None:
                            current_record[field] = value
                        else:
                            current_record[field] += f" {value}"

            last_line_top = line["top"]

        if current_record is not None:
            records.append(current_record)

        required_fields = Trading212StatementParser.REQUIRED_RECORD_FIELDS[
            header["type"]
        ]
        return [
            record
            for record in records
            if all(record.get(field) is not None for field in required_fields)
        ]