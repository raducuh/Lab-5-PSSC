using Magazin.Data.Entities;
using Magazin.Domain.Entities;
using Magazin.Domain.Repositories;
using Magazin.Domain.ValueObjects;
using Magazin.Functional;
using Microsoft.EntityFrameworkCore;

namespace Magazin.Data.Repositories;

/// <inheritdoc cref="IProductsRepository"/>
public sealed class ProductsRepository(ShoppingContext db) : IProductsRepository
{
    public async Task<IReadOnlyList<ProductAvailability>> GetAvailabilityAsync(IReadOnlyCollection<ProductCode> codes, CancellationToken cancellationToken)
    {
        if (codes.Count == 0)
        {
            return [];
        }

        string[] values = [.. codes.Select(code => code.Value)];

        List<ProductEntity> rows = await db.Products
            .AsNoTracking()
            .Where(product => values.Contains(product.Code))
            .ToListAsync(cancellationToken);

        return [.. rows.Select(ToAvailability)];
    }

    // Rândurile propriei baze de date sunt de încredere: un cod sau un preț care încalcă regulile domeniului este
    // corupție a datelor, nu eroare de utilizator, deci se aruncă InvalidDataException.
    private static ProductAvailability ToAvailability(ProductEntity row) => new(
        new Product(
            ProductCode.Create(row.Code).GetValueOrThrow(_ => new InvalidDataException($"Cod de produs invalid în baza de date: '{row.Code}'.")),
            row.Name,
            Money.Create(row.Price).GetValueOrThrow(_ => new InvalidDataException($"Preț invalid în baza de date pentru produsul '{row.Code}'."))),
        row.Stock);
}
