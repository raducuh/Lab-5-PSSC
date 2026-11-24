using System.Text.RegularExpressions;
using Magazin.Functional;

namespace Magazin.Domain.ValueObjects;

/// <summary>
/// Codul unui produs din catalog (format <c>PR0001</c>: două litere mari urmate de exact 4 cifre).
/// Obiect-valoare cu un singur mod de eșec, deci eroarea sa (<see cref="InvalidProductCodeFormat"/>)
/// nu are nevoie de o ierarhie închisă.
/// </summary>
public sealed partial record ProductCode
{
    /// <summary>Formatul acceptat.</summary>
    public const string PatternText = "^[A-Z]{2}[0-9]{4}$";

    [GeneratedRegex(PatternText)]
    private static partial Regex Pattern { get; }

    public string Value { get; }

    private ProductCode(string value) => Value = value;

    /// <summary>Construiește un cod validat (spațiile de la capete și literele mici sunt normalizate) sau întoarce motivul eșecului.</summary>
    public static Result<ProductCode, InvalidProductCodeFormat> Create(string? raw)
    {
        string? normalized = raw?.Trim().ToUpperInvariant();
        return normalized is not null && Pattern.IsMatch(normalized)
            ? new ProductCode(normalized)
            : new InvalidProductCodeFormat(raw);
    }

    public override string ToString() => Value;
}

/// <summary>Textul primit nu respectă formatul <see cref="ProductCode.PatternText"/>.</summary>
public sealed record InvalidProductCodeFormat(string? Raw);
