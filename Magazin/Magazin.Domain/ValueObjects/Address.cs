using System.Text.RegularExpressions;
using Magazin.Functional;

namespace Magazin.Domain.ValueObjects;

/// <summary>
/// Adresa de livrare: stradă, oraș și cod poștal (6 cifre). Obiect-valoare: două adrese cu aceleași
/// componente sunt egale, iar o adresă nu se modifică după construire. Se construiește doar prin
/// <see cref="Create"/>, care adună <b>toate</b> erorile (nu doar prima).
/// </summary>
public sealed partial record Address
{
    /// <summary>Formatul codului poștal românesc: exact 6 cifre.</summary>
    public const string PostalCodePatternText = "^[0-9]{6}$";

    [GeneratedRegex(PostalCodePatternText)]
    private static partial Regex PostalCodePattern { get; }

    /// <summary>Lungimea maximă a adresei scrise ca text („stradă, oraș, cod poștal”): coloana <c>Order.DeliveryAddress</c> este <c>NVARCHAR(200)</c>.</summary>
    public const int MaxLength = 200;

    public string Street { get; }

    public string City { get; }

    public string PostalCode { get; }

    private Address(string street, string city, string postalCode)
    {
        Street = street;
        City = city;
        PostalCode = postalCode;
    }

    /// <summary>Construiește o adresă validată sau întoarce lista tuturor motivelor pentru care nu este valabilă.</summary>
    public static Result<Address, IReadOnlyList<AddressError>> Create(string? street, string? city, string? postalCode)
    {
        List<AddressError> errors = [];

        if (string.IsNullOrWhiteSpace(street))
        {
            errors.Add(new AddressError.EmptyStreet());
        }

        if (string.IsNullOrWhiteSpace(city))
        {
            errors.Add(new AddressError.EmptyCity());
        }

        string? code = postalCode?.Trim();
        if (code is null || !PostalCodePattern.IsMatch(code))
        {
            errors.Add(new AddressError.InvalidPostalCode(postalCode));
        }

        if (errors is [] && $"{street!.Trim()}, {city!.Trim()}, {code}".Length > MaxLength)
        {
            errors.Add(new AddressError.TooLong(MaxLength));
        }

        return errors is []
            ? new Address(street!.Trim(), city!.Trim(), code!)
            : errors;
    }

    public override string ToString() => $"{Street}, {City}, {PostalCode}";
}
