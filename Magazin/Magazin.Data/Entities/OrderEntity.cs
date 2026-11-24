namespace Magazin.Data.Entities;

/// <summary>Rândul din tabela <c>Order</c> (vezi <c>SQL/create-magazin-db.sql</c>).</summary>
public sealed class OrderEntity
{
    public int Id { get; set; }

    /// <summary>Momentul plasării comenzii (UTC).</summary>
    public DateTime Date { get; set; }

    public required string DeliveryAddress { get; set; }

    public int CustomerId { get; set; }

    public CustomerEntity Customer { get; set; } = null!;

    public ICollection<OrderItemEntity> Items { get; } = [];
}
