using Magazin.Domain.Entities;
using Magazin.Domain.States;
using Magazin.Domain.ValueObjects;

namespace Magazin.Domain.Events;

/// <summary>
/// Evenimentul de domeniu produs de plasarea cu succes a unei comenzi: descrie un fapt petrecut. <see cref="OrderId"/>
/// este identificatorul dat de baza de date la salvare, deci evenimentul există doar după ce comanda a fost salvată.
/// Nu există un eveniment de eșec: un eșec nu s-a întâmplat, deci nu este un eveniment — este o eroare
/// (<see cref="Errors.PlaceOrderError"/>), transportată de <see cref="Functional.Result{TSuccess, TFailure}"/>.
/// </summary>
public sealed record OrderPlacedEvent(
    int OrderId,
    Customer Customer,
    IReadOnlyList<PricedOrderLine> Lines,
    Address DeliveryAddress,
    Money Total,
    DateTimeOffset PlacedAt);
