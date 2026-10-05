using Microsoft.EntityFrameworkCore;

namespace Petit.WebApi.Data;

public class PetitDbContext(DbContextOptions<PetitDbContext> options) : DbContext(options)
{
    public DbSet<Pet> Pets => Set<Pet>();
    public DbSet<Food> Foods => Set<Food>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new PetEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new FoodEntityTypeConfiguration());
    }
}
