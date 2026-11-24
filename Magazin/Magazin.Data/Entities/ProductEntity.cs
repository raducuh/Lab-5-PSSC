namespace Magazin.Data.Entities;

/// <summary>Rândul din tabela <c>Products</c> (vezi <c>SQL/create-magazin-db.sql</c>).</summary>
public sealed class ProductEntity
{
    public int Id { get; set; }

    public required string Code { get; set; }

    public required string Name { get; set; }

    public decimal Price { get; set; }

    /// <summary>Unitatea de măsură în care se comandă produsul (buc, kg, l ...).</summary>
    public required string QuantityType { get; set; }

    /// <summary>Cantitatea disponibilă în stoc (coloană adăugată în Lucrarea 5, vezi <c>SQL/create-magazin-db.sql</c>).</summary>
    public int Stock { get; set; }
}
