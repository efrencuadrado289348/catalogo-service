using Catalog.API.Errors;
using Catalog.API.Security;
using Catalog.Application;
using Catalog.Infrastructure;
using Microsoft.OpenApi;

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

// Security: validate JWTs issued by Identity.API
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pegar el accessToken obtenido en Identity (POST /api/auth/login) sin el prefijo 'Bearer'."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    await app.Services.ApplyMigrationsAsync();
}

app.UseAuthentication(); // who are you? (401)
app.UseAuthorization();  // are you allowed? (403)

app.MapControllers();

app.Run();
