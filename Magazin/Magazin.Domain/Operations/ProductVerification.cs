using Magazin.Domain.Entities;
using Magazin.Domain.Errors;
using Magazin.Domain.States;
using Magazin.Domain.ValueObjects;
using Magazin.Functional;

namespace Magazin.Domain.Operations;

/// <summary>Verificarea existenței produselor în catalog, pe baza codului de produs.</summary>
public static class ProductVerification
{
    extension(Order.CustomerVerified order)
    {
        /// <summary>
        /// Caută fiecare cod în <paramref name="catalog"/> (produsele citite din baza de date înainte de workflow). Catalogul este un parametru (valoare), nu un serviciu:
        /// operația rămâne pură. Dacă lipsesc produse, le raportează pe toate, o singură dată fiecare.
        /// </summary>
        public Result<Order.ProductsVerified, PlaceOrderError> CheckProductsExist(IReadOnlyDictionary<ProductCode, Product> catalog)
        {
            IReadOnlyList<ProductCode> unknown =
            [
                .. order.Lines
                    .Select(line => line.ProductCode)
                    .Where(code => !catalog.ContainsKey(code))
                    .Distinct(),
            ];

            return unknown.Count == 0
                ? new Order.ProductsVerified(
                    order.Customer,
                    [.. order.Lines.Select(line => new ProductLine(catalog[line.ProductCode], line.Quantity))],
                    order.DeliveryAddress)
                : new PlaceOrderError.UnknownProducts(unknown);
        }
    }
}
