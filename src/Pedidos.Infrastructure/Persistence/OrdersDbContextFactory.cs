
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Pedidos.Infrastructure.Persistence;

public class OrdersDbContextFactory
    : IDesignTimeDbContextFactory<OrdersDbContext>
{
    public OrdersDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__OrdersDb")
            ?? "Host=localhost;Port=5435;Database=orders;Username=orders;Password=orders123";

        var optionsBuilder = new DbContextOptionsBuilder<OrdersDbContext>();

        optionsBuilder.UseNpgsql(connectionString);

        return new OrdersDbContext(optionsBuilder.Options);
    }
}
