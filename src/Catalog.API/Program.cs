using Catalog.API.Errors;
using Catalog.Application;
using Catalog.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("CatalogDb")
    ?? builder.Configuration.GetConnectionString("CatalogoDb")
    ?? throw new InvalidOperationException("Missing connection string 'CatalogDb'.");

// Composition root: wire up application layers
builder.Services
    .AddApplication()
    .AddInfrastructure(connectionString);

builder.Services.AddControllers(options =>
{
    // Required property constraints are validated by the domain, not ASP.NET
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    await app.Services.ApplyMigrationsAsync();
}

app.MapControllers();

app.Run();
