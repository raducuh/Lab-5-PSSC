namespace Magazin.Domain.ValueObjects;

/// <summary>Motivele pentru care un text sau un număr nu poate deveni o <see cref="Quantity"/> validă.</summary>
public abstract record QuantityError
{
    private QuantityError()
    {
    }

    /// <summary>Textul primit nu reprezintă un număr întreg.</summary>
    public sealed record NotANumber(string? Raw) : QuantityError;

    /// <summary>Numărul este în afara intervalului permis pentru o linie din coș.</summary>
    public sealed record OutOfRange(int Value) : QuantityError;
}
