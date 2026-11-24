using Magazin.Domain.Entities;
using Magazin.Domain.ValueObjects;

namespace Magazin.Domain.States;

/// <summary>O linie de comandă după conversia în obiecte-valoare: cod și cantitate valabile prin construcție.</summary>
public sealed record ValidatedOrderLine(ProductCode ProductCode, Quantity Quantity);

/// <summary>O linie de comandă al cărei produs a fost găsit în catalog (deci cunoaște numele și prețul unitar).</summary>
public sealed record ProductLine(Product Product, Quantity Quantity);

/// <summary>O linie de comandă cu prețul calculat: preț unitar × cantitate.</summary>
public sealed record PricedOrderLine(Product Product, Quantity Quantity, Money LineTotal);
