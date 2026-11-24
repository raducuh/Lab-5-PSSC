namespace Magazin.Data.Queries;

/// <summary>Model de citire pentru afișarea conținutului bazei de date (produse, clienți, comenzi), separat de porturile domeniului.</summary>
public interface IShopQuery
{
    Task<IReadOnlyList<ProductRow>> GetProductsAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<CustomerRow>> GetCustomersAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<OrderRow>> GetOrdersAsync(CancellationToken cancellationToken);
}
