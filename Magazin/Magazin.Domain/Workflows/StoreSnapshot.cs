using Magazin.Domain.Entities;
using Magazin.Domain.ValueObjects;

namespace Magazin.Domain.Workflows;

/// <summary>
/// Tot ce logica pură trebuie să știe despre magazin, ca <b>valori</b> (nu servicii): clienții și produsele căutate,
/// stocul lor și orașele în care se livrează. <see cref="PlaceOrderWorkflow.ExecuteAsync"/> le citește din baza de date
/// (și din configurație) înainte de a apela <see cref="PlaceOrderWorkflow.Place"/>.
/// </summary>
public sealed record StoreSnapshot(
    IReadOnlyDictionary<CustomerCode, Customer> Customers,
    IReadOnlyDictionary<ProductCode, Product> Catalog,
    IReadOnlyDictionary<ProductCode, int> Stock,
    IReadOnlySet<string> DeliverableCities);
