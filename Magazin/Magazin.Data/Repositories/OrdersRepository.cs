using Magazin.Data.Entities;
using Magazin.Domain.Repositories;
using Magazin.Domain.States;
using Microsoft.EntityFrameworkCore;

namespace Magazin.Data.Repositories;

/// <inheritdoc cref="IOrdersRepository"/>
public sealed class OrdersRepository(ShoppingContext db) : IOrdersRepository
{
    public async Task<int> SaveAsync(Order.Placed order, CancellationToken cancellationToken)
    {
        string customerCode = order.Customer.Code.Value;
        string[] productCodes = [.. order.Lines.Select(line => line.Product.Code.Value).Distinct()];

        // Clientul și produsele sunt urmărite de context: produsele se modifică (scade stocul), iar clientul se leagă de comandă.
        // Workflow-ul a verificat deja că există; dacă totuși lipsesc (ștergere concurentă), e o eroare de infrastructură.
        CustomerEntity customer = await db.Customers.SingleAsync(row => row.Code == customerCode, cancellationToken);

        Dictionary<string, ProductEntity> products = (await db.Products
                .Where(row => productCodes.Contains(row.Code))
                .ToListAsync(cancellationToken))
            .ToDictionary(row => row.Code, StringComparer.OrdinalIgnoreCase);

        OrderEntity newOrder = new()
        {
            Date = order.PlacedAt.UtcDateTime,
            DeliveryAddress = order.DeliveryAddress.ToString(),
            Customer = customer,
        };

        foreach (PricedOrderLine line in order.Lines)
        {
            if (!products.TryGetValue(line.Product.Code.Value, out ProductEntity? product))
            {
                throw new InvalidOperationException($"Produsul {line.Product.Code} verificat anterior nu mai există în baza de date.");
            }

            product.Stock -= line.Quantity.Value;
            if (product.Stock < 0)
            {
                throw new InvalidOperationException($"Stocul produsului {line.Product.Code} s-a epuizat între verificare și salvare.");
            }

            newOrder.Items.Add(new OrderItemEntity { Product = product, Quantity = line.Quantity.Value });
        }

        // Un singur SaveChangesAsync = o singură tranzacție: comanda, liniile ei și scăderea stocului se salvează împreună sau deloc.
        db.Orders.Add(newOrder);
        await db.SaveChangesAsync(cancellationToken);

        return newOrder.Id;
    }
}
