using System.Diagnostics;
using Magazin.Domain.Commands;
using Magazin.Domain.Entities;
using Magazin.Domain.Errors;
using Magazin.Domain.Events;
using Magazin.Domain.Operations;
using Magazin.Domain.Repositories;
using Magazin.Domain.States;
using Magazin.Domain.ValueObjects;
using Magazin.Functional;

namespace Magazin.Domain.Workflows;

/// <summary>
/// Workflow-ul de plasare a unei comenzi. Urmează structura „impur → pur → impur” (Wlaschin cap. 12): citește din
/// baza de date starea necesară (clientul, produsele, stocul), execută logica de afaceri ca o singură funcție pură
/// (<see cref="Place"/>), apoi salvează rezultatul dacă a reușit. Nu prinde excepții: o excepție aici este o eroare de
/// infrastructură, nu un eșec de validare, și se propagă către marginea aplicației (consolă, API).
/// </summary>
public sealed class PlaceOrderWorkflow(
    ICustomersRepository customersRepository,
    IProductsRepository productsRepository,
    IOrdersRepository ordersRepository,
    DeliveryZone deliveryZone,
    TimeProvider clock)
{
    /// <summary>
    /// Execută workflow-ul complet: încarcă starea din baza de date, plasează comanda (logică pură), apoi o salvează
    /// pe ramura de succes și întoarce evenimentul cu identificatorul dat de baza de date.
    /// </summary>
    public async Task<Result<OrderPlacedEvent, PlaceOrderError>> ExecuteAsync(PlaceOrderCommand command, CancellationToken cancellationToken)
    {
        StoreSnapshot store = await LoadStoreAsync(command, cancellationToken);

        Result<Order.Placed, PlaceOrderError> placed = Place(command, store, clock.GetUtcNow());

        switch (placed)
        {
            case Result<Order.Placed, PlaceOrderError>.Ok ok:
                int orderId = await ordersRepository.SaveAsync(ok.Value, cancellationToken);
                return new Result<OrderPlacedEvent, PlaceOrderError>.Ok(ok.Value.ToEvent(orderId));
            case Result<Order.Placed, PlaceOrderError>.Error error:
                return new Result<OrderPlacedEvent, PlaceOrderError>.Error(error.Value);
            default:
                throw new UnreachableException();
        }
    }

    /// <summary>
    /// Nucleul pur al workflow-ului: validare → client → produse → stoc → adresă → preț → comandă plasată, compuse cu
    /// <c>Bind</c>/<c>Map</c>. Nu are efecte secundare, deci este direct testabilă fără repository-uri sau bază de date.
    /// </summary>
    public static Result<Order.Placed, PlaceOrderError> Place(PlaceOrderCommand command, StoreSnapshot store, DateTimeOffset now) =>
        new Order.Unvalidated(command.CustomerCode, command.Lines, command.DeliveryAddress)
            .Validate()
            // Cast explicit la PlaceOrderError (tipul de bază): MapError deduce tipul din corpul lambdei, altfel ar
            // deduce tipul concret derivat (Validation), iar Bind-ul următor nu s-ar potrivi.
            .MapError(errors => (PlaceOrderError)new PlaceOrderError.Validation(errors))
            .Bind(validated => validated.CheckCustomerExists(store.Customers))
            .Bind(customerVerified => customerVerified.CheckProductsExist(store.Catalog))
            .Bind(productsVerified => productsVerified.CheckStock(store.Stock))
            .Bind(stockVerified => stockVerified.CheckDeliveryAddress(store.DeliverableCities))
            .Map(addressVerified => addressVerified.CalculatePrice())
            .Map(priced => priced.Place(now));

    // Partea impură de la început: cere bazei de date doar ce are nevoie această comandă. Codurile care nu au formatul
    // corect nu se caută (le raportează validarea); codurile care nu există în baza de date lipsesc pur și simplu din
    // rezultat, iar operațiile pure le raportează ca eroare de domeniu.
    private async Task<StoreSnapshot> LoadStoreAsync(PlaceOrderCommand command, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<CustomerCode> customerCodes =
            CustomerCode.Create(command.CustomerCode) is Result<CustomerCode, InvalidCustomerCodeFormat>.Ok customerCode
                ? [customerCode.Value]
                : [];

        IReadOnlyCollection<ProductCode> productCodes =
        [
            .. command.Lines
                .Select(line => ProductCode.Create(line.ProductCode))
                .OfType<Result<ProductCode, InvalidProductCodeFormat>.Ok>()
                .Select(ok => ok.Value)
                .Distinct(),
        ];

        IReadOnlyList<Customer> customers = await customersRepository.GetByCodesAsync(customerCodes, cancellationToken);
        IReadOnlyList<ProductAvailability> products = await productsRepository.GetAvailabilityAsync(productCodes, cancellationToken);

        return new StoreSnapshot(
            customers.ToDictionary(customer => customer.Code),
            products.ToDictionary(item => item.Product.Code, item => item.Product),
            products.ToDictionary(item => item.Product.Code, item => item.Stock),
            deliveryZone.Cities);
    }
}
