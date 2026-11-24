using System.Diagnostics;
using System.Globalization;
using Magazin.Domain.ValueObjects;

namespace Magazin.Domain.Errors;

/// <summary>Un motiv pentru care datele brute ale comenzii nu au putut fi convertite în obiecte-valoare.</summary>
public abstract record ValidationError
{
    private ValidationError()
    {
    }

    /// <summary>Codul clientului nu respectă formatul așteptat.</summary>
    public sealed record InvalidCustomerCode(string? Raw) : ValidationError;

    /// <summary>Comanda nu conține nicio linie.</summary>
    public sealed record EmptyOrder : ValidationError;

    /// <summary>Codul produsului nu respectă formatul așteptat.</summary>
    public sealed record InvalidProductCode(string? Raw) : ValidationError;

    /// <summary>Cantitatea unui produs nu este valabilă.</summary>
    public sealed record InvalidQuantity(string? ProductCode, QuantityError Error) : ValidationError;

    /// <summary>Adresa de livrare conține o componentă nevalabilă.</summary>
    public sealed record InvalidDeliveryAddress(AddressError Error) : ValidationError;

    /// <summary>Un mesaj scurt, potrivit pentru afișare directă.</summary>
    public string ToMessage() => this switch
    {
        InvalidCustomerCode e => $"Cod de client invalid ({e.Raw ?? "<lipsă>"})",
        EmptyOrder => "Comanda nu conține niciun produs",
        InvalidProductCode e => $"Cod de produs invalid ({e.Raw ?? "<lipsă>"})",
        InvalidQuantity e => $"Cantitate invalidă pentru {e.ProductCode} ({DescribeQuantityError(e.Error)})",
        InvalidDeliveryAddress e => DescribeAddressError(e.Error),
        _ => throw new UnreachableException(),
    };

    private static string DescribeQuantityError(QuantityError error) => error switch
    {
        QuantityError.NotANumber e => $"nu este un număr: {e.Raw ?? "<lipsă>"}",
        QuantityError.OutOfRange e => $"în afara intervalului {Quantity.Minimum}-{Quantity.Maximum}: {e.Value.ToString(CultureInfo.InvariantCulture)}",
        _ => throw new UnreachableException(),
    };

    private static string DescribeAddressError(AddressError error) => error switch
    {
        AddressError.EmptyStreet => "Adresa de livrare: strada lipsește",
        AddressError.EmptyCity => "Adresa de livrare: orașul lipsește",
        AddressError.TooLong e => $"Adresa de livrare: prea lungă (maximum {e.MaxLength} de caractere)",
        AddressError.InvalidPostalCode e => $"Adresa de livrare: cod poștal invalid ({e.Raw ?? "<lipsă>"})",
        _ => throw new UnreachableException(),
    };
}
