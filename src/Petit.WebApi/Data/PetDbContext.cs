using Microsoft.EntityFrameworkCore;

namespace Petit.WebApi.Data;

public class PetDbContext(DbContextOptions<PetDbContext> options) : DbContext(options)
{
    public DbSet<Pet> Pets => Set<Pet>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new PetEntityTypeConfiguration());
    }
}
