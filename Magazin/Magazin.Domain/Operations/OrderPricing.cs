using Magazin.Domain.States;
using Magazin.Domain.ValueObjects;

namespace Magazin.Domain.Operations;

/// <summary>Calcularea prețului comenzii.</summary>
public static class OrderPricing
{
    extension(Order.AddressVerified order)
    {
        /// <summary>
        /// Prețul fiecărei linii este prețul unitar înmulțit cu cantitatea, iar totalul este suma liniilor.
        /// Funcția este totală: orice <see cref="Order.AddressVerified"/> produce un <see cref="Order.Priced"/>, nu are cum eșua.
        /// </summary>
        public Order.Priced CalculatePrice()
        {
            IReadOnlyList<PricedOrderLine> lines =
                [.. order.Lines.Select(line => new PricedOrderLine(line.Product, line.Quantity, line.Product.UnitPrice.Times(line.Quantity)))];

            return new Order.Priced(order.Customer, lines, order.DeliveryAddress, Money.Sum(lines.Select(line => line.LineTotal)));
        }
    }
}
