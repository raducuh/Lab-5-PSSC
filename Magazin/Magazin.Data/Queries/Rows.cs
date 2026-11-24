namespace Magazin.Data.Queries;

/// <summary>Un produs, pentru citire directă (fără trecere prin domeniu).</summary>
public sealed record ProductRow(int Id, string Code, string Name, decimal Price, string QuantityType, int Stock);

/// <summary>Un client, pentru citire directă.</summary>
public sealed record CustomerRow(int Id, string Code, string Name);

/// <summary>O linie de comandă; <paramref name="LineTotal"/> folosește prețul curent al produsului (schema nu păstrează prețul de la momentul comenzii).</summary>
public sealed record OrderLineRow(int Id, string ProductCode, string ProductName, int Quantity, string QuantityType, decimal LineTotal);

/// <summary>O comandă cu liniile ei și totalul calculat din ele.</summary>
public sealed record OrderRow(
    int Id,
    DateTime Date,
    string DeliveryAddress,
    string CustomerCode,
    string CustomerName,
    decimal Total,
    IReadOnlyList<OrderLineRow> Lines);
