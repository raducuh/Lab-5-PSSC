namespace Magazin.Data.Entities;

/// <summary>Rândul din tabela <c>OrderItem</c> (vezi <c>SQL/create-magazin-db.sql</c>): o linie dintr-o comandă.</summary>
public sealed class OrderItemEntity
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public OrderEntity Order { get; set; } = null!;

    public int ProductId { get; set; }

    public ProductEntity Product { get; set; } = null!;

    public int Quantity { get; set; }
}
