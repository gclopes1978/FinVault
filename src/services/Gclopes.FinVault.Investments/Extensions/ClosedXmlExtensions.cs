using ClosedXML.Excel;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Gclopes.FinVault.Investments.Extensions;

public static class ClosedXmlExtensions
{
    public static string? GetNullableString(this IXLCell cell)
    {
        var value = cell.GetString();

        return string.IsNullOrWhiteSpace(value)
            ? null
            : value;
    }

    public static decimal GetDecimalOrDefault(this IXLCell cell)
    {
        var value = cell.GetString();

        if (string.IsNullOrWhiteSpace(value))
            return 0m;

        if (decimal.TryParse(
                value,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var result))
        {
            return result;
        }

        throw new InvalidDataException(
            $"Cell '{cell.Address}' contains an invalid decimal value: '{value}'.");
    }

    public static DateTime GetDateTimeValue(this IXLCell cell)
    {
        var value = cell.GetString();

        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidDataException(
                $"Cell '{cell.Address}' contains an empty date value.");

        if (DateTime.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var result))
        {
            return result;
        }

        throw new InvalidDataException(
            $"Cell '{cell.Address}' contains an invalid date value: '{value}'.");
    }

    public static string GetT212Id(this IXLRow row)
    {
        var id = row.Cell(7).GetString();

        if (!string.IsNullOrWhiteSpace(id))
            return id;

        var key = string.Join("|",
            row.Cell(1).GetString(),
            row.Cell(2).GetString(),
            row.Cell(3).GetString(),
            row.Cell(4).GetString(),
            row.Cell(8).GetString(),
            row.Cell(9).GetString(),
            row.Cell(12).GetString());

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(key));

        return new Guid(hash[..16]).ToString();
    }
}