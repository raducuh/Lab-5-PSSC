using System.Globalization;

namespace Magazin.ConsoleApp;

/// <summary>Afișarea unui tabel în consolă, cu coloane aliniate, și formatarea valorilor independent de cultura calculatorului.</summary>
internal static class ConsoleTable
{
    public static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);

    public static string Money(decimal value) => value.ToString("0.00", CultureInfo.InvariantCulture);

    public static string Date(DateTime value) => value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

    public static void Print(string title, string[] headers, IReadOnlyList<string[]> rows)
    {
        int[] widths =
        [
            .. headers.Select((header, column) => Math.Max(header.Length, rows.Select(row => row[column].Length).DefaultIfEmpty(0).Max())),
        ];

        Console.WriteLine($"=== {title} ({rows.Count} înregistrări) ===");
        Console.WriteLine(FormatRow(headers, widths));
        Console.WriteLine(string.Join("-+-", widths.Select(width => new string('-', width))));
        foreach (string[] row in rows)
        {
            Console.WriteLine(FormatRow(row, widths));
        }
    }

    private static string FormatRow(string[] cells, int[] widths) =>
        string.Join(" | ", cells.Select((cell, column) => cell.PadRight(widths[column])));
}
