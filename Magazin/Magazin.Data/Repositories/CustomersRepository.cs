using Magazin.Data.Entities;
using Magazin.Domain.Entities;
using Magazin.Domain.Repositories;
using Magazin.Domain.ValueObjects;
using Magazin.Functional;
using Microsoft.EntityFrameworkCore;

namespace Magazin.Data.Repositories;

/// <inheritdoc cref="ICustomersRepository"/>
public sealed class CustomersRepository(ShoppingContext db) : ICustomersRepository
{
    public async Task<IReadOnlyList<Customer>> GetByCodesAsync(IReadOnlyCollection<CustomerCode> codes, CancellationToken cancellationToken)
    {
        if (codes.Count == 0)
        {
            return [];
        }

        string[] values = [.. codes.Select(code => code.Value)];

        List<CustomerEntity> rows = await db.Customers
            .AsNoTracking()
            .Where(customer => values.Contains(customer.Code))
            .ToListAsync(cancellationToken);

        // Baza de date proprie e de încredere: dacă un rând nu respectă formatul, e o coruptere a datelor,
        // nu o greșeală a utilizatorului — se aruncă direct, nu se transformă într-o eroare de domeniu.
        return
        [
            .. rows.Select(row => new Customer(
                CustomerCode.Create(row.Code).GetValueOrThrow(_ => new InvalidDataException($"Cod de client invalid în baza de date: '{row.Code}'.")),
                row.Name)),
        ];
    }
}
