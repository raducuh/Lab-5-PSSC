using System.Diagnostics;
using System.Globalization;
using Magazin.Domain.ValueObjects;

namespace Magazin.Domain.Errors;

/// <summary>Cantitatea cerută pentru un produs depășește stocul disponibil.</summary>
public sealed record StockShortage(ProductCode ProductCode, int Requested, int Available);

/// <summary>Motivul pentru care workflow-ul de plasare a unei comenzi nu a produs un eveniment.</summary>
public abstract record PlaceOrderError
{
    private PlaceOrderError()
    {
    }

    /// <summary>Datele brute nu au putut fi convertite în obiecte-valoare.</summary>
    public sealed record Validation(IReadOnlyList<ValidationError> Errors) : PlaceOrderError;

    /// <summary>Codul clientului are formatul corect, dar nu există în baza de date.</summary>
    public sealed record UnknownCustomer(CustomerCode Code) : PlaceOrderError;

    /// <summary>Unul sau mai multe coduri de produs nu există în catalog.</summary>
    public sealed record UnknownProducts(IReadOnlyList<ProductCode> Codes) : PlaceOrderError;

    /// <summary>Stocul nu acoperă cantitățile cerute pentru unul sau mai multe produse.</summary>
    public sealed record InsufficientStock(IReadOnlyList<StockShortage> Shortages) : PlaceOrderError;

    /// <summary>Magazinul nu livrează la adresa dată.</summary>
    public sealed record UndeliverableAddress(Address Address) : PlaceOrderError;

    /// <summary>Mesajele tuturor erorilor, potrivite pentru afișare (consolă sau răspuns HTTP).</summary>
    public IReadOnlyList<string> ToMessages() => this switch
    {
        Validation e => [.. e.Errors.Select(error => error.ToMessage())],
        UnknownCustomer e => [$"Client inexistent ({e.Code})"],
        UnknownProducts e => [.. e.Codes.Select(code => $"Produs inexistent în catalog ({code})")],
        InsufficientStock e => [.. e.Shortages.Select(s => $"Stoc insuficient pentru {s.ProductCode}: cerut {s.Requested.ToString(CultureInfo.InvariantCulture)}, disponibil {s.Available.ToString(CultureInfo.InvariantCulture)}")],
        UndeliverableAddress e => [$"Nu livrăm la adresa {e.Address}"],
        _ => throw new UnreachableException(),
    };

    // "sealed": fără el, fiecare înregistrare derivată își sintetizează propriul ToString() care ascunde acest override.
    public sealed override string ToString() => string.Join(Environment.NewLine, ToMessages());
}
