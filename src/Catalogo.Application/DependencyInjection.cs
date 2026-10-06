using Microsoft.Extensions.DependencyInjection;

namespace Catalogo.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Registra el Mediator y todos los handlers (commands y queries) de este proyecto
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        return services;
    }
}
