using Magazin.Domain.Errors;
using Magazin.Domain.States;
using Magazin.Functional;

namespace Magazin.Domain.Operations;

/// <summary>Verificarea adresei de livrare față de zonele acoperite de magazin.</summary>
public static class DeliveryAddressVerification
{
    extension(Order.StockVerified order)
    {
        /// <summary>
        /// Adresa este acceptată dacă orașul ei se află în <paramref name="deliverableCities"/>. Cine construiește mulțimea
        /// alege comparatorul (de exemplu <see cref="StringComparer.OrdinalIgnoreCase"/>), deci operația nu depinde de cultură.
        /// </summary>
        public Result<Order.AddressVerified, PlaceOrderError> CheckDeliveryAddress(IReadOnlySet<string> deliverableCities) =>
            deliverableCities.Contains(order.DeliveryAddress.City)
                ? new Order.AddressVerified(order.Customer, order.Lines, order.DeliveryAddress)
                : new PlaceOrderError.UndeliverableAddress(order.DeliveryAddress);
    }
}
