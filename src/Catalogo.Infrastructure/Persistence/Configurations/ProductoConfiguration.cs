using Catalogo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalogo.Infrastructure.Persistence.Configurations;

public class ProductoConfiguration : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> builder)
    {
        builder.ToTable("Productos");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.Nombre)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(p => p.Descripcion)
            .HasMaxLength(500);

        builder.Property(p => p.Stock)
            .IsRequired();

        builder.Property(p => p.Activo)
            .IsRequired();

        builder.Property(p => p.FechaCreacion)
            .IsRequired();

        builder.Property(p => p.CategoriaId)
            .IsRequired();

        builder.ComplexProperty(p => p.Precio, precio =>
        {
            precio.Property(p => p.Valor)
                .HasColumnName("Precio")
                .HasPrecision(18, 2)
                .IsRequired();

            precio.Property(p => p.Moneda)
                .HasColumnName("Moneda")
                .HasMaxLength(3)
                .IsRequired();
        });
    }
}