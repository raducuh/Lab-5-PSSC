namespace Magazin.Domain.ValueObjects;

/// <summary>Motivele pentru care datele primite nu pot deveni o <see cref="Address"/> validă.</summary>
public abstract record AddressError
{
    private AddressError()
    {
    }

    /// <summary>Strada lipsește sau conține doar spații.</summary>
    public sealed record EmptyStreet : AddressError;

    /// <summary>Orașul lipsește sau conține doar spații.</summary>
    public sealed record EmptyCity : AddressError;

    /// <summary>Adresa scrisă ca text depășește lungimea maximă acceptată de baza de date.</summary>
    public sealed record TooLong(int MaxLength) : AddressError;

    /// <summary>Codul poștal nu are exact 6 cifre.</summary>
    public sealed record InvalidPostalCode(string? Raw) : AddressError;
}
