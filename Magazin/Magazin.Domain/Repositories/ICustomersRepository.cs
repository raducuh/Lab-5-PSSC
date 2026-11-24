using Magazin.Domain.Entities;
using Magazin.Domain.ValueObjects;

namespace Magazin.Domain.Repositories;

/// <summary>Port prin care workflow-ul află care dintre codurile de client primite aparțin unor clienți existenți.</summary>
public interface ICustomersRepository
{
    /// <summary>Dintre codurile date, întoarce clienții care există în baza de date (cei inexistenți lipsesc din rezultat).</summary>
    Task<IReadOnlyList<Customer>> GetByCodesAsync(IReadOnlyCollection<CustomerCode> codes, CancellationToken cancellationToken);
}
