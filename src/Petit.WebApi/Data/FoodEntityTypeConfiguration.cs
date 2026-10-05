using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Petit.WebApi.Data;

public class FoodEntityTypeConfiguration : IEntityTypeConfiguration<Food>
{
    public void Configure(EntityTypeBuilder<Food> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Producer)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Type)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.PetType)
            .HasMaxLength(20);

        builder.Property(e => e.CaloriesPer100g);

        builder.Property(e => e.ServingSize);

        builder.Property(e => e.SubCategory)
            .HasMaxLength(100);

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

        builder.HasIndex(e => new { e.Name, e.Producer });
    }
}
