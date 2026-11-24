using System.Globalization;
using Magazin.Functional;

namespace Magazin.Domain.ValueObjects;

/// <summary>Suma este negativă, deci nu poate fi un preț.</summary>
public sealed record NegativeAmount(decimal Value);

/// <summary>
/// O sumă de bani în lei, cu două zecimale. Obiect-valoare: imutabil, construit doar prin <see cref="Create"/>
/// (care refuză sumele negative) sau prin operațiile <see cref="Plus"/>/<see cref="Times"/>, care păstrează invariantul.
/// </summary>
public sealed record Money
{
    /// <summary>Moneda tuturor sumelor din acest domeniu.</summary>
    public const string Currency = "RON";

    /// <summary>Numărul de zecimale păstrate.</summary>
    public const int Scale = 2;

    public decimal Amount { get; }

    private Money(decimal amount) => Amount = decimal.Round(amount, Scale, MidpointRounding.AwayFromZero);

    /// <summary>Suma zero: elementul neutru la adunare.</summary>
    public static Money Zero { get; } = new(0m);

    /// <summary>Construiește o sumă dintr-o valoare numerică, sau întoarce motivul pentru care nu este valabilă.</summary>
    public static Result<Money, NegativeAmount> Create(decimal amount) =>
        amount < 0m
            ? new NegativeAmount(amount)
            : new Money(amount);

    /// <summary>Suma a două sume valabile este întotdeauna valabilă.</summary>
    public Money Plus(Money other) => new(Amount + other.Amount);

    /// <summary>Prețul unei linii: suma înmulțită cu o cantitate (valabilă, deci pozitivă).</summary>
    public Money Times(Quantity quantity) => new(Amount * quantity.Value);

    /// <summary>Totalul unui șir de sume (zero pentru un șir gol).</summary>
    public static Money Sum(IEnumerable<Money> values) => values.Aggregate(Zero, (total, value) => total.Plus(value));

    public override string ToString() => $"{Amount.ToString("0.00", CultureInfo.InvariantCulture)} {Currency}";
}
