using Magazin.Domain.Entities;

namespace Magazin.Domain.Repositories;

/// <summary>Un produs din catalog împreună cu stocul disponibil în acest moment, așa cum le citește <see cref="IProductsRepository"/>.</summary>
public sealed record ProductAvailability(Product Product, int Stock);
