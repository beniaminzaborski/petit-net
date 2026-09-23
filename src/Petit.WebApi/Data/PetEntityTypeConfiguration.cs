using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Petit.WebApi.Data;

public class PetEntityTypeConfiguration : IEntityTypeConfiguration<Pet>
{
    public void Configure(EntityTypeBuilder<Pet> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Type)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Gender)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.Breed)
            .HasMaxLength(100);

        builder.Property(e => e.Weight)
            .HasPrecision(5, 2);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.OwnerId)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(e => e.CreatedAt)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.UpdatedAt)
            .ValueGeneratedOnUpdate();

        builder.HasIndex(e => e.OwnerId);

        builder.HasIndex(e => new { e.Name, e.Type });
    }
}
