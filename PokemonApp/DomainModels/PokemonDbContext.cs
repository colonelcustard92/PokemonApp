
using Microsoft.EntityFrameworkCore;
using PokemonApp.DomainModels;

public class PokemonDbContext : DbContext
{
    public PokemonDbContext(DbContextOptions<PokemonDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; }
}
