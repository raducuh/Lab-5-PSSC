using Magazin.Domain.Errors;
using Magazin.Domain.States;
using Magazin.Domain.ValueObjects;
using Magazin.Functional;

namespace Magazin.Domain.Operations;

/// <summary>Verificarea stocului pentru produsele deja găsite în catalog.</summary>
public static class StockVerification
{
    extension(Order.ProductsVerified order)
    {
        /// <summary>
        /// Compară cantitatea totală cerută pentru fiecare produs (liniile cu același produs se adună) cu stocul din
        /// <paramref name="stock"/> (citit din baza de date înainte de workflow); un produs care lipsește din dicționar are stocul zero. Raportează toate lipsurile.
        /// </summary>
        public Result<Order.StockVerified, PlaceOrderError> CheckStock(IReadOnlyDictionary<ProductCode, int> stock)
        {
            IReadOnlyList<StockShortage> shortages =
            [
                .. order.Lines
                    .GroupBy(line => line.Product.Code)
                    .Select(group => new StockShortage(group.Key, group.Sum(line => line.Quantity.Value), stock.GetValueOrDefault(group.Key)))
                    .Where(shortage => shortage.Requested > shortage.Available),
            ];

            return shortages.Count == 0
                ? new Order.StockVerified(order.Customer, order.Lines, order.DeliveryAddress)
                : new PlaceOrderError.InsufficientStock(shortages);
        }
    }
}
