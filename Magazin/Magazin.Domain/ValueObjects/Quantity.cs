using System.Globalization;
using Magazin.Functional;

namespace Magazin.Domain.ValueObjects;

/// <summary>
/// Cantitatea unui produs comandat: un număr întreg între <see cref="Minimum"/> și <see cref="Maximum"/>.
/// Obiect-valoare: imutabil, fără identitate, construit doar prin <see cref="Create"/>/<see cref="Parse"/>,
/// care întorc <see cref="Result{TSuccess, TFailure}"/> în loc să arunce excepții.
/// </summary>
public sealed record Quantity
{
    /// <summary>Cea mai mică cantitate permisă (inclusiv).</summary>
    public const int Minimum = 1;

    /// <summary>Cea mai mare cantitate permisă pe o linie din coș (inclusiv).</summary>
    public const int Maximum = 100;

    public int Value { get; }

    private Quantity(int value) => Value = value;

    /// <summary>Construiește o cantitate dintr-un număr deja cunoscut.</summary>
    public static Result<Quantity, QuantityError> Create(int value) =>
        value is >= Minimum and <= Maximum
            ? new Quantity(value)
            : new QuantityError.OutOfRange(value);

    /// <summary>Construiește o cantitate dintr-un text (consolă, formular web).</summary>
    public static Result<Quantity, QuantityError> Parse(string? raw) =>
        int.TryParse(raw, NumberStyles.None, CultureInfo.InvariantCulture, out int value)
            ? Create(value)
            : new QuantityError.NotANumber(raw);

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}
