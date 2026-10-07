using Catalogo.API.Errors;
using Catalogo.Application;
using Catalogo.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("CatalogoDb")
    ?? throw new InvalidOperationException("Falta la cadena de conexión 'CatalogoDb'.");

// Composition root: aquí se conectan las capas
builder.Services
    .AddApplication()
    .AddInfrastructure(connectionString);

builder.Services.AddControllers(options =>
{
    // Las reglas de obligatoriedad las valida el dominio, no ASP.NET
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
    await app.Services.AplicarMigracionesAsync();
}

app.MapControllers();

app.Run();
