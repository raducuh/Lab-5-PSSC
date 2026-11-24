using Magazin.Domain.ValueObjects;

namespace Magazin.Domain.Repositories;

/// <summary>Port prin care workflow-ul citește produsele (cu preț) și stocul lor curent din baza de date.</summary>
public interface IProductsRepository
{
    /// <summary>Dintre codurile date, întoarce produsele care există în baza de date, cu stocul lor (cele inexistente lipsesc din rezultat).</summary>
    Task<IReadOnlyList<ProductAvailability>> GetAvailabilityAsync(IReadOnlyCollection<ProductCode> codes, CancellationToken cancellationToken);
}
