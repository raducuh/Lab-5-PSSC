namespace Magazin.Data.Entities;

/// <summary>Rândul din tabela <c>Customers</c> (vezi <c>SQL/create-magazin-db.sql</c>).</summary>
public sealed class CustomerEntity
{
    public int Id { get; set; }

    public required string Code { get; set; }

    public required string Name { get; set; }

    public ICollection<OrderEntity> Orders { get; } = [];
}
