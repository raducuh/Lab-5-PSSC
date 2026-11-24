using System.Text.RegularExpressions;
using Magazin.Functional;

namespace Magazin.Domain.ValueObjects;

/// <summary>
/// Codul unui client (format <c>CL0001</c>: literele „CL” urmate de exact 4 cifre), așa cum apare în coloana
/// <c>Customers.Code</c>. Obiect-valoare cu un singur mod de eșec, deci eroarea sa nu are nevoie de o ierarhie închisă.
/// </summary>
public sealed partial record CustomerCode
{
    /// <summary>Formatul acceptat.</summary>
    public const string PatternText = "^CL[0-9]{4}$";

    [GeneratedRegex(PatternText)]
    private static partial Regex Pattern { get; }

    public string Value { get; }

    private CustomerCode(string value) => Value = value;

    /// <summary>Construiește un cod validat (spațiile de la capete și literele mici sunt normalizate) sau întoarce motivul eșecului.</summary>
    public static Result<CustomerCode, InvalidCustomerCodeFormat> Create(string? raw)
    {
        string? normalized = raw?.Trim().ToUpperInvariant();
        return normalized is not null && Pattern.IsMatch(normalized)
            ? new CustomerCode(normalized)
            : new InvalidCustomerCodeFormat(raw);
    }

    public override string ToString() => Value;
}

/// <summary>Textul primit nu respectă formatul <see cref="CustomerCode.PatternText"/>.</summary>
public sealed record InvalidCustomerCodeFormat(string? Raw);
