using Magazin.Domain.States;

namespace Magazin.Domain.Commands;

/// <summary>
/// Comanda care pornește workflow-ul de plasare a unei comenzi: intenția clientului, cu date brute.
/// Clientul este identificat prin cod (text), nu prin entitate: existența lui o verifică workflow-ul în baza de date.
/// </summary>
public sealed record PlaceOrderCommand(string? CustomerCode, IReadOnlyList<UnvalidatedOrderLine> Lines, UnvalidatedAddress DeliveryAddress);
