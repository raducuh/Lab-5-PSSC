using Magazin.Domain.States;

namespace Magazin.Domain.Repositories;

/// <summary>Port prin care workflow-ul persistă rezultatul plasării unei comenzi.</summary>
public interface IOrdersRepository
{
    /// <summary>
    /// Salvează comanda, liniile ei și scăderea stocului, într-o singură tranzacție. Întoarce identificatorul
    /// atribuit comenzii de baza de date.
    /// </summary>
    Task<int> SaveAsync(Order.Placed order, CancellationToken cancellationToken);
}
