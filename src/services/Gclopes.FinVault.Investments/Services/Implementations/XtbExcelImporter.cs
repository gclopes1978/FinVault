using ClosedXML.Excel;
using Gclopes.FinVault.Investments.Data.Entities;
using Gclopes.FinVault.Investments.Extensions;
using Gclopes.FinVault.Investments.Services.Interfaces;

namespace Gclopes.FinVault.Investments.Services.Implementations;

public class XtbExcelImporter : IXtbExcelImporter
{
    public Task<XtbWorkbook> ParseAsync(
        Stream xlsxStream,
        CancellationToken cancellationToken = default)
    {
        using var workbook = new XLWorkbook(xlsxStream);

        var closedPositions = ReadClosedPositions(
            workbook.Worksheet("Closed Positions"));


        var cashOperations = ReadCashOperations(
            workbook.Worksheet("Cash Operations"));

        var openPositions = ReadOpenPositions(
            workbook.Worksheet("Open Positions"));

        return Task.FromResult(
            new XtbWorkbook(
                ClosedPositions: closedPositions,
                CashOperations: cashOperations,
                OpenPositions: openPositions));
    }

    private static void ValidateHeaders(
    IXLWorksheet worksheet,
    params string[] expectedHeaders)
    {
        var headerRow = worksheet.Row(5);

        for (var i = 0; i < expectedHeaders.Length; i++)
        {
            var actualHeader = headerRow.Cell(i + 1).GetString();

            if (!string.Equals(
                    actualHeader,
                    expectedHeaders[i],
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    $"Unexpected header in worksheet '{worksheet.Name}', " +
                    $"column {i + 1}. Expected '{expectedHeaders[i]}', " +
                    $"but found '{actualHeader}'.");
            }
        }
    }

