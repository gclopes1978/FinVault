using ClosedXML.Excel;
using System.Globalization;

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
}