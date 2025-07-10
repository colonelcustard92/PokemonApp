using Microsoft.EntityFrameworkCore;

namespace PokemonApp.DomainModels;

public class PokemonDbContext : DbContext
{
    public DbSet<User> Users { get; set; }

    
    public PokemonDbContext()
    {
    }

  
    public PokemonDbContext(DbContextOptions<PokemonDbContext> options)
        : base(options)
    {
    }

   
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=pokedex.db");

        }
    }
}