    private static List<XtbCashOperation> ReadCashOperations(
        IXLWorksheet worksheet)
    {
        ValidateHeaders(
        worksheet,
        "Type",
        "Instrument",
        "Ticker",
        "Category",
        "Time",
        "Amount",
        "ID",
        "Comment",
        "Product",
        "Position ID");

        var operations = new List<XtbCashOperation>();

        // Header is on row 5, data starts on row 6.
        foreach (var row in worksheet.RowsUsed().Skip(5))
        {
            if (row.IsEmpty())
                continue;
            var time = row.Cell(5).GetNullableString();

            if (time is null)
                continue;

            operations.Add(new XtbCashOperation
            {
                Type = row.Cell(1).GetString(),
                Instrument = row.Cell(2).GetString(),
                Ticker = row.Cell(3).GetString(),
                Category = row.Cell(4).GetString(),
                Time = row.Cell(5).GetDateTimeValue(),
                Amount = row.Cell(6).GetDecimalOrDefault(),
                OperationId = row.Cell(7).GetString(),
                Comment = row.Cell(8).GetNullableString(),
                Product = row.Cell(9).GetString(),
                PositionId = row.Cell(10).GetNullableString()
            });
        }

        return operations;
    }
    private static List<XtbClosedPosition> ReadClosedPositions(
    IXLWorksheet worksheet)
    {
        ValidateHeaders(
            worksheet,
            "Instrument",
            "Ticker",
            "Category",
            "Type",
            "Volume",
            "Open Price",
            "Open Time (UTC)",
            "Close Price",
            "Close Time (UTC)",
            "Product",
            "Profit/Loss",
            "Gross Profit",
            "Purchase Value",
            "Sale Value",
            "Stop Loss",
            "Take Profit",
            "Commission",
            "Margin",
            "Swap",
            "Rollover",
            "Open Conversion Rate",
            "Close Conversion Rate",
            "Close Origin",
            "Position ID",
            "Comment");

        var positions = new List<XtbClosedPosition>();

        foreach (var row in worksheet.RowsUsed().Skip(5))
        {
            if (row.IsEmpty())
                continue;
            var type = row.Cell(4).GetNullableString();

            if (type is null)
                continue;

            positions.Add(new XtbClosedPosition
            {
                Instrument = row.Cell(1).GetString(),
                Ticker = row.Cell(2).GetString(),
                Category = row.Cell(3).GetString(),
                Type = type,
                Volume = row.Cell(5).GetDecimalOrDefault(),
                OpenPrice = row.Cell(6).GetDecimalOrDefault(),
                OpenTimeUtc = row.Cell(7).GetDateTimeValue(),
                ClosePrice = row.Cell(8).GetDecimalOrDefault(),
                CloseTimeUtc = row.Cell(9).GetDateTimeValue(),
                Product = row.Cell(10).GetString(),
                ProfitLoss = row.Cell(11).GetDecimalOrDefault(),
                GrossProfit = row.Cell(12).GetDecimalOrDefault(),
                PurchaseValue = row.Cell(13).GetDecimalOrDefault(),
                SaleValue = row.Cell(14).GetDecimalOrDefault(),
                StopLoss = row.Cell(15).GetDecimalOrDefault(),
                TakeProfit = row.Cell(16).GetDecimalOrDefault(),
                Commission = row.Cell(17).GetDecimalOrDefault(),
                Margin = row.Cell(18).GetDecimalOrDefault(),
                Swap = row.Cell(19).GetDecimalOrDefault(),
                Rollover = row.Cell(20).GetDecimalOrDefault(),
                OpenConversionRate = row.Cell(21).GetDecimalOrDefault(),
                CloseConversionRate = row.Cell(22).GetDecimalOrDefault(),
                CloseOrigin = row.Cell(23).GetNullableString(),
                PositionId = row.Cell(24).GetString(),
                Comment = row.Cell(25).GetNullableString()
            });
        }

        return positions;
    }
    private static List<XtbOpenPosition> ReadOpenPositions(
    IXLWorksheet worksheet)
    {
        ValidateHeaders(
            worksheet,
            "Product",
            "Instrument/Position",
            "Ticker",
            "Category",
            "Type",
            "Volume",
            "Value",
            "Current price",
            "Open price",
            "Open time (UTC)",
            "Stop Loss",
            "Take Profit",
            "Net Profit %",
            "Net Profit",
            "Gross Profit",
            "Margin",
            "Open Commission",
            "Swap",
            "Rollover");

        var positions = new List<XtbOpenPosition>();

        foreach (var row in worksheet.RowsUsed().Skip(5))
        {
            if (row.IsEmpty())
                continue;
            var type = row.Cell(5).GetNullableString();

            if (type is null)
                continue;

            positions.Add(new XtbOpenPosition
            {
                Product = row.Cell(1).GetString(),
                InstrumentPosition = row.Cell(2).GetString(),
                Ticker = row.Cell(3).GetString(),
                Category = row.Cell(4).GetString(),
                Type = type,
                Volume = row.Cell(6).GetDecimalOrDefault(),
                Value = row.Cell(7).GetDecimalOrDefault(),
                CurrentPrice = row.Cell(8).GetDecimalOrDefault(),
                OpenPrice = row.Cell(9).GetDecimalOrDefault(),
                OpenTimeUtc = row.Cell(10).GetDateTime(),
                StopLoss = row.Cell(11).GetDecimalOrDefault(),
                TakeProfit = row.Cell(12).GetDecimalOrDefault(),
                NetProfitPercent = row.Cell(13).GetDecimalOrDefault(),
                NetProfit = row.Cell(14).GetDecimalOrDefault(),
                GrossProfit = row.Cell(15).GetDecimalOrDefault(),
                Margin = row.Cell(16).GetDecimalOrDefault(),
                OpenCommission = row.Cell(17).GetDecimalOrDefault(),
                Swap = row.Cell(18).GetDecimalOrDefault(),
                Rollover = row.Cell(19).GetDecimalOrDefault()
            });
        }

        return positions;
    }
}