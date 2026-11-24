using Magazin.Domain.Entities;
using Magazin.Domain.Errors;
using Magazin.Domain.States;
using Magazin.Domain.ValueObjects;
using Magazin.Functional;

namespace Magazin.Domain.Operations;

/// <summary>Verificarea existenței clientului, pe baza codului de client.</summary>
public static class CustomerVerification
{
    extension(Order.Validated order)
    {
        /// <summary>
        /// Caută codul clientului printre clienții <paramref name="customers"/> citiți din baza de date înainte de workflow.
        /// Mulțimea este un parametru (valoare), nu un serviciu: operația rămâne pură.
        /// </summary>
        public Result<Order.CustomerVerified, PlaceOrderError> CheckCustomerExists(IReadOnlyDictionary<CustomerCode, Customer> customers) =>
            customers.TryGetValue(order.CustomerCode, out Customer? customer)
                ? new Order.CustomerVerified(customer, order.Lines, order.DeliveryAddress)
                : new PlaceOrderError.UnknownCustomer(order.CustomerCode);
    }
}
