using Magazin.Data.Queries;
using Magazin.Data.Repositories;
using Magazin.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Magazin.Data;

/// <summary>Înregistrează <see cref="ShoppingContext"/> și adaptoarele care implementează porturile domeniului.</summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddShopData(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ShoppingContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<ICustomersRepository, CustomersRepository>();
        services.AddScoped<IProductsRepository, ProductsRepository>();
        services.AddScoped<IOrdersRepository, OrdersRepository>();
        services.AddScoped<IShopQuery, ShopQuery>();
        return services;
    }
}
