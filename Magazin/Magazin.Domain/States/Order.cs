using Magazin.Domain.Entities;
using Magazin.Domain.ValueObjects;

namespace Magazin.Domain.States;

/// <summary>
/// Tipul-sumă care descrie stările prin care trece o comandă în workflow-ul de plasare:
/// <see cref="Unvalidated"/> → <see cref="Validated"/> → <see cref="CustomerVerified"/> → <see cref="ProductsVerified"/>
/// → <see cref="StockVerified"/> → <see cref="AddressVerified"/> → <see cref="Priced"/> → <see cref="Placed"/>.
/// Ierarhia este închisă (constructor privat, înregistrări imbricate <see langword="sealed"/>). Fiecare operație din
/// <c>Operations/</c> este definită doar pe starea pe care o transformă, deci un pipeline ordonat greșit nu compilează.
/// Nu există o stare „Invalid”: un eșec nu este o stare a comenzii, ci o eroare transportată de
/// <see cref="Functional.Result{TSuccess, TFailure}"/>. Stările nu conțin detalii de persistență (chei, indicatori „murdari”).
/// </summary>
public abstract record Order
{
    private Order()
    {
    }

    /// <summary>Datele comenzii așa cum au fost primite, înainte de validare.</summary>
    public sealed record Unvalidated(string? CustomerCode, IReadOnlyList<UnvalidatedOrderLine> Lines, UnvalidatedAddress DeliveryAddress) : Order;

    /// <summary>Codul clientului, codurile, cantitățile și adresa sunt convertite în obiecte-valoare; nu s-a verificat nimic față de baza de date.</summary>
    public sealed record Validated(CustomerCode CustomerCode, IReadOnlyList<ValidatedOrderLine> Lines, Address DeliveryAddress) : Order;

    /// <summary>Clientul există în baza de date (acum avem entitatea, nu doar codul).</summary>
    public sealed record CustomerVerified(Customer Customer, IReadOnlyList<ValidatedOrderLine> Lines, Address DeliveryAddress) : Order;

    /// <summary>Fiecare cod de produs a fost găsit în catalog.</summary>
    public sealed record ProductsVerified(Customer Customer, IReadOnlyList<ProductLine> Lines, Address DeliveryAddress) : Order;

    /// <summary>Stocul din baza de date acoperă cantitățile cerute pentru fiecare produs.</summary>
    public sealed record StockVerified(Customer Customer, IReadOnlyList<ProductLine> Lines, Address DeliveryAddress) : Order;

    /// <summary>Adresa de livrare se află într-o zonă acoperită de magazin.</summary>
    public sealed record AddressVerified(Customer Customer, IReadOnlyList<ProductLine> Lines, Address DeliveryAddress) : Order;

    /// <summary>Prețul fiecărei linii și totalul comenzii sunt calculate.</summary>
    public sealed record Priced(Customer Customer, IReadOnlyList<PricedOrderLine> Lines, Address DeliveryAddress, Money Total) : Order;

    /// <summary>Starea finală a logicii pure: comanda cu momentul plasării, gata de salvat. Identificatorul îl dă baza de date la salvare.</summary>
    public sealed record Placed(Customer Customer, IReadOnlyList<PricedOrderLine> Lines, Address DeliveryAddress, Money Total, DateTimeOffset PlacedAt) : Order;
}
