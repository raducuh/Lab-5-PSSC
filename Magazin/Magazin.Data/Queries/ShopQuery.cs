using Microsoft.EntityFrameworkCore;

namespace Magazin.Data.Queries;

/// <inheritdoc cref="IShopQuery"/>
internal sealed class ShopQuery(ShoppingContext db) : IShopQuery
{
    public async Task<IReadOnlyList<ProductRow>> GetProductsAsync(CancellationToken cancellationToken) =>
        await db.Products
            .AsNoTracking()
            .OrderBy(product => product.Id)
            .Select(product => new ProductRow(product.Id, product.Code, product.Name, product.Price, product.QuantityType, product.Stock))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CustomerRow>> GetCustomersAsync(CancellationToken cancellationToken) =>
        await db.Customers
            .AsNoTracking()
            .OrderBy(customer => customer.Id)
            .Select(customer => new CustomerRow(customer.Id, customer.Code, customer.Name))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<OrderRow>> GetOrdersAsync(CancellationToken cancellationToken)
    {
        // Două interogări simple, legate în memorie: evită proiecția unei colecții imbricate într-un singur SELECT.
        var orders = await db.Orders
            .AsNoTracking()
            .OrderBy(order => order.Id)
            .Select(order => new { order.Id, order.Date, order.DeliveryAddress, CustomerCode = order.Customer.Code, CustomerName = order.Customer.Name })
            .ToListAsync(cancellationToken);

        var lines = await db.OrderItems
            .AsNoTracking()
            .OrderBy(item => item.Id)
            .Select(item => new
            {
                item.OrderId,
                item.Id,
                ProductCode = item.Product.Code,
                ProductName = item.Product.Name,
                item.Quantity,
                item.Product.QuantityType,
                LineTotal = item.Quantity * item.Product.Price,
            })
            .ToListAsync(cancellationToken);

        ILookup<int, OrderLineRow> linesByOrder = lines.ToLookup(
            line => line.OrderId,
            line => new OrderLineRow(line.Id, line.ProductCode, line.ProductName, line.Quantity, line.QuantityType, line.LineTotal));

        return
        [
            .. orders.Select(order => new OrderRow(
                order.Id,
                order.Date,
                order.DeliveryAddress,
                order.CustomerCode,
                order.CustomerName,
                linesByOrder[order.Id].Sum(line => line.LineTotal),
                [.. linesByOrder[order.Id]])),
        ];
    }
}
