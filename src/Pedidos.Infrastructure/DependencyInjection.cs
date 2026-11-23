
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pedidos.Application.Interfaces;
using Pedidos.Infrastructure.ExternalServices;
using Pedidos.Infrastructure.Persistence;
using Pedidos.Infrastructure.Repositories;

namespace Pedidos.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString,
        string catalogBaseUrl)
    {
        services.AddDbContext<OrdersDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IOrderRepository, OrderRepository>();

        // Catalog requires a JWT: forward the one received by Pedidos
        services.AddHttpContextAccessor();
        services.AddTransient<ForwardAuthorizationHandler>();

        services.AddHttpClient<IProductCatalogService, ProductCatalogService>(
            client =>
            {
                client.BaseAddress = new Uri(catalogBaseUrl);
            })
            .AddHttpMessageHandler<ForwardAuthorizationHandler>();

        return services;
    }
}
